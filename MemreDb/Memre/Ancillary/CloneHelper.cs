using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Ancillary
{
    static internal class CloneHelper
    {
        static internal T CloneObject<T>(T objectToClone) where T : new()
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
    }
}
