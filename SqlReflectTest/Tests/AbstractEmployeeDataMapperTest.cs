using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest.Tests
{
    public abstract class AbstractEmployeeDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper employees;

        public AbstractEmployeeDataMapperTest(IDataMapper employees)
        {
            this.employees = employees;
        }

        public void TestEmployeeGetAll()
        {
            IEnumerable res = employees.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            Assert.AreEqual(9, count);
        }
        public void TestEmployeeGetById()
        {
            Employee e = (Employee)employees.GetById(3);
            Assert.AreEqual("Janet", e.FirstName);
            Assert.AreEqual("Leverling", e.LastName);
        }

        public void TestEmployeeInsertAndDelete()
        {
            //
            // Create and Insert new Employee
            // 
            Employee e = new Employee()
            {
                LastName = "Bernardo",
                FirstName = "Chanty",
                Title = "CEO",
                TitleOfCourtesy = "Mr",
                Address = "Rua Joao Villaret",
                City = "Lisbon",
                Region = "Europe",
                PostalCode = "2620",
                Country = "Portugal",
                HomePhone = "(351) 961234567",
                Extension = "651"
            };
            object id = employees.Insert(e);
            //
            // Get the new employee object from database
            //
            Employee actual = (Employee)employees.GetById(id);
            Assert.AreEqual(e.FirstName, actual.FirstName);
            //
            // Delete the created employee from database
            //
            employees.Delete(actual);
            object res = employees.GetById(id);
            actual = res != null ? (Employee)res : default(Employee);
            Assert.IsNull(actual.FirstName);
            Assert.IsNull(actual.LastName);
        }

        public void TestEmployeeUpdate()
        {
            Employee original = (Employee)employees.GetById(3);
            Employee modified = new Employee()
            {
                EmployeeID = original.EmployeeID,
                FirstName = "Roberto",
                LastName = "Rei",
                Title = "Owner",
                TitleOfCourtesy = "Mr",
                Address = "Schreider",
                City = "Munich",
                Region = "Europe",
                PostalCode = "80331",
                Country = "Alemanha",
                HomePhone = "40250616224256",
                Extension = "8923"
            };
            employees.Update(modified);
            Employee actual = (Employee)employees.GetById(3);
            Assert.AreEqual(modified.FirstName, actual.FirstName);
            Assert.AreEqual(modified.LastName, actual.LastName);
            employees.Update(original);
            actual = (Employee)employees.GetById(3);
            Assert.AreEqual("Janet", actual.FirstName);
            Assert.AreEqual("Leverling", actual.LastName);
        }
    }
}
