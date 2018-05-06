using SqlReflect.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlReflect
{
    public abstract class DynamicDataMapper : AbstractDataMapper
    {
        readonly string getAllStmt;
        readonly string getByIdStmt;
        protected readonly string insertStmt;
        protected readonly string deleteStmt;
        protected readonly string updateStmt;

        public DynamicDataMapper(Type klass, string connStr, bool withCache) : base(connStr, withCache)
        {
            TableAttribute table = klass.GetCustomAttribute<TableAttribute>();
            if (table == null) throw new InvalidOperationException(klass.Name + " should be annotated with Table custom attribute !!!!");

            PropertyInfo[] properties = klass.GetProperties();

            PropertyInfo pk = klass.GetProperties().First(p => p.IsDefined(typeof(PKAttribute)));
            PKAttribute pkAtt = (PKAttribute)pk.GetCustomAttribute(typeof(PKAttribute));
            string columns;

            if (!(pkAtt.AutoIncrement)) columns = BuildColumns(true, properties).ToString();
            //String.Join(",", klass.GetProperties().Select(p => p.Name));
            else columns = BuildColumns(false, properties).ToString();
            //String.Join(",", klass.GetProperties().Where(p => p != pk).Select(p => p.Name));

            getAllStmt = "SELECT " + pk.Name + "," + columns + " FROM " + table.Name;
            getByIdStmt = getAllStmt + " WHERE " + pk.Name + "=";
            insertStmt = "INSERT INTO " + table.Name + "(" +  columns + ") OUTPUT INSERTED." + pk.Name + " VALUES ";
            deleteStmt = "DELETE FROM " + table.Name + " WHERE " + pk.Name + "=";
            updateStmt = "UPDATE " + table.Name + " SET {0} WHERE " + pk.Name + "={1}";
        }

        protected override string SqlGetAll()
        {
            return getAllStmt;
        }

        protected override string SqlGetById(object id)
        {
            return getByIdStmt + "'" + id + "'";
        }

        public StringBuilder BuildColumns(bool pkNeeded, PropertyInfo[] properties){
            StringBuilder prebuildedQuery = new StringBuilder("");
            for (int i = 0; i < properties.Length; ++i){
                if (properties[i].IsDefined(typeof(PKAttribute))){
                    if (pkNeeded) prebuildedQuery.Append(properties[i].Name);
                    else continue;
                }
                else{
                    if (IsADBEntity(properties[i].PropertyType)){
                        string pk = GetPKName(properties[i].PropertyType.GetProperties());
                        prebuildedQuery.Append(pk);
                    }
                    else prebuildedQuery.Append(properties[i].Name);
                }
                if (i != properties.Length - 1) prebuildedQuery.Append(",");
            }
            return prebuildedQuery;
        }

        public string GetPKName(PropertyInfo[] properties){
            string aux = "";
            foreach (PropertyInfo p in properties){
                if (p.IsDefined(typeof(PKAttribute))){
                    aux = p.Name;
                    break;
                }
            }
            return aux;
        }

        public static bool IsADBEntity(Type type){
            return Attribute.IsDefined(type, typeof(TableAttribute));
        }
    }
}