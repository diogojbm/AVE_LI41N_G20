using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;


namespace SqlReflectTest
{
    public abstract class AbstractRegionDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper regions;

        public AbstractRegionDataMapperTest(IDataMapper regions)
        {
            this.regions = regions;
        }

        public void TestRegionGetAll()
        {
            IEnumerable res = regions.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            Assert.AreEqual(4, count);
        }
        public void TestRegionGetById()
        {
            Region r = (Region)regions.GetById(3);
            r.RegionDescription = r.RegionDescription.Trim();
            Assert.AreEqual("Northern", r.RegionDescription);
        }

        public void TestRegionInsertAndDelete()
        {
            //
            // Create and Insert new Region
            // 
            Region r = new Region()
            {
                RegionID = 156,
                RegionDescription = "Europe"
            };
            object id = regions.Insert(r);
            //
            // Get the new region object from database
            //
            Region actual = (Region)regions.GetById(id);
            actual.RegionDescription = actual.RegionDescription.Trim();
            Assert.AreEqual(r.RegionDescription, actual.RegionDescription);
            //
            // Delete the created region from database
            //
            regions.Delete(actual);
            object res = regions.GetById(id);
            actual = res != null ? (Region)res : default(Region);
            Assert.IsNull(actual.RegionDescription);
        }

        public void TestRegionUpdate()
        {
            Region original = (Region)regions.GetById(3);
            Region modified = new Region()
            {
                RegionID = original.RegionID,
                RegionDescription = "Northern"
            };
            regions.Update(modified);
            Region actual = (Region)regions.GetById(3);
            actual.RegionDescription = actual.RegionDescription.Trim();
            Assert.AreEqual(modified.RegionDescription, actual.RegionDescription);
            regions.Update(original);
            actual = (Region)regions.GetById(3);
            actual.RegionDescription = actual.RegionDescription.Trim();
            Assert.AreEqual("Northern", actual.RegionDescription);
        }
    }
}
