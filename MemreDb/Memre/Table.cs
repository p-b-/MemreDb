using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre.Indices;
using System.Diagnostics;
using MemreDb.Memre.Ancillary;

namespace MemreDb.Memre
{
    internal class Table<ContainedType> : BaseTable
        where ContainedType : class, new()
    {
        internal Table(Database parentDatabase, string name) : this(parentDatabase, name, String.Empty, false)
        {
        }

        internal Table(Database parentDatabase, string name, string primaryKeyMember, bool primaryKeyAutoIncrements)
            : base(typeof(ContainedType))
        {
            Name = name;
            this._parentDatabase = parentDatabase;
            this._nextId = 1;
            this._primaryIndex = null;

            if (!String.IsNullOrEmpty(primaryKeyMember))
            {
                SetPrimaryKey(primaryKeyMember);
                this._primaryKeyAutoIncrements = primaryKeyAutoIncrements;
            }
        }

        void SetPrimaryKey(string member)
        {
            this._primaryKeyMember = member;
            this._hasPrimaryKey = true;
            PropertyInfo propInfo = typeof(ContainedType).GetProperty(this._primaryKeyMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (propInfo == null)
            {
                throw new Exception($"Primary key {member} for table {Name} does not exist as an member of the stored class");
            }
            this._primaryIndex = new TreeIndex(true);
            this._primaryIndex.KeyMemberInfo = propInfo;
        }

        internal void MemberToBeIndexed(string member)
        {
            // Check to ensure if the member is already indexed
            if (this._propertyNameToIndex.ContainsKey(member))
            {
                return;
            }
            PropertyInfo memberPropInfo = typeof(ContainedType).GetProperty(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (memberPropInfo == null)
            {
                throw new Exception($"Member {member} for table {Name} does not exist as an member of the stored class");
            }
            TreeIndex index = new TreeIndex(false);
            index.KeyMemberInfo = memberPropInfo;
            this._propertyNameToIndex[member] = index;
        }

        internal uint AddRow(ContainedType objectToAdd)
        {
            if (objectToAdd == null)
            {
                throw new Exception($"Cannot insert null object into table {Name}");
            }

            uint idToReturn = 0;
            this._rows.Add(objectToAdd);

            if (this._hasPrimaryKey &&
                this._primaryKeyAutoIncrements && objectToAdd != null)
            {
                if (this._primaryIndex!.KeyMemberInfo != null)
                {
                    idToReturn = this._nextId;
                    this._primaryIndex.KeyMemberInfo.SetValue(objectToAdd, idToReturn);
                    this._nextId++;
                }
            }
            if (this._hasPrimaryKey && this._primaryIndex!.KeyMemberInfo != null)
            {
                IComparable key = this._primaryIndex!.KeyMemberInfo.GetValue(objectToAdd) as IComparable;
                if (key != null)
                {
                    AddRowToIndex(objectToAdd!, key);
                }
            }
            return idToReturn;
        }

        void AddRowToIndex<PrimaryKeyType>(ContainedType o, PrimaryKeyType primaryKeyValue)
            where PrimaryKeyType : IComparable
        {
            this._primaryIndex.Insert(primaryKeyValue, o);

            foreach(var member in this._propertyNameToIndex.Keys)
            {
                TreeIndex index = this._propertyNameToIndex[member];
                PropertyInfo memberPropertyInfo = typeof(ContainedType).GetProperty(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                IComparable memberValue = memberPropertyInfo.GetValue(o) as IComparable;

                index.Insert(memberValue, o);
            }
        }

        internal ContainedType GetObject<PrimaryKeyType>(PrimaryKeyType key)
            where PrimaryKeyType : IComparable
        {
            object toReturnAsObject=base.GetObjectByPrimaryKey(key);
            ContainedType toReturn = null!;
            if (toReturnAsObject!=null)
            {
                toReturn = toReturnAsObject as ContainedType;
            }
            return toReturn!;
        }

        internal List<ContainedType> GetObjects<MatchingType>(string memberToMatch, MatchingType matchTo)
        {
            List<ContainedType> toReturn = new List<ContainedType>();
            PropertyInfo memberPropertyInfo = typeof(ContainedType).GetProperty(memberToMatch, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (memberPropertyInfo==null)
            {
                throw new Exception($"Cannot find member {memberToMatch} inside table {Name}");
            }
            foreach (ContainedType row in this._rows)
            {
                MatchingType rowType = (MatchingType)memberPropertyInfo.GetValue(row);
                if (rowType==null)
                {
                    continue;
                }
                if (EqualityComparer<MatchingType>.Default.Equals(rowType,matchTo))
                {
                    toReturn.Add(row);
                }
            }
            return toReturn;
        }
    }
}