using MemreDb.Memre.Indices.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices
{
    internal interface ITreeIndex
    {
        bool Empty { get; }
        Object GetAnyValue();
        void Update(IComparable indexValue, object value);
        void Delete(IComparable indexValue);
        void Delete(IComparable indexValue, Object valueToDelete);


        object GetValueSingular(IComparable indexValue);
        List<object> GetValuesEqualsAsList(IComparable indexValue);

        void WriteTreeToConsole();
        void WriteTreeAsOrderedListToConsole();
        void WriteTreeAsOrderedListToConsoleUsingIterators(bool ascending);
    }
}
