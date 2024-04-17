# Introduction

MemreDb is a simple database-like system, that was developed from a need for mocking data as if it were connected
to an API subsystem which is fetching results form a SQL Database, and the results are returned as fully formed C# objects.

This probably isn't the database that you're looking for.

# Usage

Running this calls the tests in the demo directory.

# Note

This project was created over a couple of weeks as a learning exercise about the internals of SQL. Without foreign-key queries it isn't really that useful.

To support retrieving multiple class types via the query.Select<T>() when foreign-key queries are supported, as C# does not support variadic template types for method calls, one of the following will need implementing:
* Multiple overloads of the Select() method different number of template types
* Multiple calls to the Select() methods with differing template types.


# Constraints

All foreign key constraints need to be against the primary key
All keys that form part of queries need to derive from IComparable
All objects added to the database need a public parameterless constructor
Currently no SQL parser, queries are constructor with the query.AddWhere() and where.GetTopLevelClause(), and subclauses are added in a hierarchy.
No foreign key queries in the initial check-in.


* Tidy test code up  :done:
  Add better examples  :done:
  Add more queries
* Delete Query
* Tidy up use of interfaces
  Improved - but need to probably get rid of ITreeIndex as it should only be used internally

* On Insertion - clone object :done:
* On querying, insert object into results tree from original node, so all data is copied over
* On querying, create return tree :done:

* SELECT WHERE on single tables   :done:
* UPDATE WHERE on single tables
* Add Reverse iterator :done:
  range operators (< > <= >= ) to use reverse and forward iterator :done:
* Add <> range operator :done:
* Add Invert range operator
* Multi-threaded support
  Read priority locks
 
* Ordering :done for single table queries:
* Foreign key joins
* query.Select on other tables from query
* Query support for non-indexed columns :done:
* Query support for non-indexed tables - may be possible if do comparison on reference.
  Need to store in this._rows in this case
* Remove storing in this._rows if there is a primary key
  Querying on non-indexes rows uses ._rows currently - but could move to primary index
* Cast query parameters to similar types (ie int to uint)
  Will using IComparable<T> work better?


* SQL Parser

* Add MemreDb.IClone interface
  Classes can clone themselves
* Add NoClone attribute, for cloning code to skip fields

* Serialisation


* Potential ideas
* Direct support for soft-deletions, to remove deleted objects from foreign key constraints.
* Multi-threaded support using optimistic locking
