using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MemreDb.Memre.Indices;

namespace MemreDb.Memre
{
    internal class BaseTable
    {
        internal Type TableType { get; set; }
        public string Name { get; set; }
        internal List<Constraint> _constraints;
        internal List<object> _rows;
        internal TreeIndex _primaryIndex;
        internal Dictionary<string, TreeIndex> _propertyNameToIndex;
        internal bool _primaryKeyAutoIncrements = false;

        internal object _stateLock = new object();
        internal UInt64 _currentSequence = 1;

        internal string _primaryKeyMember = String.Empty;
        internal bool _hasPrimaryKey = false;
        internal uint _nextId;

        internal Database _parentDatabase;

        internal UInt64 StateId
        {
            get
            {
                UInt64 toReturn = 0;
                lock (_stateLock)
                {
                    toReturn = _currentSequence;
                }
                return toReturn;
            }
        }

        internal bool HasPrimaryKey
        {
            get
            {
                return this._hasPrimaryKey;
            }
        }

        internal PropertyInfo PrimaryKeyAccessor
        {
            get
            {
                if (HasPrimaryKey)
                {
                    return this._primaryIndex.KeyMemberInfo;
                }
                else
                {
                    return null;
                }
            }
        }

        internal BaseTable(Type _tableType)
        {
            TableType = _tableType;

            this._rows = new List<object>();
            this._constraints = new List<Constraint>();
            this._propertyNameToIndex = new Dictionary<string, TreeIndex>();
        }

        internal object GetObjectByPrimaryKey<PrimaryKeyType>(PrimaryKeyType pkValue)
            where PrimaryKeyType : IComparable
        {
            object toReturn = null!;

            if (this._hasPrimaryKey)
            {
                toReturn = this._primaryIndex.GetValueSingular(pkValue);
            }

            return toReturn!;
        }

        internal void DeleteObjectWithPrimaryKey<PrimaryKeyType>(PrimaryKeyType pkValue)
            where PrimaryKeyType : IComparable
        {
            if (this._hasPrimaryKey)
            {
                object objectBeingDeleted = this._primaryIndex.GetValueSingular(pkValue);
                this._primaryIndex.Delete(pkValue);
                this._rows.Remove(objectBeingDeleted);

                foreach (var member in this._propertyNameToIndex.Keys)
                {
                    TreeIndex index = this._propertyNameToIndex[member];
                    PropertyInfo memberPropertyInfo = objectBeingDeleted.GetType().GetProperty(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    IComparable memberValue = memberPropertyInfo.GetValue(objectBeingDeleted) as IComparable;
                    if (index.IsUnique)
                    {
                        index.Delete(memberValue);
                    }
                    else
                    {
                        index.Delete(memberValue, objectBeingDeleted);
                    }
                }
            }
        }

        internal List<object> GetObjects<MatchType>(MatchType matchTo, PropertyInfo columnProperty)
            where MatchType : IComparable
        {
            if (matchTo == null)
            {
                throw new Exception($"Cannot get object in table {Name} with a null search parameter");
            }
            TreeIndex index;

            if (!this._propertyNameToIndex.TryGetValue(columnProperty.Name, out index))
            {
                return SearchForObjects(matchTo, columnProperty);
            }
            return index.GetValuesEqualsAsList(matchTo);
        }

        internal List<object> SearchForObjects(object matchTo, PropertyInfo columnProperty)
        {
            List<object> toReturn = new List<object>();
            foreach (Object objToCheck in this._rows)
            {
                object checkAgainstValue = columnProperty.GetValue(objToCheck);
                if (checkAgainstValue.Equals(matchTo))
                {
                    toReturn.Add(objToCheck);
                }
            }
            return toReturn;
        }

        internal List<object> GetObjects(out UInt64 currentSequence)
        {
            currentSequence = StateId;
            List<object> returnValue = new List<Object>();
            returnValue.AddRange(this._rows);
            return returnValue;
        }

        internal Dictionary<String, TreeIndex> GetIndices(out UInt64 currentSequence)
        {
            lock (this._stateLock)
            {
                currentSequence = this._currentSequence;
                Dictionary<String, TreeIndex> returnValue = new Dictionary<string, TreeIndex>();
                if (this._hasPrimaryKey)
                {
                    returnValue[this._primaryIndex.KeyMemberInfo.Name] = this._primaryIndex;
                }
                foreach (string indexName in this._propertyNameToIndex.Keys)
                {
                    returnValue[indexName] = this._propertyNameToIndex[indexName];
                }
                return returnValue;
            }
        }


        internal object SearchForObject(object matchTo, PropertyInfo columnProperty)
        {
            foreach (Object objToCheck in this._rows)
            {
                object checkAgainstValue = columnProperty.GetValue(objToCheck);
                if (checkAgainstValue.Equals(matchTo))
                {
                    return objToCheck;
                }
            }
            return null;
        }

        internal object GetPrimaryKeyValueFromObject(object objectValue)
        {
            if (!HasPrimaryKey)
            {
                throw new Exception($"Cannot get primary key value for object in table {Name} as no primary key is set");
            }

            return _primaryIndex.KeyMemberInfo.GetValue(objectValue);
        }
    }

}
