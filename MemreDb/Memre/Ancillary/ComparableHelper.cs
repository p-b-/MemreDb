using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Ancillary
{
    static internal class ComparableHelper
    {
        static internal IComparable CastToCompatibleType(Type mustMatchType, IComparable alterAndReturn)
        {
            if (mustMatchType == alterAndReturn.GetType())
            {
                return alterAndReturn;
            }
            if (mustMatchType == typeof(uint))
            {
                return CastToUnsignedInt(alterAndReturn);
            }
            throw new Exception($"Cannot compare types {mustMatchType.Name} and {alterAndReturn.GetType()}");
        }

        static internal IComparable CastToCompatibleType(IComparable mustMatch, IComparable alterAndReturn)
        {
            if (mustMatch.GetType() == alterAndReturn.GetType())
            {
                return alterAndReturn;
            }
            if (mustMatch.GetType() == typeof(uint))
            {
                return CastToUnsignedInt(alterAndReturn);
            }
            throw new Exception($"Cannot compare types {mustMatch.GetType()} and {alterAndReturn.GetType()}");
        }

        static IComparable CastToUnsignedInt(IComparable alterAndReturn)
        {
            int toAlterAsInt = (int)alterAndReturn;
            uint toAlterAsUInt = (uint)toAlterAsInt;
            IComparable toReturn = toAlterAsUInt;
            return toReturn;
        }
    }
}
