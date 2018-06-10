using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    public class TypeProperties
    {
        static Dictionary<Type, PropertySet> properties = new Dictionary<Type, PropertySet>();

        public static PropertySet GetPS(Type t, string connStr) {
            PropertySet ps;
            bool b = properties.TryGetValue(t, out ps);
            if (!b){
                ps = new PropertySet(t, connStr);
                properties.Add(t, ps);
            }    
            return ps;
        }
    }
}
