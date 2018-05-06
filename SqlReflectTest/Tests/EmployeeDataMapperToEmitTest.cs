using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest.Tests
{
    [TestClass]
    public class EmployeeDataMapperToEmitTest : AbstractEmployeeDataMapperTest
    {
        public EmployeeDataMapperToEmitTest() : base(EmitDataMapper.Build(typeof(Employee), NORTHWIND, false))
        {
        }

        [TestMethod]
        public new void TestEmployeeGetAllToEmit()
        {
            base.TestEmployeeGetAll();
        }

        [TestMethod]
        public new void TestEmployeeGetByIdToEmit()
        {
            base.TestEmployeeGetById();
        }


        [TestMethod]
        public new void TestEmployeeInsertAndDeleteToEmit()
        {
            base.TestEmployeeInsertAndDelete();
        }

        [TestMethod]
        public new void TestEmployeeUpdateToEmit()
        {
            base.TestEmployeeUpdate();
        }
    }
}