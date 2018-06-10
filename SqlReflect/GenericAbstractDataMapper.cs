using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    public abstract class AbstractDataMapper<K,V> : IDataMapper<K, V>
    {
        //readonly string connStr;
        //readonly DataSet cache;

        /*public AbstractDataMapper(string connStr) : this(connStr, true)
        {
        }

        public AbstractDataMapper(string connStr, bool withCache)
        {
            withCache = false;
            this.connStr = connStr;
            if (withCache) cache = new DataSet();
        }*/

        protected abstract string SqlGetAll();
        protected abstract string SqlGetById(K id);
        protected abstract string SqlInsert(V target);
        protected abstract string SqlDelete(V target);
        protected abstract string SqlUpdate(V target);

        protected abstract object Load(IDataReader dr);

        V IDataMapper<K, V>.getById(K id)
        {
            throw new NotImplementedException();
        }

        IEnumerable<V> IDataMapper<K, V>.GetAll()
        {
            throw new NotImplementedException();
        }

        K IDataMapper<K, V>.Insert(V target)
        {
            throw new NotImplementedException();
        }

        void IDataMapper<K, V>.Update(V target)
        {
            throw new NotImplementedException();
        }

        void IDataMapper<K, V>.Delete(V target)
        {
            throw new NotImplementedException();
        }

        public object GetById(object id)
        {
            throw new NotImplementedException();
        }

        IEnumerable IDataMapper.GetAll()
        {
            throw new NotImplementedException();
        }

        public object Insert(object target)
        {
            throw new NotImplementedException();
        }

        public void Update(object target)
        {
            throw new NotImplementedException();
        }

        public void Delete(object target)
        {
            throw new NotImplementedException();
        }
    }
}
