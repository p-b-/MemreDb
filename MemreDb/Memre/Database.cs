using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MemreDb.Memre
{
    internal class Database : IDatabase
    {
        internal string DatabaseName { get; set; }
        Dictionary<string, BaseTable> tableNameToTable;
        Dictionary<string, List<Constraint>> _parentTableNameToConstraintList;
        Dictionary<string, List<Constraint>> _childTableNameToConstraintList;
        ReaderWriterLockSlim _lock;

        object _structureSequenceLock = new object();
        UInt64 _structureSequence = 1;
        Guid _writeLockId = Guid.Empty;

        object _readIdLocksLockingObject = new Object();
        List<Guid> _readLockIds = new List<Guid>();

        internal Database(string databaseName)
        {
            DatabaseName = databaseName;
            this._lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            this.tableNameToTable = new Dictionary<string, BaseTable>();
            this._parentTableNameToConstraintList = new Dictionary<string, List<Constraint>>();
            this._childTableNameToConstraintList = new Dictionary<string, List<Constraint>>();
        }

        #region IDatabase implementation
        public bool ObtainReadLock()
        {
            this._lock.EnterReadLock();
            return true;
        }

        public bool ObtainReadLock(out UInt64 currentStructureSequence, out Guid readLockId)
        {
            List<String> emptyTableNamesList = new List<string>();
            Dictionary<string, UInt64> currentTableSequences;

            return ObtainReadLock(emptyTableNamesList, out currentStructureSequence, out currentTableSequences, out readLockId);
        }

        public bool ObtainReadLock(List<string> forTables, out UInt64 currentStructureSequence, out Dictionary<string, UInt64> currentTableSequences, out Guid readLockId)
        {
            readLockId = Guid.Empty;
            currentTableSequences = new Dictionary<string, ulong>();
            bool obtainedLock = false;

            try
            {
                this._lock.EnterReadLock();
                obtainedLock = true;

                currentStructureSequence = GetCurrentStructureSequence();
                foreach (string tableName in forTables)
                {
                    BaseTable table = GetTable(tableName);
                    currentTableSequences[tableName] = table.StateId;
                }
            }
            catch(Exception)
            {
                if (obtainedLock)
                {
                    this._lock.ExitReadLock();
                }
                throw;
            }
            readLockId = Guid.NewGuid();
            AddIdToReadLocks(readLockId);

            return obtainedLock;
        }

        public bool ObtainReadLockEnsuringMatchingState(UInt64 expectedStructureSequence, Dictionary<string, UInt64> expectedTableSequences, out Guid readLockId)
        {
            readLockId = Guid.Empty;
            bool obtainedLock = false;
            this._lock.EnterReadLock();
            obtainedLock = true;

            bool incorrectSequence = true;
            bool returnValue = true;

            try
            {
                if (GetCurrentStructureSequence() == expectedStructureSequence)
                {
                    incorrectSequence = false;
                    foreach (string tableName in expectedTableSequences.Keys)
                    {
                        UInt64 expectedTableSequence = expectedTableSequences[tableName];
                        BaseTable table = GetTable(tableName);
                        if (expectedTableSequence != table.StateId)
                        {
                            incorrectSequence = true;
                            break;
                        }
                    }
                }
            }
            catch(Exception)
            {
                if (obtainedLock)
                {
                    this._lock.ExitReadLock();
                    throw;
                }
            }
            if (incorrectSequence)
            {
                this._lock.ExitReadLock();
                returnValue = false;
            }
            else
            {
                readLockId = Guid.NewGuid();
                AddIdToReadLocks(readLockId);
            }

            return returnValue;
        }


        public bool ObtainReadLock(int millisecondsTimeout)
        {
            return this._lock.TryEnterReadLock(millisecondsTimeout);
        }

        public void RelinquishReadLock(Guid readLockId)
        {
            this._lock.ExitReadLock();
            RemoveReadLockId(readLockId);
        }

        public bool ObtainWriteLock()
        {
            this._lock.EnterWriteLock();
            return true;
        }

        public bool ObtainWriteLock(UInt64 expectedStructureSequence, Dictionary<string, UInt64> expectedTableSequences, out Guid writeId)
        {
            writeId = Guid.Empty;
            this._lock.EnterWriteLock();

            bool incorrectSequence = true;
            bool returnValue = true;

            if (GetCurrentStructureSequence()==expectedStructureSequence)
            {
                incorrectSequence = false;
                foreach(string tableName in expectedTableSequences.Keys)
                {
                    UInt64 expectedTableSequence = expectedTableSequences[tableName];
                    BaseTable table = GetTable(tableName);
                    if (expectedTableSequence != table.StateId)
                    {
                        incorrectSequence = true;
                        break;
                    }
                }
            }
            if (incorrectSequence)
            {
                this._lock.ExitWriteLock();
                returnValue = false;
            }
            else
            {
                _writeLockId = Guid.NewGuid();
                writeId = _writeLockId;
            }

            return returnValue;
        }

        public bool ObtainWriteLock(int millisecondsTimeout)
        {
            return this._lock.TryEnterWriteLock(millisecondsTimeout);
        }

        public void RelinquishWriteLock()
        {
            this._lock.ExitWriteLock();
            this._writeLockId = Guid.Empty;
        }

        public void AddTable<ContainedType>(string tableName, ContainedType tableClass)
            where ContainedType : class, new()
        {
            if (tableClass == null)
            {
                throw new Exception("Missing table class");
            }
            Table<ContainedType> t = new Table<ContainedType>(this, tableName);
            AddTableThreadSafe(t);
        }

        public void AddTable<ContainedType>(string tableName, string primaryKeyMember, bool autoIncrementingPrimaryKey)
            where ContainedType : class, new()
        {
            Table<ContainedType> t = new Table<ContainedType>(this, tableName, primaryKeyMember, autoIncrementingPrimaryKey);

            AddTableThreadSafe<ContainedType>(t);
        }

        public void AddConstraint<ParentType, ChildType>(string parentTableName, string childTableName, string childForeignKeyMember, bool oneToOneNotMany, bool cascadeDelete)
            where ParentType : class, new()
            where ChildType : class, new()
        {
            GenericConstraint<ParentType, ChildType> constraint;
            Table<ParentType> parentTable;
            Table<ChildType> childTable;

            UInt64 currentStructureSequence = CreateConstraintThreadSafe<ParentType, ChildType>(parentTableName,
                childTableName, childForeignKeyMember, oneToOneNotMany, cascadeDelete, 
                out parentTable, out childTable, out constraint);

            AddConstraintThreadSafe<ParentType, ChildType>(currentStructureSequence, constraint);
        }

        public uint AddObjectToTable<T>(string tableName, T rowObject)
            where T : class, new()
        {
            Table<T> table;
            UInt64 currentStructureSequence = CheckCanAddObjectToTableThreadSafe<T>(tableName, rowObject, out table);

            uint idToReturn = 0;
            if (table!=null)
            {
                T clonedObject = CloneObject(rowObject);
                idToReturn = AddObjectToTableThreadSafe<T>(currentStructureSequence, table, clonedObject);
            }
            return idToReturn;
        }

        public IMemreQuery CreateQuery(QueryType queryType, params string[] tableNames)
        {
            List<string> tableNamesAsList = tableNames.ToList();
            Query query = new Query(this, queryType, tableNamesAsList);

            return query as IMemreQuery;
        }

        #endregion

        internal bool CanDeleteObjectFromTable(string tableName, IComparable matchTo, Guid writeId)
        {
            if (this._writeLockId==Guid.Empty || writeId!=this._writeLockId)
            {
                throw new Exception("Cannot delete objects due to incorrect write lock id being passed");
            }
            return ProcessObjectDeletionFromTable(tableName, matchTo, false);
        }

        internal void DeleteObjectFromTable(string tableName, IComparable matchTo, Guid writeId)
        {
            if (this._writeLockId == Guid.Empty || writeId != this._writeLockId)
            {
                throw new Exception("Cannot delete objects due to incorrect write lock id being passed");
            }
            ProcessObjectDeletionFromTable(tableName, matchTo, true);
        }

        internal PropertyInfo GetPrimaryKeyAccessorForTable(string tableName, Guid readLockId)
        {
            if (!ReadIdLockExists(readLockId))
            {
                throw new Exception("Cannot get primary key accessor due to incorrect read lock id being passed");
            }
            BaseTable table = GetTable(tableName);
            if (table == null)
            {
                throw new Exception($"Table {tableName} does not exist");
            }
            return table.PrimaryKeyAccessor;
        }

        internal Dictionary<string, List<Object>> GetDataFromTables(List<string> tableNames, Guid readLockId)
        {
            if (!ReadIdLockExists(readLockId))
            {
                throw new Exception("Cannot get data from tables due to incorrect read lock id being passed");
            }
            Dictionary<string, List<Object>> toReturn = new Dictionary<string, List<object>>();

            foreach (String tableName in tableNames)
            {
                BaseTable table = GetTable(tableName);
                UInt64 tableSequence = 0;
                toReturn[tableName] = table.GetObjects(out tableSequence);
            }

            return toReturn;
        }

        internal Dictionary<string, Dictionary<string, TreeIndex>> GetIndicesForTables(List<string> tableNames, Guid readLockId)
        {
            if (!ReadIdLockExists(readLockId))
            {
                throw new Exception("Cannot get data from indices due to incorrect read lock id being passed");
            }
            Dictionary<string, Dictionary<string, TreeIndex>> toReturn = new Dictionary<string, Dictionary<string, TreeIndex>>();

            foreach (String tableName in tableNames)
            {
                BaseTable table = GetTable(tableName);
                UInt64 tableSequence = 0;

                Dictionary<string, TreeIndex> indices = table.GetIndices(out tableSequence);
                toReturn[table.Name] = indices;
            }

            return toReturn;
        }

        private void EnforceConstraintsOnChildTable<T>(string tableName, T rowObject) where T : class, new()
        {
            List<Constraint> constraints = GetConstraintsForChildTable(tableName);
            foreach (Constraint c in constraints)
            {
                var foreignKeyValue = c.ChildForeignKeyPropertyInfo.GetValue(rowObject) as IComparable;
                if (foreignKeyValue == null)
                {
                    // Not the responsibility of the system to enforce whether fields can be null
                    //  or not
                    continue;
                }

                object parentTypeObject = null;
                if (!TryGetObjectFromTable(c.ParentTableName, foreignKeyValue, out parentTypeObject))
                {
                    throw new Exception($"Cannot add row to table {tableName} as violates foreign key constraints");
                }
            }
        }

        private bool TryGetObjectFromTable<PrimaryKeyType>(string tableName, PrimaryKeyType key, out object value)
            where PrimaryKeyType : IComparable
        {
            if (!TableExists(tableName))
            {
                throw new Exception($"Cannot get object, table {tableName} does not exist");
            }
            BaseTable table = GetTable(tableName);

            // No need to clone, this is used private inside the database class
            value = table.GetObjectByPrimaryKey(key);

            if (value == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        bool AnyConstraintsAsParentTable(string tableName)
        {
            List<Constraint> constraintList = null;
            if (!this._parentTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                return false;
            }
            Debug.Assert(constraintList.Count > 0);
            return true;
        }

        bool AnyConstraintsAsChildTable(string tableName)
        {
            List<Constraint> constraintList = null;
            if (!this._childTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                return false;
            }
            Debug.Assert(constraintList.Count > 0);
            return true;
        }

        List<Constraint> GetConstraintsForParentTable(string tableName)
        {
            List<Constraint> constraintList = null;
            if (!this._parentTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                constraintList = new List<Constraint>();
            }
            return constraintList;
        }

        List<Constraint> GetConstraintsForChildTable(string tableName)
        {
            List<Constraint> constraintList = null;
            if (!this._childTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                constraintList = new List<Constraint>();
            }
            return constraintList;
        }

        void AddConstraintToParentTable(string tableName, Constraint c)
        {
            List<Constraint> constraintList = null;
            if (!this._parentTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                constraintList = new List<Constraint>();
                this._parentTableNameToConstraintList[tableName] = constraintList;
            }

            constraintList.Add(c);
        }

        void AddConstraintToChildTable(string tableName, Constraint c)
        {
            List<Constraint> constraintList = null;
            if (!this._childTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                constraintList = new List<Constraint>();
                this._childTableNameToConstraintList[tableName] = constraintList;
            }

            constraintList.Add(c);
        }

        void RemoveConstraintFromParentTable(string tableName, Constraint c)
        {
            List<Constraint> constraintList = null;
            if (!this._parentTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                return;
            }

            constraintList.Remove(c);
            if (constraintList.Count == 0)
            {
                this._parentTableNameToConstraintList.Remove(tableName);
            }
        }

        void RemoveConstraintFromChildTable(string tableName, Constraint c)
        {
            List<Constraint> constraintList = null;
            if (!this._childTableNameToConstraintList.TryGetValue(tableName, out constraintList))
            {
                return;
            }

            constraintList.Remove(c);
            if (constraintList.Count == 0)
            {
                this._childTableNameToConstraintList.Remove(tableName);
            }
        }

        private T CloneObject<T>(T objectToClone) where T : new()
        {
            if (objectToClone == null)
            {
                throw new Exception("Cannot clone null object");
            }

            FieldInfo[] fis = objectToClone.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object clonedObject = new T();
            foreach (FieldInfo fi in fis)
            {
                if (fi.FieldType.Namespace != objectToClone.GetType().Namespace)
                    fi.SetValue(clonedObject, fi.GetValue(objectToClone));
                else
                {
                    object obj = fi.GetValue(objectToClone);
                    fi.SetValue(clonedObject, CloneObject(obj));
                }
            }
            return (T)clonedObject;

        }

        private Table<ContainedType> GetTable<ContainedType>(string tableName)
            where ContainedType : class, new()
        {
            BaseTable tableAsBaseType = this.tableNameToTable[tableName];
            Table<ContainedType> table = tableAsBaseType as Table<ContainedType>;
            if (table == null)
            {
                throw new Exception($"Cannot get table {tableName}");
            }
            return table;
        }

        private BaseTable GetTable(string tableName)
        {
            BaseTable tableAsBaseType = this.tableNameToTable[tableName];
            if (tableAsBaseType == null)
            {
                throw new Exception($"Cannot get table {tableName}");
            }
            return tableAsBaseType;
        }

        private bool TableExists(string tableName)
        {
            return this.tableNameToTable.ContainsKey(tableName);
        }

        private UInt64 GetCurrentStructureSequence()
        {
            lock (this._structureSequenceLock)
            {
                return this._structureSequence;
            }
        }

        private bool IncrementStructureSequence(UInt64 previousSequenceNumber)
        {
            lock (this._structureSequenceLock)
            {
                if (this._structureSequence != previousSequenceNumber)
                {
                    return false;
                }
                else
                {
                    this._structureSequence++;
                    return true;
                }
            }
        }

        private UInt64 CreateConstraintThreadSafe<ParentType, ChildType>(string parentTableName,
            string childTableName, string childForeignKeyMember, bool oneToOneNotMany, bool cascadeDelete, 
            out Table<ParentType> parentTable, out Table<ChildType> childTable, 
            out GenericConstraint<ParentType, ChildType> constraint)
            where ParentType : class, new()
            where ChildType : class, new()
        {
            bool readLockObtained = false;
            UInt64 currentStructureSequence = 0;
            Guid readLockId = Guid.Empty;

            PropertyInfo childForeignKeyAccessor;
            try
            {
                readLockObtained = ObtainReadLock(out currentStructureSequence, out readLockId);

                if (!TableExists(parentTableName))
                {
                    throw new Exception($"Cannot set constraint, parent table {parentTableName} does not exist");
                }
                if (!TableExists(childTableName))
                {
                    throw new Exception($"Cannot set constraint, child table {childTableName} does not exist");
                }
                parentTable = GetTable<ParentType>(parentTableName);
                childTable = GetTable<ChildType>(childTableName);
                childForeignKeyAccessor = typeof(ChildType).GetProperty(childForeignKeyMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                constraint = new GenericConstraint<ParentType, ChildType>()
                {
                    ParentTable = parentTable,
                    ChildTable = childTable,
                    ChildForeignKeyMember = childForeignKeyMember,
                    OneToOneNotMany = oneToOneNotMany,
                    CascadeDelete = cascadeDelete,
                    ChildForeignKeyPropertyInfo = childForeignKeyAccessor,
                    ParentTableName = parentTableName,
                    ChildTableName = childTableName
                };
                if (childForeignKeyAccessor == null)
                {
                    throw new Exception($"Cannot access foreign key member {childForeignKeyMember} on table {childTableName}");
                }
            }
            finally
            {
                if (readLockObtained)
                {
                    RelinquishReadLock(readLockId);
                }
            }
            return currentStructureSequence;
        }

        private void AddConstraintThreadSafe<ParentType, ChildType>(UInt64 currentStructureSequence, GenericConstraint<ParentType, ChildType> constraint)
            where ParentType : class, new()
            where ChildType : class, new()
        {
            bool writeLockObtained = false;
            try
            {
                writeLockObtained = ObtainWriteLock();
                if (IncrementStructureSequence(currentStructureSequence))
                {
                    AddConstraintToParentTable(constraint.ParentTableName, constraint);
                    AddConstraintToChildTable(constraint.ChildTableName, constraint);
                    constraint.ChildTable.MemberToBeIndexed(constraint.ChildForeignKeyMember);
                }
                else
                {
                    throw new Exception($"Could not add constraint to database, as structure changed during operation");
                }
            }
            finally
            {
                if (writeLockObtained)
                {
                    RelinquishWriteLock();
                }
            }
        }

        private UInt64 CheckCanAddObjectToTableThreadSafe<T>(string tableName, T rowObject, out Table<T> table)
            where T : class, new()
        {
            bool readLockObtained = false;
            UInt64 currentStructureSequence = 0;
            List<string> tableNames = new List<string>() { tableName };
            Dictionary<string, UInt64> currentTableSequences;
            Guid readLockId = Guid.Empty;

            try
            {
                readLockObtained = ObtainReadLock(tableNames, out currentStructureSequence, out currentTableSequences, out readLockId);

                EnforceConstraintsOnChildTable(tableName, rowObject);
                if (!TableExists(tableName))
                {
                    throw new Exception($"Table {tableName} does not exist");
                }
                object o = this.tableNameToTable[tableName];
                table = o as Table<T>;
            }
            finally
            {
                if (readLockObtained)
                {
                    RelinquishReadLock(readLockId);
                }
            }
            return currentStructureSequence;
        }

        private uint AddObjectToTableThreadSafe<T>(UInt64 currentStructureSequence, Table<T> table, T clonedObject)
            where T : class, new()
        {
            uint idToReturn = 0;
            bool writeLockObtained = false;
            try
            {
                writeLockObtained = ObtainWriteLock();
                if (IncrementStructureSequence(currentStructureSequence))
                {
                    idToReturn = table.AddRow(clonedObject);
                }
                else
                {
                    throw new Exception($"Could not add constraint to database, as structure changed during operation");
                }
            }
            finally
            {
                if (writeLockObtained)
                {
                    RelinquishWriteLock();
                }
            }
            return idToReturn;
        }

        private void AddTableThreadSafe<ContainedType>(Table<ContainedType> table)
            where ContainedType : class, new()
        {
            bool success = false;
            for (int attempt = 0; attempt < 3; ++attempt)
            {
                bool readLockObtained = false;
                Guid readLockId = Guid.Empty;
                UInt64 currentStructureSequence = 0;
                try
                {
                    readLockObtained = ObtainReadLock(out currentStructureSequence, out readLockId);
                    if (TableExists(table.Name))
                    {
                        throw new Exception($"Table {table.Name} already exists");
                    }
                }
                finally
                {
                    if (readLockObtained)
                    {
                        RelinquishReadLock(readLockId);
                    }
                }

                bool writeLockObtained = false;
                try
                {
                    writeLockObtained = ObtainWriteLock();
                    if (IncrementStructureSequence(currentStructureSequence))
                    {
                        this.tableNameToTable[table.Name] = table;
                        success = true;
                    }
                }
                finally
                {
                    if (writeLockObtained)
                    {
                        RelinquishWriteLock();
                    }
                }
                if (success)
                {
                    break;
                }
            }
            if (!success)
            {
                throw new Exception($"Could not add table {table.Name}, as structure changed during operation");
            }
        }

        private bool ProcessObjectDeletionFromTable(string tableName, IComparable matchTo, bool actuallyDelete)
        {
            if (!TableExists(tableName))
            {
                throw new Exception($"Cannot delete object, table {tableName} does not exist");
            }

            BaseTable table = GetTable(tableName);

            object objectToDelete = table.GetObjectByPrimaryKey(matchTo);
            if (!table.HasPrimaryKey)
            {
                // TODO Delete objects by specifying any column
                // TODO Delete objects by criteria any criteria
                throw new Exception("Cannot currently delete an object from a table without a primary key");
            }
            else
            {
                IComparable pkValue = table._primaryIndex.KeyMemberInfo.GetValue(objectToDelete) as IComparable;
                List<Constraint> constraints = GetConstraintsForParentTable(tableName);
                foreach (Constraint c in constraints)
                {
                    BaseTable childTable = GetTable(c.ChildTableName);
                    List<object> linkedObjects = childTable.GetObjects(pkValue, c.ChildForeignKeyPropertyInfo);
                    if (linkedObjects.Count > 0)
                    {
                        if (c.CascadeDelete)
                        {
                            // Deleting the object from the table, ultimately alters the original list
                            List<object> listCopy = new List<object>();
                            listCopy.AddRange(linkedObjects);
                            foreach (Object linkedObject in listCopy)
                            {
                                var primaryKeyValue = childTable.GetPrimaryKeyValueFromObject(linkedObject);
                                bool subDeletionSuccessful = ProcessObjectDeletionFromTable(c.ChildTableName, primaryKeyValue as IComparable, actuallyDelete);
                                if (!subDeletionSuccessful && !actuallyDelete)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (actuallyDelete)
                            {
                                throw new Exception($"Cannot delete object(s) from table {tableName} due to non-cascasing deletion foreign key constraint");
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                if (actuallyDelete)
                {
                    table.DeleteObjectWithPrimaryKey(matchTo);
                }
            }
            return true;
        }

        void AddIdToReadLocks(Guid id)
        {
            lock (this._readIdLocksLockingObject)
            {
                this._readLockIds.Add(id);
            }
        }

        bool ReadIdLockExists(Guid id)
        {
            bool exists = false;
            lock (this._readIdLocksLockingObject)
            {
                if (id != Guid.Empty)
                {
                    exists = this._readLockIds.Contains(id);
                }
            }
            return exists;
        }

        void RemoveReadLockId(Guid id)
        {
            lock (this._readIdLocksLockingObject)
            {
                this._readLockIds.Remove(id);
            }
        }

    }
}
