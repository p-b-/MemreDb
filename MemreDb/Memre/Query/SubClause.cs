using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre.Ancillary;
using MemreDb.Memre;
using MemreDb.Memre.Indices;
using MemreDb.Memre.Indices.Iterators;

namespace MemreDb.Memre
{
    internal class SubClause
    {
        class SubClauseData
        {
            internal string ComparisonTable { get; set; }
            internal string MemberToCompare { get; set; }
            internal Object LiteralToCompare { get; set; }

            internal bool IsLiteralNotVariable()
            {
                return LiteralToCompare != null;
            }

            internal SubClauseData()
            {
                ComparisonTable = string.Empty;
                MemberToCompare = string.Empty;
                LiteralToCompare = null;
            }

        }
        SubClauseType _subClauseType;

        SubClauseData _lhsData;
        SubClauseData _rhsData;

        SubClause _leftSubClause;
        SubClause _rightSubClause;
        SubClauseComparitor _clauseComparitor;

        Query _parentQuery;

        internal SubClause(Query parentQuery)
        {
            this._parentQuery = parentQuery;
            this._subClauseType = SubClauseType.EndNode;
            this._lhsData = null;
            this._rhsData = null;
            this._leftSubClause = null;
            this._rightSubClause = null;
        }

        internal ITreeIndexInternals Execute()
        {
            switch (this._subClauseType)
            {
                case SubClauseType.And: return ExecuteAndClause();
                case SubClauseType.Or: return ExecuteOrClause();
                case SubClauseType.EndNode: return ExecuteEndClause();
                default:
                    throw new Exception("Unknown sub clause type in WHERE statement");
            }
        }

        private ITreeIndexInternals ExecuteAndClause()
        {
            ITreeIndexInternals lhsIndex = this._leftSubClause.Execute();
            ITreeIndexInternals rhsIndex = this._rightSubClause.Execute();
            ITreeIndexInternals mergedIndex = lhsIndex.SetOperation(rhsIndex, SetOperation.And) as TreeIndex;
            return mergedIndex;
        }

        private ITreeIndexInternals ExecuteOrClause()
        {
            ITreeIndexInternals lhsIndex = this._leftSubClause.Execute();
            ITreeIndexInternals rhsIndex = this._rightSubClause.Execute();
            ITreeIndexInternals mergedIndex = lhsIndex.SetOperation(rhsIndex, SetOperation.Or) as TreeIndex;
            return mergedIndex;
        }

        private ITreeIndexInternals ExecuteEndClause()
        {
            EnsureSubClauseCanExecute();

            ITreeIndexInternals endClauseIndex = this._parentQuery.GetIndexForMember(this._lhsData.ComparisonTable, this._lhsData.MemberToCompare);
            PropertyInfo primaryKeyAccessor = this._parentQuery.GetPrimaryKeyAccessorForTable(this._lhsData.ComparisonTable);

            ITreeIndexInternals toReturn;
            if (endClauseIndex == null)
            {
                toReturn = ExecuteEndClauseAgainstRawData(primaryKeyAccessor);
            }
            else
            {
                toReturn = endClauseIndex.ConstructTreeIndexFromTreeIndex();
                ITreeIndexInternals clauseResults = new TreeIndex(false);
                switch (this._clauseComparitor)
                {
                    case SubClauseComparitor.Equals:
                        clauseResults = endClauseIndex.GetValuesEquals((this._rhsData.LiteralToCompare as IComparable)!);
                        break;
                    case SubClauseComparitor.NotEquals:
                        clauseResults = endClauseIndex.GetValuesNotEquals((this._rhsData.LiteralToCompare as IComparable)!);
                        break;
                    case SubClauseComparitor.LessThan:
                        clauseResults = endClauseIndex.GetValuesLessThan((this._rhsData.LiteralToCompare as IComparable)!, IncludeEquals.DoNotInclude);
                        break;
                    case SubClauseComparitor.LessThanOrEqual:
                        clauseResults = endClauseIndex.GetValuesLessThan((this._rhsData.LiteralToCompare as IComparable)!, IncludeEquals.Include);
                        break;
                    case SubClauseComparitor.GreaterThan:
                        clauseResults = endClauseIndex.GetValuesGreaterThan((this._rhsData.LiteralToCompare as IComparable)!, IncludeEquals.DoNotInclude);
                        break;
                    case SubClauseComparitor.GreaterThanOrEqual:
                        clauseResults = endClauseIndex.GetValuesGreaterThan((this._rhsData.LiteralToCompare as IComparable)!, IncludeEquals.Include);
                        break;
                }

                ITreeNodeIterator iterator = clauseResults.GetIterator();
                TreeNode node = iterator.Node;
                while (node != null)
                {
                    foreach(object o in node.Values)
                    {
                        IComparable indexValue = primaryKeyAccessor.GetValue(o) as IComparable;
                        toReturn.Insert(indexValue, o);
                    }
                    node = iterator.MoveNext();
                }
            }
            return toReturn;
        }

        ITreeIndexInternals ExecuteEndClauseAgainstRawData(PropertyInfo primaryKeyAccessor)
        {
            List<object> tableData = null;
            tableData = this._parentQuery.GetDataForTable(this._lhsData.ComparisonTable);
            if (tableData == null ||
                tableData.Count==0 )
            {
                return null;
            }
            PropertyInfo accessor = tableData[0].GetType().GetProperty(this._lhsData.MemberToCompare, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            TreeIndex toReturn = new TreeIndex(false);
            toReturn.KeyMemberInfo = primaryKeyAccessor;
            IComparable compareRHS = (this._rhsData.LiteralToCompare as IComparable)!;

            compareRHS = Ancillary.ComparableHelper.CastToCompatibleType(accessor.PropertyType, compareRHS);
            int comparison = 0;
            foreach (object o in tableData)
            {
                IComparable compareLHS = accessor.GetValue(o) as IComparable;
                comparison = compareLHS.CompareTo(compareRHS);
                bool insert = false;
                switch (this._clauseComparitor)
                {
                    case SubClauseComparitor.Equals:
                        
                        if (comparison == 0)
                        {
                            insert = true;
                        }
                        break;
                    case SubClauseComparitor.NotEquals:
                        if (comparison != 0)
                        {
                            insert = true;
                        }
                        break;
                    case SubClauseComparitor.LessThan:
                        if (comparison < 0)
                        {
                            insert = true;
                        }
                        break;
                    case SubClauseComparitor.LessThanOrEqual:
                        if (comparison <= 0)
                        {
                            insert = true;
                        }
                        break;
                    case SubClauseComparitor.GreaterThan:
                        if (comparison > 0)
                        {
                            insert = true;
                        }
                        break;
                    case SubClauseComparitor.GreaterThanOrEqual:
                        if (comparison >= 0)
                        {
                            insert = true;
                        }
                        break;
                }
                if (insert)
                {
                    IComparable indexValue = primaryKeyAccessor.GetValue(o) as IComparable;
                    toReturn.Insert(indexValue, o);
                }
            }
            return toReturn;
        }

        void EnsureSubClauseCanExecute()
        {
            if (this._lhsData == null || this._rhsData == null)
            {
                throw new Exception("WHERE statement not set up correctly, no literals or table comparisons in subclause");
            }
            if (this._lhsData.IsLiteralNotVariable())
            {
                throw new Exception("WHERE statement not set up correctly, LHS of subclause is a literal");
            }
            else if (this._rhsData.LiteralToCompare == null)
            {
                throw new Exception("WHERE statement not set up correctly, RHS of subclause has no literal value");
            }
        }

        internal void SetSubClauseToType(SubClauseType setToType)
        {
            this._subClauseType = setToType;
            if (this._subClauseType != SubClauseType.EndNode)
            {
                this._leftSubClause = new SubClause(this._parentQuery);
                this._rightSubClause = new SubClause(this._parentQuery);
            }
        }

        internal void SetSubClauseWithLiteralTo(string lhsMemberToCompare, SubClauseComparitor comparitor, object rhsCompareToLiteral)
        {
            string lhsTableName = this._parentQuery.GetLeftTable();
            SetSubClauseWithLiteralTo(lhsTableName, lhsMemberToCompare, comparitor, rhsCompareToLiteral);
        }

        internal void SetSubClauseTo(string lhsMemberToCompare, SubClauseComparitor comparitor, string rhsMemberToCompare)
        {
            string selectedTableName = this._parentQuery.GetLeftTable();
            SetSubClauseTo(selectedTableName, lhsMemberToCompare, comparitor, selectedTableName, rhsMemberToCompare);
        }

        internal void SetSubClauseTo(string lhsComparisonTable, string lhsMemberToCompare, SubClauseComparitor comparitor, string rhsMemberToCompare)
        {
            string selectedTableName = this._parentQuery.GetLeftTable();
            SetSubClauseTo(lhsComparisonTable, lhsMemberToCompare, comparitor, selectedTableName, rhsMemberToCompare);
        }

        internal void SetSubClauseWithLiteralTo(string lhsComparisonTable, string lhsMemberToCompare, SubClauseComparitor comparitor, object rhsCompareToLiteral)
        {
            if (!this._parentQuery.TableExistsInQuery(lhsComparisonTable))
            {
                throw new Exception($"Cannot query on table {lhsComparisonTable} as table does not exist");
            }
            SubClauseData lhsSCD = new SubClauseData
            {
                ComparisonTable = lhsComparisonTable,
                MemberToCompare = lhsMemberToCompare
            };

            SubClauseData rhsSCD = new SubClauseData
            {
                LiteralToCompare = rhsCompareToLiteral
            };
            this._lhsData = lhsSCD;
            this._rhsData = rhsSCD;
            this._clauseComparitor = comparitor;
        }

        internal void SetSubClauseTo(string lhsComparisonTable, string lhsMemberToCompare, SubClauseComparitor comparitor, string rhsComparisonTable, string rhsMemberToCompare)
        {
            SubClauseData lhsSCD = new SubClauseData
            {
                ComparisonTable = lhsComparisonTable,
                MemberToCompare = lhsMemberToCompare
            };

            SubClauseData rhsSCD = new SubClauseData
            {
                ComparisonTable = rhsComparisonTable,
                MemberToCompare = rhsMemberToCompare
            };
            this._lhsData = lhsSCD;
            this._rhsData = rhsSCD;
            this._clauseComparitor = comparitor;
        }

        internal void SetSubClauseWithLiteralTo(object lhsCompareToLiteral, SubClauseComparitor comparitor, string rhsComparisonTable, string rhsMemberToCompare)
        {
            SetSubClauseWithLiteralTo(rhsComparisonTable, rhsMemberToCompare, comparitor, lhsCompareToLiteral);
        }

        internal SubClause GetLeftSubClause()
        {
            if (this._subClauseType == SubClauseType.EndNode)
            {
                throw new Exception("Cannot get sub clause for an end node");
            }
            return this._leftSubClause!;
        }

        internal SubClause GetRightSubClause()
        {
            if (this._subClauseType == SubClauseType.EndNode)
            {
                throw new Exception("Cannot get sub clause for an end node");
            }
            return this._rightSubClause!;
        }
    }

}
