using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dialupconnmgr.Common.Extenssions
{
    public static class TypeExtensions
    {
        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            else
                return null;
        }
    }
}
