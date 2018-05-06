using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

namespace SqlReflectTest.Tests
{
    [TestClass]
    public class ProductDataMapperToEmitTest : AbstractProductDataMapperTest
    {
        public ProductDataMapperToEmitTest() : base(EmitDataMapper.Build(typeof(Product), NORTHWIND, false), EmitDataMapper.Build(typeof(Supplier), NORTHWIND, false), EmitDataMapper.Build(typeof(Category), NORTHWIND, false))
        {
        }

        [TestMethod]
        public new void TestProductGetAllToEmit()
        {
            base.TestProductGetAll();
        }

        [TestMethod]
        public new void TestProductGetByIdToEmit()
        {
            base.TestProductGetById();
        }


        [TestMethod]
        public new void TestProductInsertAndDeleteToEmit()
        {
            base.TestProductInsertAndDelete();
        }

        [TestMethod]
        public new void TestProductUpdateToEmit()
        {
            base.TestProductUpdate();
        }
    }
}