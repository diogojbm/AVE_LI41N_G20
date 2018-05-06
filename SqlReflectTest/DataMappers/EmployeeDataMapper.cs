using SqlReflect;
using System;
using System.Data;
using SqlReflectTest.Model;

namespace SqlReflectTest.DataMappers
{
    class EmployeeDataMapper : DynamicDataMapper
    {
        public EmployeeDataMapper(Type klass, string connStr, bool withCache) : base(klass, connStr, withCache)
        {
        }
        protected override object Load(IDataReader dr)
        {
            Employee e = new Employee();
            if (!dr["EmployeeID"].Equals(DBNull.Value)) e.EmployeeID = (int)dr["EmployeeID"];
            if (!dr["LastName"].Equals(DBNull.Value)) e.LastName = (string)dr["LastName"];
            if (!dr["FirstName"].Equals(DBNull.Value)) e.FirstName = (string)dr["FirstName"];
            if (!dr["Title"].Equals(DBNull.Value)) e.Title = (string)dr["Title"];
            if (!dr["TitleOfCourtesy"].Equals(DBNull.Value)) e.TitleOfCourtesy = (string)dr["TitleOfCourtesy"];
            if (!dr["Address"].Equals(DBNull.Value)) e.Address = (string)dr["Address"];
            if (!dr["City"].Equals(DBNull.Value)) e.City = (string)dr["City"];
            if (!dr["Region"].Equals(DBNull.Value)) e.Region = (string)dr["Region"];
            if (!dr["PostalCode"].Equals(DBNull.Value)) e.PostalCode = (string)dr["PostalCode"];
            if (!dr["Country"].Equals(DBNull.Value)) e.Country = (string)dr["Country"];
            if (!dr["HomePhone"].Equals(DBNull.Value)) e.HomePhone = (string)dr["HomePhone"];
            if (!dr["Extension"].Equals(DBNull.Value)) e.Extension = (string)dr["Extension"];
            return e;
        }

        protected override string SqlDelete(object target)
        {
            Employee e = (Employee)target;
            return deleteStmt + CheckType(e.EmployeeID);
        }

        protected override string SqlInsert(object target)
        {
            Employee e = (Employee)target;
            string values = CheckType(e.LastName) + ", " + CheckType(e.FirstName) + ", " + CheckType(e.Title) + ", " + CheckType(e.TitleOfCourtesy) + ", " + CheckType(e.Address) + ", " + CheckType(e.City) + ", " + CheckType(e.Region) + ", " + CheckType(e.PostalCode) + ", " + CheckType(e.Country) + ", " + CheckType(e.HomePhone) + ", " + CheckType(e.Extension);
            return insertStmt + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Employee e = (Employee)target;
            return String.Format(updateStmt, 
                "LastName = " + CheckType(e.LastName) + 
                ", FirstName = " + CheckType(e.FirstName) + 
                ", Title = " + CheckType(e.Title) +
                ", TitleOfCourtesy = " + CheckType(e.TitleOfCourtesy) +
                ", Address = " + CheckType(e.Address) +
                ", City = " + CheckType(e.City) +
                ", Region = " + CheckType(e.Region) +
                ", PostalCode = " + CheckType(e.PostalCode) +
                ", Country = " + CheckType(e.Country) +
                ", HomePhone = " + CheckType(e.HomePhone) +
                ", Extension = " + CheckType(e.Extension)
                , CheckType(e.EmployeeID));
        }
    }
}
