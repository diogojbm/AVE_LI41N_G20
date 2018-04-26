using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    class EmployeeDataMapper : DynamicDataMapper
    {
        public EmployeeDataMapper(Type klass, string connStr, bool withCache) : base(klass, connStr, withCache)
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override object Load(IDataReader dr)
        {
            throw new NotImplementedException();
        }

        protected override string SqlDelete(object target)
        {
            return deleteStmt;
        }

        protected override string SqlGetAll()
        {
            return getAllStmt;
        }

        protected override string SqlGetById(object id)
        {
            return getByIdStmt;
        }

        protected override string SqlInsert(object target)
        {
            return insertStmt;
        }

        protected override string SqlUpdate(object target)
        {
            return updateStmt;
        }
    }
}
