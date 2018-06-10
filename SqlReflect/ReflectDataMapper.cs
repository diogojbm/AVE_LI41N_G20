using System;
using System.Data;
using System.Reflection;
using System.Text;
using SqlReflect.Attributes;


namespace SqlReflect
{
    public class ReflectDataMapper : AbstractDataMapper
    {
        Type klass;
        PropertySet ps;
        TableAttribute table;
        PropertyInfo[] properties;
        string TABLE_NAME;
        string columns;
        string columns_except_pk;
        private string PK_NAME;
        private bool noIdentity = false;
        private string connStr;
        
        public ReflectDataMapper(Type klass, string connStr, bool withCache) : base(connStr, withCache)
        {
        }

        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            this.klass = klass;
            this.connStr = connStr;
            ps = TypeProperties.GetPS(klass, connStr);
            table = (TableAttribute)klass.GetCustomAttribute(typeof(TableAttribute));
            TABLE_NAME = table.Name;
            properties = ps.GetProperties();
            PK_NAME = GetPKName();
            BuildQueries(TABLE_NAME);
        }

        void BuildQueries(string table_name)
        {
            columns = ps.Columns;
            columns_except_pk = ps.ColumnsExceptPk;
        }

        public string GetPKName()
        {
            string aux = "";
            foreach (PropertyInfo p in properties){
                if (p.IsDefined(typeof(PKAttribute))){
                    aux = p.Name;
                    break;
                }
            }
            return aux;
        }

        public PropertyInfo GetPK()
        {
            foreach (PropertyInfo p in properties){
                if (p.IsDefined(typeof(PKAttribute))) return p;
            }
            return null;
        }

        protected override object Load(IDataReader dr)
        {
            object instance = Activator.CreateInstance(klass);
            string complexPropertyName;
            foreach (PropertyInfo p in properties){
                Type t = p.PropertyType;
                if (ps.IsADBEntity(t)){
                    ReflectDataMapper rdm = Mappers.GetMapper(t, connStr);
                    complexPropertyName = rdm.GetPKName();
                    // e.g. "SupplierID" propertyInfo
                    //PropertyInfo complexPI = rdm.GetPK();
                    PropertyInfo complexPI = p;

                    object complexObject = rdm.GetById(dr[complexPropertyName]);
                    complexPI.SetValue(instance, complexObject);
                }
                else{
                    if (!(dr[p.Name] is DBNull)) p.SetValue(instance, dr[p.Name]);
                }
            }
            return instance;
        }

        protected override string SqlGetAll()
        {
            return @"SELECT " + columns +  " FROM " + TABLE_NAME;
        }

        protected override string SqlGetById(object id)
        {
            return @"SELECT " + columns + " FROM " + TABLE_NAME + " WHERE " + PK_NAME + "=" + id;
        }

        protected override string SqlInsert(object target)
        {
            object val;
            StringBuilder values = new StringBuilder("");
            int count = 0;
            for (int i = 0; i < properties.Length; ++i){
                if (!properties[i].IsDefined(typeof(PKAttribute))) {
                    if (count == 0) values.Append("'");
                    Type t = properties[i].PropertyType;
                    if (ps.IsADBEntity(t)){
                        object complexObj = properties[i].GetValue(target);
                        PropertySet ps = TypeProperties.GetPS(t, connStr);
                        val = ps.pk.GetValue(complexObj);
                        values.Append(val);
                    }
                    else {
                        val = properties[i].GetValue(target);
                        values.Append(val);
                    }
                    if (i != properties.Length - 1){
                        values.Append("','");
                        ++count;
                    }
                    else values.Append("'");
                }
                else
                {
                    if (properties[i].IsDefined(typeof(NotIdentity))) noIdentity = true;
                    continue;
                }
            }
            if(noIdentity) return "INSERT INTO " + TABLE_NAME + "(" + columns + ") OUTPUT INSERTED." + PK_NAME + " VALUES " + "(" + values + ")";
            return "INSERT INTO " + TABLE_NAME + "(" + columns_except_pk + ") OUTPUT INSERTED." + PK_NAME + " VALUES " + "(" + values + ")";
        }


        protected override string SqlDelete(object target)
        {
            object val = new Object();
            foreach (PropertyInfo p in properties){
                if (p.IsDefined(typeof(PKAttribute))){
                    val = p.GetValue(target);
                    break;
                }
            }
            return "DELETE FROM " + TABLE_NAME + " WHERE " + PK_NAME + " = " + val;
        }

        protected override string SqlUpdate(object target)
        {
            string pk_value = "";
            string updateString = "UPDATE " + TABLE_NAME + " SET ";
            string updateToConcatenate = "{0} = '{1}'";
            for (int i = 0; i < properties.Length; ++i){
                if (properties[i].IsDefined(typeof(PKAttribute))){
                    pk_value += string.Format(updateToConcatenate, PK_NAME, properties[i].GetValue(target));
                    continue;
                }
                updateString += string.Format(updateToConcatenate, properties[i].Name, properties[i].GetValue(target));
                if (i != properties.Length - 1) updateString += ", ";
            }
            return updateString += " WHERE " + pk_value;
        }

        
    }
}
