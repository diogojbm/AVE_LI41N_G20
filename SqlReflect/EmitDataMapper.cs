using System.Reflection.Emit;
using System;
using System.Reflection;
using System.Data;
using System.Linq;
using SqlReflect.Attributes;

namespace SqlReflect
{
    public class EmitDataMapper
    {
        private static readonly FieldInfo dbnullValue = typeof(DBNull).GetField("Value");
        private static readonly MethodInfo get_Item = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(string) });
        private static readonly MethodInfo equals = typeof(object).GetMethod("Equals", new Type[] { typeof(object) });
        private static readonly MethodInfo getById = typeof(IDataMapper).GetMethod("GetByID", new Type[] { typeof(object) });
        private static readonly MethodInfo checkType = typeof(AbstractDataMapper).GetMethod("CheckType", new Type[] { typeof(object) });
        private static readonly MethodInfo concatTwoStrings = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
        private static readonly MethodInfo concatStringArray = typeof(string).GetMethod("Concat", new Type[] { typeof(string[])});
        private static readonly MethodInfo format = typeof(String).GetMethod("Format", new Type[] { typeof(string), typeof(object), typeof(object) });
        private static readonly FieldInfo updateStmt = typeof(DynamicDataMapper).GetField("updateStmt", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo deleteStmt = typeof(DynamicDataMapper).GetField("deleteStmt", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo[] properties;
        private static PropertyInfo pk;
        private static AssemblyName asmName;
        private static AssemblyBuilder aBld;
        private static ModuleBuilder mb;
        private static TypeBuilder tb;

        //No teste, passar como parâmetro o EmitDataMapperBuild();
        public static DynamicDataMapper Build(Type klass, string connStr, bool withCache){
            //AssemblyBuilder, ModuleBuilder, TypeBuilder, MethodBuilder
            BuildDDM(klass);
            pk = klass.GetProperties().First(p => p.IsDefined(typeof(PKAttribute)));

            PKAttribute pkAtt = (PKAttribute)pk.GetCustomAttribute(typeof(PKAttribute));
            if ((pkAtt.AutoIncrement)){
                properties = klass.GetProperties().Where(p => p != pk).ToArray();
            }
            else properties = klass.GetProperties();
            /*
              Construir os campos
              readonly IDataMapper categories;
              readonly IDataMapper suppliers;
              FieldBuilder categories = tb.DefineField(...)
              BuildFields(tb);
             */

            BuildMethodConstructor(tb);
            BuildMethodLoad(klass, tb);
            BuildMethodInsert(klass, tb, properties);
            BuildMethodDelete(klass, tb);
            BuildMethodUpdate(klass, tb, properties);

            Type t = tb.CreateType();
            aBld.Save(asmName.Name + ".dll");

            //Create an Instance of type t, with the 3 following parameters passed to the constructor
            return (DynamicDataMapper)Activator.CreateInstance(t, klass, connStr, withCache);
        }

        private static void BuildDDM(Type mKlass){
            asmName = new AssemblyName("DDM" + mKlass.Name);
            aBld = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            mb = aBld.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");
            tb = mb.DefineType("DDM" + mKlass.Name, TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit, typeof(DynamicDataMapper));
        }

        static void BuildMethodConstructor(TypeBuilder tb){
            Type t = typeof(DynamicDataMapper);
            ConstructorInfo baseConstructor = t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Type), typeof(string), typeof(bool) }, null);
            ConstructorBuilder cb = tb.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |  MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[] { typeof(Type), typeof(string), typeof(bool) });
            ILGenerator il = cb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, baseConstructor);

            // afetar fields

            il.Emit(OpCodes.Ret);
        }

        static void BuildMethodLoad(Type modelKlass, TypeBuilder tb){
            MethodBuilder mb = tb.DefineMethod("Load", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.ReuseSlot, typeof(object), new Type[] {typeof(IDataReader) });
            ILGenerator il = mb.GetILGenerator();
            if (typeof(ValueType).IsAssignableFrom(modelKlass)) BuildLoadToEmitValueType(modelKlass, tb, mb, il);
            else BuildLoadToEmitReferenceType(modelKlass, tb, mb, il);
        }

        private static void BuildLoadToEmitValueType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il){
            LocalBuilder modelObj = il.DeclareLocal(modelKlass);
            il.Emit(OpCodes.Ldloca_S, modelObj);
            // il.Emit(OpCodes.Initobj, modelKlass.GetConstructor(Type.EmptyTypes)); --> not possible because initobj does not call constructor method
            il.Emit(OpCodes.Initobj, modelKlass);
            
            foreach (PropertyInfo p in modelKlass.GetProperties()){
                Label l = il.DefineLabel();
                if (DynamicDataMapper.IsADBEntity(p.PropertyType)) SetComplexProperty(modelKlass, p, modelObj, il, l);
                SetSimplePropertyVT(modelKlass, p, modelObj, il, l);
            }

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Box, modelKlass);
            il.Emit(OpCodes.Ret);
        }

        private static void SetComplexProperty(Type modelKlass, PropertyInfo p, LocalBuilder modelObj, ILGenerator il, Label l) {
            LocalBuilder propObj = il.DeclareLocal(p.PropertyType);
            il.Emit(OpCodes.Ldarg_0);
            //il.Emit(OpCodes.Ldfld, typeof(IDataMapper).SqlReflectTest.DataMappers.ProductDataMapper::suppliers);
            il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Ldstr, "SupplierID");
            il.Emit(OpCodes.Callvirt, get_Item);
            il.Emit(OpCodes.Callvirt, getById);
            if (typeof(ValueType).IsAssignableFrom(p.PropertyType)) {
                il.Emit(OpCodes.Unbox_Any, p.PropertyType);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloca_S, propObj);
                il.Emit(OpCodes.Ldsfld, dbnullValue);
                il.Emit(OpCodes.Constrained, p.PropertyType);
            }
            else {
                il.Emit(OpCodes.Castclass, p.PropertyType);
                il.Emit(OpCodes.Ldsfld, dbnullValue);
            }
            il.Emit(OpCodes.Callvirt, equals);
            il.Emit(OpCodes.Brtrue_S, l);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_0);
            //il.Emit(OpCodes.Ldfld SqlReflect.IDataMapper SqlReflectTest.DataMappers.ProductDataMapper::suppliers
            il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Ldstr, "SupplierID");
            il.Emit(OpCodes.Callvirt, get_Item);
            il.Emit(OpCodes.Callvirt, getById);
            if (typeof(ValueType).IsAssignableFrom(p.PropertyType))
                il.Emit(OpCodes.Castclass, p.PropertyType);
            else
                il.Emit(OpCodes.Unbox_Any, p.PropertyType);
            il.Emit(OpCodes.Callvirt, p.GetSetMethod());
        }

        private static void SetSimplePropertyVT(Type modelKlass, PropertyInfo p, LocalBuilder modelObj, ILGenerator il, Label l){
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, p.Name);
            il.Emit(OpCodes.Callvirt, get_Item);
            il.Emit(OpCodes.Ldsfld, dbnullValue);
            il.Emit(OpCodes.Callvirt, equals);
            il.Emit(OpCodes.Brtrue_S, l);
            il.Emit(OpCodes.Ldloca_S, modelObj);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, p.Name);
            il.Emit(OpCodes.Callvirt, get_Item);

            if (typeof(ValueType).IsAssignableFrom(p.PropertyType))
                il.Emit(OpCodes.Unbox_Any, p.PropertyType);
            else
                il.Emit(OpCodes.Castclass, p.PropertyType);

            il.Emit(OpCodes.Callvirt, p.GetSetMethod());
            il.MarkLabel(l);
        }

        private static void BuildLoadToEmitReferenceType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il){
            LocalBuilder modelObj = il.DeclareLocal(modelKlass);
            il.Emit(OpCodes.Newobj, modelKlass.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);

            foreach (PropertyInfo p in modelKlass.GetProperties()){
                Label l = il.DefineLabel();
                SetSimplePropertyRT(modelKlass, p, modelObj, il, l);
            }

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
        }

        private static void SetSimplePropertyRT(Type modelKlass, PropertyInfo p, LocalBuilder modelObj, ILGenerator il, Label l){
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, p.Name);
            il.Emit(OpCodes.Callvirt, get_Item);
            il.Emit(OpCodes.Ldsfld, dbnullValue);
            il.Emit(OpCodes.Callvirt, equals);
            il.Emit(OpCodes.Brtrue_S, l);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, p.Name);
            il.Emit(OpCodes.Callvirt, get_Item);

            if (typeof(ValueType).IsAssignableFrom(p.PropertyType))
                il.Emit(OpCodes.Unbox_Any, p.PropertyType);
            else
                il.Emit(OpCodes.Castclass, p.PropertyType);

            il.Emit(OpCodes.Callvirt, p.GetSetMethod());
            il.MarkLabel(l);
        }

        static void BuildMethodInsert(Type modelKlass, TypeBuilder tb, PropertyInfo[] properties){
            MethodBuilder mb = tb.DefineMethod("SqlInsert", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), new Type[] { typeof(object) });
            ILGenerator il = mb.GetILGenerator();

            LocalBuilder modelObj = il.DeclareLocal(modelKlass);
            il.DeclareLocal(typeof(string));

            if (typeof(ValueType).IsAssignableFrom(modelKlass)) BuildInsertToEmitValueType(modelKlass, tb, mb, il, properties, modelObj);
            else BuildInsertToEmitReferenceType(modelKlass, tb, mb, il, properties);
             
            il.Emit(OpCodes.Ret);
        }

        private static void BuildInsertToEmitValueType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il, PropertyInfo[] properties, LocalBuilder modelObj){
            int count = 0;
            int temp = properties.Count() * 2 - 1;

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, modelKlass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_S, temp);
            il.Emit(OpCodes.Newarr, typeof(string));
            
            foreach (PropertyInfo p in properties)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, count++);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloca_S, modelObj);
                il.Emit(OpCodes.Call, p.GetGetMethod());
                il.Emit(OpCodes.Call, typeof(AbstractDataMapper).GetMethod("CheckType", BindingFlags.Instance | BindingFlags.Public));
                il.Emit(OpCodes.Stelem_Ref);
                if (count < temp)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4_S, count++);
                    il.Emit(OpCodes.Ldstr, ",");
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string[]) }));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(DynamicDataMapper).GetField("insertStmt", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ldstr, "(");
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldstr, ")");
            il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }));
        }

        private static void BuildInsertToEmitReferenceType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il, PropertyInfo[] properties){
            int count = 0;
            //para não incluir a última vírgula
            int temp = properties.Count() * 2 - 1;
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, modelKlass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_S, temp);
            il.Emit(OpCodes.Newarr, typeof(string));

            foreach (PropertyInfo p in properties)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, count++);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, p.GetGetMethod());
                il.Emit(OpCodes.Call, typeof(AbstractDataMapper).GetMethod("CheckType", BindingFlags.Instance | BindingFlags.Public));
                il.Emit(OpCodes.Stelem_Ref);
                if (count < temp){
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4_S, count++);
                    il.Emit(OpCodes.Ldstr, ",");
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string[]) }));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(DynamicDataMapper).GetField("insertStmt", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ldstr, "(");
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldstr, ")");
            il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }));
        }

        static void BuildMethodDelete(Type modelKlass, TypeBuilder tb){
            MethodBuilder mb = tb.DefineMethod("SqlDelete", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.ReuseSlot, typeof(string), new Type[] { typeof(object) });
            ILGenerator il = mb.GetILGenerator();
            LocalBuilder modelObj = il.DeclareLocal(modelKlass);

            if (typeof(ValueType).IsAssignableFrom(modelKlass)) BuildDeleteToEmitValueType(modelKlass, tb, mb, il, modelObj);
            else BuildDeleteToEmitReferenceType(modelKlass, tb, mb, il);

            il.Emit(OpCodes.Ret);
        }

        private static void BuildDeleteToEmitValueType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il, LocalBuilder modelObj){
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, modelKlass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, deleteStmt);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, modelObj);
            il.Emit(OpCodes.Call, pk.GetGetMethod());
            il.Emit(OpCodes.Box, pk.PropertyType);
            il.Emit(OpCodes.Call, checkType);
            il.Emit(OpCodes.Call, concatTwoStrings);
        }

        private static void BuildDeleteToEmitReferenceType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il){
            PropertyInfo pk = modelKlass.GetProperties().First(p => p.IsDefined(typeof(PKAttribute)));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, modelKlass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, deleteStmt);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, pk.GetGetMethod());
            il.Emit(OpCodes.Call, checkType);
            il.Emit(OpCodes.Call, concatTwoStrings);
        }

        static void BuildMethodUpdate(Type modelKlass, TypeBuilder tb, PropertyInfo[] properties){
            MethodBuilder mb = tb.DefineMethod("SqlUpdate", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.ReuseSlot, typeof(string), new Type[] { typeof(object) });
            ILGenerator il = mb.GetILGenerator();
            LocalBuilder modelObj = il.DeclareLocal(modelKlass);

            il.Emit(OpCodes.Ldarg_1);
            if (typeof(ValueType).IsAssignableFrom(modelKlass)) BuildUpdateToEmitValueType(modelKlass, tb, mb, il, properties, modelObj);
            else BuildUpdateToEmitReferenceType(modelKlass, tb, mb, il, properties);
            il.Emit(OpCodes.Ret);
        }

        private static void BuildUpdateToEmitValueType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il, PropertyInfo[] properties, LocalBuilder modelObj){
            int temp = properties.Count() * 2;
            int count = 0;

            il.Emit(OpCodes.Unbox_Any, modelKlass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, updateStmt);
            il.Emit(OpCodes.Ldc_I4_S, temp);
            il.Emit(OpCodes.Newarr, typeof(string));
            foreach (PropertyInfo p in properties){
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, count++);
                if (count < 2) il.Emit(OpCodes.Ldstr, p.Name + " = ");
                else il.Emit(OpCodes.Ldstr, ", " + p.Name + " = ");
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, count++);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloca_S, modelObj);
                il.Emit(OpCodes.Call, p.GetGetMethod());
                il.Emit(OpCodes.Call, checkType);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Call, concatStringArray);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, modelObj);
            il.Emit(OpCodes.Call, pk.GetGetMethod());
            il.Emit(OpCodes.Box, pk.PropertyType);
            il.Emit(OpCodes.Call, checkType);
            il.Emit(OpCodes.Call, format);
        }

        private static void BuildUpdateToEmitReferenceType(Type modelKlass, TypeBuilder tb, MethodBuilder mb, ILGenerator il, PropertyInfo[] properties) {
            int temp = properties.Count() * 2;
            int count = 0;

            il.Emit(OpCodes.Castclass, modelKlass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, updateStmt);
            il.Emit(OpCodes.Ldc_I4_S, temp);
            il.Emit(OpCodes.Newarr, typeof(string));
            foreach (PropertyInfo p in properties) {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, count++);
                if(count < 2) il.Emit(OpCodes.Ldstr, p.Name + " = ");
                else il.Emit(OpCodes.Ldstr, ", " + p.Name + " = ");
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, count++);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, p.GetGetMethod());
                il.Emit(OpCodes.Call, checkType);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Call, concatStringArray);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, pk.GetGetMethod());
            il.Emit(OpCodes.Call, checkType);
            il.Emit(OpCodes.Call, format);
        }
    }
}