using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    public interface IDataMapper<K,V> : IDataMapper
    {
        V getById(K id);
        new IEnumerable<V> GetAll();
        K Insert(V target);
        void Update(V target);
        void Delete(V target);
    }
}
