using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre.Indices;
using MemreDb.Memre.Ancillary;
using MemreDb.Memre.Indices.Iterators;

namespace MemreDb.Memre
{
    internal class Query : IMemreQuery
    {
        string _leftTableName;
        List<string> _tableNames;
        Database _db;
        QueryType _queryType;
        WhereClause _whereClause = null;

        bool _ordered = false;
        Dictionary<string, List<Object>> _tableData;

        UInt64 _structureSequence = 0;
        Dictionary<string, Dictionary<string, TreeIndex>> _tableIndices;
        Dictionary<string, UInt64> _tableSequences;
        Guid _writeId = Guid.Empty;
        Guid _readLockId = Guid.Empty;

        TreeIndex _filteredData;
        OrderDirection _filterDataOrder;

        internal Query(Database db, QueryType queryType, String selectFromTable)
            : this(db, queryType, new List<String> { selectFromTable })
        {

        }

        internal Query(Database db, QueryType queryType, List<string> tableNames)
        {
            this._leftTableName = tableNames[0];
            this._db = db;
            this._queryType = queryType;
            this._tableNames = tableNames;
            this._tableData = null;
            this._tableIndices = null;
            this._filteredData = null;
            this._filterDataOrder = OrderDirection.Ascending;
        }

        #region IMemreQuery implementation
        public List<ContainedType> Select<ContainedType>(string tableName)
            where ContainedType : new()
        {
            return SelectWithOrder<ContainedType>(tableName, this._filterDataOrder);
        }

        public void OrderBy(List<OrderBy> orderByCollection)
        {
            if (orderByCollection == null || orderByCollection.Count == 0)
            {
                return;
            }
            if (this._filteredData == null || this._filteredData.Empty)
            {
                return;
            }
            this._ordered = true;
            Object exampleObject = this._filteredData.GetAnyValue();
            // This throws an exception if invalid collection
            GetSortMemberAccessors(orderByCollection, exampleObject);

            OrderBy firstOrder = orderByCollection[0];
            this._filterDataOrder = firstOrder.Direction;
            if (firstOrder.MemberToOrder == this._filteredData.KeyMemberInfo.Name)
            {
                this._filteredData.Sort(orderByCollection);
            }
            else
            {
                TreeIndex resortedData = new TreeIndex(false);
                resortedData.AddFromOtherTreeAndSort(this._filteredData, orderByCollection);
                this._filteredData = resortedData;
            }
        }

        public WhereClause AddWhere()
        {
            if (this._whereClause != null)
            {
                throw new Exception("Query already has a where clause");
            }
            this._whereClause = new WhereClause(this);
            return this._whereClause;
        }

        public int? Execute()
        {
            int? toReturn = null;
            GetDataFromTables();

            this._filteredData = null;
            if (this._whereClause==null)
            {
                this._filteredData = GetDataWithoutWhereClause();
            }
            else
            {
                this._filteredData = ExecuteWhereClause();
            }

            switch (this._queryType)
            {
                case QueryType.Delete:
                    toReturn = DeleteData();
                    break;
                case QueryType.Update:
                    toReturn = UpdateData();
                    break;
            }

            return toReturn;
        }
        #endregion

        void GetDataFromTables()
        {
            // TODO Consider using read lock id, rather than sequences
            // Getting read lock should return id and structure sequences.

            bool readLockObtained = false;
            try
            {
                if (_readLockId == Guid.Empty)
                {
                    readLockObtained = this._db.ObtainReadLock(_tableNames, out _structureSequence, out _tableSequences, out _readLockId);
                }
                else
                {
                    readLockObtained = true;
                }

                this._tableData = this._db.GetDataFromTables(_tableNames, _readLockId);
                this._tableIndices = this._db.GetIndicesForTables(_tableNames, _readLockId);
            }
            finally
            {
                if (readLockObtained)
                {
                    this._db.RelinquishReadLock(_readLockId);
                    this._readLockId = Guid.Empty;
                }
            }

            

            //this._structureSequence = 0;
            //this._tableData = this._db.GetDataFromTables(_tableNames, out _tableSequences, out this._structureSequence);

            //Dictionary<string, UInt64> indexSequences = new Dictionary<string, ulong>();
            //this._tableIndices = this._db.GetIndicesForTables(_tableNames, out indexSequences, this._structureSequence);

            //foreach(string tableName in _tableNames)
            //{
            //    if (_tableSequences[tableName] != indexSequences[tableName])
            //    {
            //        throw new Exception("Cannot query database, write operation occured during query setup");
            //    }
            //}
        }

        internal bool TableExistsInQuery(string tableName)
        {
            bool tableExists = _tableNames.Any(t => t == tableName);
            return tableExists;
        }

        internal string GetLeftTable()
        {
            return this._leftTableName;
        }

        internal TreeIndex GetIndexForMember(string tableName, string memberName)
        {
            if (this._tableIndices==null)
            {
                return null;
            }
            Dictionary<string, TreeIndex> indices = null;
            if (!_tableIndices.TryGetValue(tableName, out indices))
            {
                return null;
            }
            TreeIndex toReturn = null;
            if (!indices.TryGetValue(memberName, out toReturn))
            {
                return null;
            }
            return toReturn;
        }

        internal PropertyInfo GetPrimaryKeyAccessorForTable(string tableName)
        {
            UInt64 tableSequence = _tableSequences[tableName];
            return this._db.GetPrimaryKeyAccessorForTable(tableName, _readLockId);
        }

        internal List<Object> GetDataForTable(string tableName)
        {
            if (this._tableData == null)
            {
                return null;
            }
            List<Object> toReturn = null;
            _tableData.TryGetValue(tableName, out toReturn);
            return toReturn;
        }

        void CloneAndAddObjectToList<ContainedType>(List<ContainedType> stronglyTypedlist, ContainedType objectToCloneAndAdd)
            where ContainedType : new()
        {
            if (objectToCloneAndAdd != null)
            {
                ContainedType clonedObject = CloneHelper.CloneObject<ContainedType>(objectToCloneAndAdd);
                if (clonedObject != null)
                {
                    stronglyTypedlist.Add(clonedObject);
                }
            }
        }

        TreeIndex ExecuteWhereClause()
        {
            bool readLockObtained = false;

            try
            {
                readLockObtained = this._db.ObtainReadLockEnsuringMatchingState(this._structureSequence, this._tableSequences, out _readLockId);
                if (!readLockObtained)
                {
                    throw new Exception("Could not execute select query - data may have changed since the initial query");
                }
                return _whereClause.Execute() as TreeIndex;
            }
            finally
            {
                if (readLockObtained)
                {
                    this._db.RelinquishReadLock(_readLockId);
                    this._readLockId = Guid.Empty;
                }
            }
        }

        TreeIndex GetDataWithoutWhereClause()
        {
            bool readLockObtained = false;

            try
            {
                readLockObtained = this._db.ObtainReadLockEnsuringMatchingState(this._structureSequence, this._tableSequences, out _readLockId);
                if (!readLockObtained)
                {
                    throw new Exception("Could not execute select query - data may have changed since the initial query");
                }
                List<object> dataFromSelectedTable = this._tableData[this._leftTableName];
                PropertyInfo primaryKeyAccessor = GetPrimaryKeyAccessorForTable(this._leftTableName);

                TreeIndex toReturn = new TreeIndex(true);

                if (dataFromSelectedTable != null)
                {
                    foreach (object o in dataFromSelectedTable)
                    {
                        IComparable primaryKeyForRow = primaryKeyAccessor.GetValue(o) as IComparable;
                        toReturn.Insert(primaryKeyForRow, o);
                    }
                }

                return toReturn;
            }
            finally
            {
                if (readLockObtained)
                {
                    this._db.RelinquishReadLock(this._readLockId);
                    this._readLockId = Guid.Empty;
                }
            }
        }

        void GetSortMemberAccessors(List<OrderBy> orderByCollection, Object exampleObject)
        {
            if (orderByCollection.Count == 0)
            {
                throw new Exception("Order by collection contains no order information");
            }
            Dictionary<string, bool> memberNameExists = new Dictionary<string, bool>();
            foreach (OrderBy orderBy in orderByCollection)
            {
                if (memberNameExists.ContainsKey(orderBy.MemberToOrder))
                {
                    throw new Exception("Cannot order by the same column more than once in a query");
                }
                if (exampleObject!=null)
                {
                    PropertyInfo propInfo = exampleObject.GetType().GetProperty(orderBy.MemberToOrder, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propInfo==null)
                    {
                        throw new Exception($"Order by collection refers to member {orderBy.MemberToOrder} that does not exist in the underlying object");
                    }
                    orderBy.MemberAccessor = propInfo;
                }
                memberNameExists.Add(orderBy.MemberToOrder, true);
            }

        }

        List<ContainedType> SelectWithOrder<ContainedType>(string tableName, OrderDirection direction)
            where ContainedType : new()
        {
            List<ContainedType> toReturn = new List<ContainedType>();
            if (_filteredData != null)
            {
                ITreeIndexIterator iterator;
                if (direction == OrderDirection.Ascending)
                {
                    iterator = this._filteredData.GetTreeIndexIterator();
                }
                else
                {
                    iterator = this._filteredData.GetReverseTreeIndexIterator();
                }

                while(iterator.Valid)
                {
                    Object value = iterator.CurrentValue;
                    IComparable index = iterator.CurrentIndex;

                    if (value!=null)
                    {
                        ContainedType storedObjectCorrectType = (ContainedType)value;
                        CloneAndAddObjectToList(toReturn, storedObjectCorrectType);
                    }
                    iterator.MoveNext();
                }
            }
            return toReturn;
        }

        int? DeleteData()
        {
            if (this._ordered)
            {
                throw new Exception("Cannot combine DELETE with ORDER BY");
            }
            bool writeLockObtained = false;
            try
            {
                writeLockObtained = this._db.ObtainWriteLock(this._structureSequence, this._tableSequences, out _writeId);
                if (!writeLockObtained)
                {
                    throw new Exception("Could not perform deletion - data may have changed since the initial query");
                }
                ITreeIndexIterator iterator = this._filteredData.GetTreeIndexIterator();

                while (iterator.Valid)
                {
                    if (!this._db.CanDeleteObjectFromTable(_leftTableName, iterator.CurrentIndex, this._writeId))
                    {
                        throw new Exception("Cannot execute delete due to foreign key violations");
                    }
                    iterator.MoveNext();
                }

                iterator = this._filteredData.GetTreeIndexIterator();
                int count = 0;
                while (iterator.Valid)
                {
                    this._db.DeleteObjectFromTable(_leftTableName, iterator.CurrentIndex, this._writeId);
                    iterator.MoveNext();
                    ++count;
                }
                return count;
            }
            finally
            {
                if (writeLockObtained)
                {
                    this._db.RelinquishWriteLock();
                    this._writeId = Guid.Empty;
                }
            }
        }

        int? UpdateData()
        {
            return null;
        }
    }
}