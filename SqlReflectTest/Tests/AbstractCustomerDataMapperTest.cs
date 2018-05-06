using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest.Tests
{
    public abstract class AbstractCustomerDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper customers;

        public AbstractCustomerDataMapperTest(IDataMapper customers)
        {
            this.customers = customers;
        }

        public void TestCustomerGetAll()
        {
            IEnumerable res = customers.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            Assert.AreEqual(91, count);
        }
        public void TestCustomerGetById()
        {
            Customer c = (Customer)customers.GetById("CHOPS");
            Assert.AreEqual("Chop-suey Chinese", c.CompanyName);
        }

        public void TestCustomerInsertAndDelete()
        {
            //
            // Create and Insert new Customer
            // 
            Customer c = new Customer()
            {
                CustomerID = "COOKS",
                CompanyName = "Bolacha Maria",
                ContactName = "Maria",
                ContactTitle = "Owner",
                Address = "Rua Combatentes do Ultramar", 
                City = "Lisbon",
                Region = "NULL",
                PostalCode = "2675",
                Country = "Portugal", 
                Phone = "(351) 965135810",
                Fax = "(351) 966027619"
            };
            object id = customers.Insert(c);
            //
            // Get the new customer object from database
            //
            Customer actual = (Customer)customers.GetById(id);
            Assert.AreEqual(c.CustomerID, actual.CustomerID);
            Assert.AreEqual(c.CompanyName, actual.CompanyName);
            //
            // Delete the created customer from database
            //
            customers.Delete(actual);
            object res = customers.GetById(id);
            Assert.IsNull(res);
        }

        public void TestCustomerUpdate()
        {
            Customer original = (Customer)customers.GetById("CHOPS");
            Customer modified = new Customer()
            {
                CustomerID = original.CustomerID,
                CompanyName = "C-Suey",
                ContactName = "Yu Dabao",
                ContactTitle = "Sales Assistent",
                Address = "Hauptstqeplr",
                City = "Luzern",
                Region = "Europe",
                PostalCode = "3013",
                Country = "Switzerland",
                Fax = "0454-862145",
                Phone = "0454-135365"
            };
            customers.Update(modified);
            Customer actual = (Customer)customers.GetById("CHOPS");
            Assert.AreEqual(modified.CompanyName, actual.CompanyName);
            customers.Update(original);
            actual = (Customer)customers.GetById("CHOPS");
            Assert.AreEqual("Chop-suey Chinese", actual.CompanyName);
        }
    }
}
