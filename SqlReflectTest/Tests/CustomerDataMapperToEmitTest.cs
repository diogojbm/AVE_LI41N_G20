using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest.Tests
{
    [TestClass]
    public class CustomerDataMapperToEmitTest : AbstractCustomerDataMapperTest
    {
        public CustomerDataMapperToEmitTest() : base(EmitDataMapper.Build(typeof(Customer), NORTHWIND, false))
        {
        }

        [TestMethod]
        public new void TestCustomerGetAllToEmit()
        {
            base.TestCustomerGetAll();
        }

        [TestMethod]
        public new void TestCustomerGetByIdToEmit()
        {
            base.TestCustomerGetById();
        }


        [TestMethod]
        public new void TestCustomerInsertAndDeleteToEmit()
        {
            base.TestCustomerInsertAndDelete();
        }

        [TestMethod]
        public new void TestCustomerUpdateToEmit()
        {
            base.TestCustomerUpdate();
        }
    }
}