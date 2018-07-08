using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Hmm
{   
    public class Sql
    {
        /// <summary>
        /// Inner class help map model from data reader
        /// </summary>
        static class Mapper
        {
            // 
            public static Dictionary<string, object> Map(SqlDataReader dr)
            {
                var oo = new Dictionary<string, object>();
                List<string> fields = getSQLDataReaderFields(dr);
                foreach(var field in fields)
                    oo[field] = dr[field];
                return oo;
            }
            public static List<Dictionary<string, object>> Maps(SqlDataReader dr)
            {
                var items = new List<Dictionary<string, object>>();
                while (dr.Read())
                    items.Add(Map(dr));
                return items;
            }
            //
            public static T Map<T>(SqlDataReader dr)where T : new()
            {
                return Map<T>(Map(dr));
            }
            public static List<T> Maps<T>(SqlDataReader dr) where T : new()
            {
                return Maps<T>(Maps(dr));
            }
            //
            public static T Map<T>(Dictionary<string, object> data) where T : new()
            {
                PropertyInfo[] modelProps = getModelProps(typeof(T));
                
                var item = new T();                
                for (int i = 0; i < modelProps.Length; i++)
                {
                    var name = (modelProps[i].GetCustomAttributes(typeof(FieldAttribute), true)[0] as FieldAttribute).Name;
                    if (data.ContainsKey(name))
                    {
                        object value = data[name];
                        modelProps[i].SetValue(item, value, null);
                    }
                }

                return item;
            }
            public static List<T> Maps<T>(List<Dictionary<string, object>> data) where T : new()
            {
                var items = new List<T>();
                foreach (var d in data)
                    items.Add(Map<T>(d));
                return items;
            }
            //
            private static PropertyInfo[] getModelProps(Type targetType, bool inheritAttr = true)
            {
                var targetProps = new List<PropertyInfo>();
                var allProps = getPublicInstanceProps(targetType);
                foreach (var prop in allProps)
                    if (prop.GetType().GetCustomAttributes(typeof(FieldAttribute), inheritAttr).Length > 0)
                        targetProps.Add(prop);
                return targetProps.ToArray();
            }
            private static PropertyInfo[] getPublicInstanceProps(Type targetType)
            {
                return targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            }
            private static List<string> getSQLDataReaderFields(SqlDataReader dr)
            {
                // get queried fields
                var fieldNames = new List<string>();
                for (int i = 0; i < dr.FieldCount; i++)
                    fieldNames.Add(dr.GetName(i).ToLower());
                return fieldNames;
            }
        }

        // cache connection string
        private static string _connectionString;

        /// <summary>
        /// Init sql helper
        /// </summary>
        /// <param name="dbSource"></param>
        /// <param name="dbName"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Sql(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static SqlParameter Param(string name, object value, SqlDbType type)
        {
            return new SqlParameter(name, value) { SqlDbType = type };
        }

        public int NonQuery(string cmdText, params SqlParameter[] @params)
        {
            // insert, delete, update
            var cmd = createCmd(cmdText, @params);
            var result = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return result;
        }

        public TResult QueryScalar<TResult>(string cmdText, params SqlParameter[] @params)
        {
            var cmd = createCmd(cmdText, @params);
            var result = (TResult) cmd.ExecuteScalar();
            cmd.Dispose();
            return result;
        }

        public Dictionary<string, object> Query(string cmdText, params SqlParameter[] @params)
        {
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            var item = Mapper.Map(dr);
            cmd.Dispose();
            return item;
        }

        public List<Dictionary<string, object>> Queries(string cmdText, params SqlParameter[] @params)
        {
            var items = new List<Dictionary<string, object>>();
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            while (dr.Read())
                items.Add(Mapper.Map(dr));
            cmd.Dispose();
            return items;
        }

        public TModel Query<TModel>(string cmdText, params SqlParameter[] @params) where TModel: new()
        {
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            var item = Mapper.Map<TModel>(dr);
            cmd.Dispose();
            return item;
        }

        public List<TModel> Queries<TModel>(string cmdText, params SqlParameter[] @params) where TModel : new()
        {
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            var items = Mapper.Maps<TModel>(dr);
            cmd.Dispose();
            return items;
        }

        private SqlCommand createCmd(string cmdText, SqlParameter[] @params)
        {
            var sqlCmd = new SqlCommand(cmdText, new SqlConnection(_connectionString));
            sqlCmd.Parameters.AddRange(@params);
            return sqlCmd;
        }        
    }
}