using SqlReflect;
using SqlReflectTest.Model;
using System;
using System.Data;

namespace SqlReflectTest.DataMappers
{
    public class ProductDataMapper : DynamicDataMapper
    {
        /*
        const string COLUMNS = "ProductName, SupplierID, CategoryID, UnitsInStock, UnitsOnOrder, ReorderLevel";
        const string SQL_GET_ALL = "SELECT ProductID, " + COLUMNS + " FROM Products";
        const string SQL_GET_BY_ID = SQL_GET_ALL + " WHERE ProductId=";
        const string SQL_INSERT = "INSERT INTO Products (" + COLUMNS + ") OUTPUT INSERTED.ProductId VALUES ";
        const string SQL_DELETE = "DELETE FROM Products WHERE ProductId = ";
        */
        readonly IDataMapper categories;
        readonly IDataMapper suppliers;

        public ProductDataMapper(Type klass, string connStr, bool withCache) : base(klass,connStr, false){
            categories = new CategoryDataMapper(connStr);
            suppliers = new SupplierDataMapper(connStr);
        }

        protected override object Load(IDataReader dr){
            Product p = new Product();
            if (!dr["ProductID"].Equals(DBNull.Value)) p.ProductID = (int)dr["ProductID"];
            if (!dr["ProductName"].Equals(DBNull.Value)) p.ProductName = (string)dr["ProductName"];
            if (!((Supplier)suppliers.GetById(dr["SupplierID"])).Equals(DBNull.Value)) p.Supplier = (Supplier)suppliers.GetById(dr["SupplierID"]);
            if (!((Category)categories.GetById(dr["CategoryID"])).Equals(DBNull.Value)) p.Category = (Category)categories.GetById(dr["CategoryID"]);
            if (!dr["UnitsInStock"].Equals(DBNull.Value)) p.UnitsInStock = (short)dr["UnitsInStock"];
            if (!dr["UnitsOnOrder"].Equals(DBNull.Value)) p.UnitsOnOrder = (short)dr["UnitsOnOrder"];
            if (!dr["ReorderLevel"].Equals(DBNull.Value)) p.ReorderLevel = (short)dr["ReorderLevel"];
            return p;
        }

        protected override string SqlInsert(object target){
            Product p = (Product)target;
            string values = CheckType(p.ProductName) +
                ", " + CheckType(p.Supplier.SupplierID) +
                ", " + CheckType(p.Category.CategoryID) +
                ", " + CheckType(p.UnitsInStock) +
                ", " + CheckType(p.UnitsOnOrder) +
                ", " + CheckType(p.ReorderLevel);
            return insertStmt + "(" + values + ")";
        }

        protected override string SqlUpdate(object target){
            Product p = (Product)target;
            return String.Format(updateStmt,
                "ProductName = " + CheckType(p.ProductName) +
                ", SupplierID = " + CheckType(p.Supplier.SupplierID) +
                ", CategoryID = " + CheckType(p.Category.CategoryID) +
                ", UnitsInStock = " + CheckType(p.UnitsInStock) +
                ", UnitsInOrder = " + CheckType(p.UnitsOnOrder) +
                ", ReorderLevel = " + CheckType(p.ReorderLevel),
                CheckType(p.ProductID));
        }
        protected override string SqlDelete(object target){
            Product p = (Product)target;
            //deleteStmt + p.ProductID
            return deleteStmt + p.ProductID;
        }
    }
}
