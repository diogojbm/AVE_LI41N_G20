using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    class Mappers
    {
        private static readonly Dictionary<Type, ReflectDataMapper> mappersDic = new Dictionary<Type, ReflectDataMapper>();
        public static ReflectDataMapper GetMapper(Type t, string connStr) {
            ReflectDataMapper rdm;
            if (!mappersDic.TryGetValue(t, out rdm)){
                rdm = new ReflectDataMapper(t, connStr);
                mappersDic.Add(t, rdm);
            }
            return rdm;
        }
    }
}
