using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Tests
{
    class TestC<T>
    where T : IComparable<T>
    {
        internal T Index { get; set; }
        internal object Value { get; set; }
        internal TestC(T createWithIndex, object createWithValue)
        {
            Index = createWithIndex;
            Value = createWithValue;
        }

        internal bool IsLessThan(TestC<T> rhs)
        {
            int comparison = Index.CompareTo(rhs.Index);
            return comparison < 0;
        }

        internal bool IsLessThan(IComparable<T> rhs)
        {
            int comparison = Index.CompareTo((T)rhs);
            return comparison < 0;
        }
    }

    class TestD
    {
        internal IComparable Index { get; set; }
        internal object Value { get; set; }
        internal TestD(IComparable createWithIndex, object createWithValue)
        {
            Index = createWithIndex;
            Value = createWithValue;
        }

        internal bool IsLessThan(TestD rhs)
        {
            int comparison = Index.CompareTo(rhs.Index);
            return comparison < 0;
        }

        internal bool IsLessThan(IComparable rhs)
        {
            int comparison = Index.CompareTo(rhs);
            return comparison < 0;
        }
    }

    class IComparableTests
    {
        static internal bool LHSIsSmaller<T>(TestC<T> lhs, TestC<T> rhs)
            where T : IComparable<T>
        {
            return lhs.IsLessThan(rhs);
        }

        static internal bool LHSIsSmaller<T>(TestC<T> lhs, IComparable rhs)
            where T : IComparable<T>
        {
            T lhsToUse = lhs.Index;
            T rhsToUse = default;
            if (lhs.Index.GetType() == rhs.GetType())
            {
                rhsToUse = (T)rhs;
            }
            else
            {
                if (lhs.Index.GetType() == typeof(int) &&
                    rhs.GetType() == typeof(uint))
                {
                    uint rhsAsBasicType = (uint)rhs;
                    int rhsAsSignedType = (int)rhsAsBasicType;
                    IComparable rhsAsIComparable = rhsAsSignedType;
                    rhsToUse = (T)rhsAsIComparable;
                }
                else
                {
                    throw new Exception("Cannot make comparison");
                }
            }
            return lhs.IsLessThan(rhsToUse);
        }

        static internal bool LHSIsSmaller(TestD lhs, TestD rhs)
        {
            return lhs.IsLessThan(rhs);
        }

        static internal bool LHSIsSmaller(TestD lhs, IComparable rhs)
        {
            IComparable lhsToUse = lhs.Index;
            IComparable rhsToUse = default;
            if (lhs.Index.GetType() == rhs.GetType())
            {
                rhsToUse = (IComparable)rhs;
            }
            else
            {
                if (lhs.Index.GetType() == typeof(int) &&
                    rhs.GetType() == typeof(uint))
                {
                    uint rhsAsBasicType = (uint)rhs;
                    int rhsAsSignedType = (int)rhsAsBasicType;
                    IComparable rhsAsIComparable = rhsAsSignedType;
                    rhsToUse = rhsAsIComparable;
                }
                else
                {
                    throw new Exception("Cannot make comparison");
                }
            }
            return lhs.IsLessThan(rhsToUse);
        }
    }
}
