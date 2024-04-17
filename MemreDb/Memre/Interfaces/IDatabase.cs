using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre
{
    internal interface IDatabase
    {
        bool ObtainReadLock();
        bool ObtainReadLock(out UInt64 currentStructureSequence, out Guid readLockId);
        bool ObtainReadLockEnsuringMatchingState(UInt64 expectedStructureSequence, Dictionary<string, UInt64> expectedTableSequences, out Guid readLockId);
        bool ObtainReadLock(List<string> forTables, out UInt64 currentStructureSequence, out Dictionary<string, UInt64> currentTableSequences, out Guid readLockId);
        bool ObtainReadLock(int millisecondsTimeout);
        void RelinquishReadLock(Guid readLockId);
        bool ObtainWriteLock();
        bool ObtainWriteLock(UInt64 expectedStructureSequence, Dictionary<string, UInt64> expectedTableSequences, out Guid writeId);
        bool ObtainWriteLock(int millisecondsTimeout);
        void RelinquishWriteLock();

        void AddTable<ContainedType>(string tableName, ContainedType tableClass)
            where ContainedType : class, new();
        void AddTable<ContainedType>(string tableName, string primaryKeyMember, bool autoIncrementingPrimaryKey)
            where ContainedType : class, new();
        void AddConstraint<ParentType, ChildType>(string parentTableName, string childTableName, string childForeignKeyMember, bool oneToOneNotMany, bool cascadeDelete)
            where ParentType : class, new()
            where ChildType : class, new();
        uint AddObjectToTable<T>(string tableName, T rowObject)
            where T : class, new();
        IMemreQuery CreateQuery(QueryType queryType, params string[] tableNames);
        //bool CanDeleteObjectFromTable(string tableName, IComparable matchTo);
        //void DeleteObjectFromTable(string tableName, IComparable matchTo);
        //bool TryGetObjectFromTable<ObjectType, PrimaryKeyType>(string tableName, PrimaryKeyType key, out ObjectType value)
        //    where ObjectType : class, new()
        //    where PrimaryKeyType : IComparable;
        //public bool TryGetObjectsFromTable<ObjectType, MatchingType>(string tableName, string memberToMatch, MatchingType matchTo, out List<ObjectType> values)
        //    where ObjectType : class, new()
        //    where MatchingType : struct;
    }
}
