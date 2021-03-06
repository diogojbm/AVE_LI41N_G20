﻿using SqlReflect.Attributes;
using System;
using System.Reflection;
using System.Text;


namespace SqlReflect
{
    public class PropertySet
    {
        PropertyInfo[] allProperties;
        private Type t;
        private string connectionString;
        public PropertyInfo pk { get; set; }
        private string _columns;
        private string _columnsExceptPK;
        public string Columns {
            get {
                _columns = BuildColumns(true).ToString();
                return _columns;
            }
        }

        public string ColumnsExceptPk {
            get {
                _columnsExceptPK = BuildColumns(false).ToString();
                return _columnsExceptPK;
            }
        }

        public PropertySet(Type t, string connStr)
        {
            this.t = t;
            connectionString = connStr;
            allProperties = t.GetProperties();
            for (int i = 0; i < allProperties.Length; ++i) {
                if (allProperties[i].IsDefined(typeof(PKAttribute))) {
                    pk = allProperties[i];
                }
            }
        }

        public StringBuilder BuildColumns(bool pkNeeded)
        {
            StringBuilder prebuildedQuery = new StringBuilder("");
            for (int i = 0; i < allProperties.Length; ++i){
                if (allProperties[i].IsDefined(typeof(PKAttribute))){
                    if (pkNeeded) prebuildedQuery.Append(allProperties[i].Name);
                    else continue;
                }
                else{
                    if (IsADBEntity(allProperties[i].PropertyType)) {
                        ReflectDataMapper rdm = Mappers.GetMapper(allProperties[i].PropertyType, connectionString);
                        string pk = rdm.GetPKName();
                        prebuildedQuery.Append(pk);
                    }
                    else prebuildedQuery.Append(allProperties[i].Name);
                }
                if (i != allProperties.Length - 1) prebuildedQuery.Append(",");
            }
            return prebuildedQuery;
        }

        public bool IsADBEntity(Type type) {
            return Attribute.IsDefined(type, typeof(TableAttribute));
        }

        public PropertyInfo[] GetProperties() {
            return allProperties;
        }
    }
}