using SqlReflect;
using System;
using System.Data;
using SqlReflectTest.Model;

namespace SqlReflectTest.DataMappers
{
    class CustomerDataMapper : DynamicDataMapper
    {
        public CustomerDataMapper(Type klass, string connStr, bool withCache) : base(klass, connStr, withCache)
        {
        }

        protected override object Load(IDataReader dr){
            Customer c = new Customer();
            if (!dr["CustomerID"].Equals(DBNull.Value)) c.CustomerID = (string)dr["CustomerID"];
            if (!dr["CompanyName"].Equals(DBNull.Value)) c.CompanyName = (string)dr["CompanyName"];
            if (!dr["ContactName"].Equals(DBNull.Value)) c.ContactName = (string)dr["ContactName"];
            if (!dr["ContactTitle"].Equals(DBNull.Value)) c.ContactTitle = (string)dr["ContactTitle"];
            if (!dr["Address"].Equals(DBNull.Value)) c.Address = (string)dr["Address"];
            if (!dr["City"].Equals(DBNull.Value)) c.City = (string)dr["City"];
            if (!dr["Region"].Equals(DBNull.Value)) c.Region = (string)dr["Region"];
            if (!dr["PostalCode"].Equals(DBNull.Value)) c.PostalCode = (string)dr["PostalCode"];
            if (!dr["Country"].Equals(DBNull.Value)) c.Country = (string)dr["Country"];
            if (!dr["Phone"].Equals(DBNull.Value)) c.Phone = (string)dr["Phone"];
            if (!dr["Fax"].Equals(DBNull.Value)) c.Fax = (string)dr["Fax"];
            return c;
        }

        protected override string SqlDelete(object target)
        {
            Customer c = (Customer)target;
            return deleteStmt + CheckType(c.CustomerID);
        }

        protected override string SqlInsert(object target)
        {
            Customer c = (Customer)target;
            string values = CheckType(c.CustomerID) +
                "," + CheckType(c.CompanyName) +
                ", " + CheckType(c.ContactName) +
                ", " + CheckType(c.ContactTitle) +
                ", " + CheckType(c.Address) +
                ", " + CheckType(c.City) +
                ", " + CheckType(c.Region) +
                ", " + CheckType(c.PostalCode) +
                ", " + CheckType(c.Country) +
                ", " + CheckType(c.Phone) +
                ", " + CheckType(c.Fax);
            return insertStmt + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Customer c = (Customer)target;
            return String.Format(updateStmt, 
                "CustomerID = " + CheckType(c.CustomerID) +
                ", CompanyName = " + CheckType(c.CompanyName) +
                ", ContactName = " + CheckType(c.ContactName) +
                ", ContactTitle = " + CheckType(c.ContactTitle) +
                ", Address = " + CheckType(c.Address) +
                ", City = " + CheckType(c.City) + 
                ", Region = " + CheckType(c.Region) + 
                ", PostalCode = " + CheckType(c.PostalCode) +
                ", Country = " + CheckType(c.Country) +
                ", Phone = " + CheckType(c.Phone) +
                ", Fax = " + CheckType(c.Fax),
                CheckType(c.CustomerID));
        }
    }
}
