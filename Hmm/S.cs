using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Hmm
{   
    public class S
    {
        /// <summary>
        /// Inner class help map model from data reader
        /// </summary>
        static class Mapper
        {
            /// <summary>
            /// Extract data from sql data reader and store in dictionary
            /// If there is no data in data reader, null object will be returned.
            /// </summary>
            public static Dictionary<string, object> Map(SqlDataReader dr)
            {
                Dictionary<string, object> o = null;
                // note that only this method Read data form data reader
                // another Map, Maps method not call this function
                if (dr.Read())
                {
                    o = new Dictionary<string, object>();
                    List<string> fields = getSQLFields(dr);
                    foreach (var field in fields)
                        o[field] = dr[field];
                }
                return o;
            }
            /// <summary>
            /// Extract data from sql data reader and store in dictionary list
            /// </summary>            
            public static List<Dictionary<string, object>> Maps(SqlDataReader dr)
            {
                var items = new List<Dictionary<string, object>>();
                Dictionary<string, object> item;
                while ((item = Map(dr)) != null)
                    items.Add(item);
                return items;
            }
            /// <summary>
            /// Get strongly-typed object with data in dictionary
            /// </summary>
            public static T Map<T>(Dictionary<string, object> data) where T : new()
            {
                Tuple<PropertyInfo, string>[] modelProps = getModelProps(typeof(T));
                // auto mapping
                var item = new T();
                for (int i = 0; i < modelProps.Length; i++)
                {
                    var columName = modelProps[i].Item2;
                    if (data.ContainsKey(columName))
                        modelProps[i].Item1.SetValue(item, data[columName], null);
                }
                return item;
            }
            /// <summary>
            /// Get strongly-typed object list with data in dictionary list
            /// </summary>            
            public static List<T> Maps<T>(List<Dictionary<string, object>> data) where T : new()
            {
                // duplicate code a bit but better performance.
                Tuple<PropertyInfo, string>[] modelProps = getModelProps(typeof(T));
                var items = new List<T>();
                // auto mapping
                foreach (var d in data)
                {
                    var item = new T();
                    for (int i = 0; i < modelProps.Length; i++)
                    {
                        var columName = modelProps[i].Item2;
                        if (d.ContainsKey(columName))
                            modelProps[i].Item1.SetValue(item, d[columName], null);
                    }
                    items.Add(item);
                }
                return items;
            }
            /// <summary>
            /// Get strongly-typed object from sql data reader
            /// </summary>
            public static T Map<T>(SqlDataReader dr) where T : new()
            {
                return Map<T>(Map(dr));
            }
            /// <summary>
            /// Get strongly-typed object list from sql data reader
            /// </summary>
            public static List<T> Maps<T>(SqlDataReader dr) where T : new()
            {
                return Maps<T>(Maps(dr));
            }            
            /// <summary>
            /// DbProperty and column name in database
            /// </summary>            
            private static Tuple<PropertyInfo, string>[] getModelProps(Type targetType)
            {
                var targetProps = new List<Tuple<PropertyInfo, string>>();
                var allProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in allProps)
                {
                    var attr = prop.GetCustomAttributes( attributeType: typeof(FieldAttr), inherit: true);
                    if (attr.Length > 0)
                        targetProps.Add(Tuple.Create(prop, (attr[0] as FieldAttr).Name));
                }
                return targetProps.ToArray();
            }            
            /// <summary>
            /// get list of column name contain in returned data
            /// </summary>
            private static List<string> getSQLFields(SqlDataReader dr)
            {
                // get queried fields
                var fieldNames = new List<string>(dr.FieldCount);
                for (int i = 0; i < dr.FieldCount; i++)
                    fieldNames.Add(dr.GetName(i));
                return fieldNames;
            }
        }

        // cache connection string
        private static string _conn;

        /// <summary>
        /// Modify this dictionary: edit/add new to fit your's need.
        /// </summary>
        public static Dictionary<Type, SqlDbType> TypeMap = new Dictionary<Type, SqlDbType> 
        {
            { typeof(System.Boolean), SqlDbType.Bit },
            { typeof(System.Int16), SqlDbType.SmallInt },
            { typeof(System.Int32), SqlDbType.Int },
            { typeof(System.Int64), SqlDbType.BigInt },
            { typeof(System.Single), SqlDbType.Real },
            { typeof(System.Double), SqlDbType.Float },
            { typeof(System.Decimal), SqlDbType.Money },
            { typeof(System.String), SqlDbType.NVarChar },
            { typeof(System.DateTime), SqlDbType.DateTime2 }
        };

        /// <summary>
        /// Init sql helper
        /// </summary>
        public S(string connectionString)
        {
            _conn = connectionString;
        }

        /// <summary>
        /// Create SqlParameter with specified SqlDbType
        /// </summary> 
        public static SqlParameter Param(string name, object value, SqlDbType type)
        {
            return new SqlParameter(name, value) { SqlDbType = type };
        }

        /// <summary>
        /// Create SqlParameter and guessing SqlDbType based-on value type
        /// </summary>
        public static SqlParameter Param(string name, object value)
        {
            var dbType = SqlDbType.Bit;
            var tov = value.GetType();

            if (TypeMap.ContainsKey(tov))
                dbType = TypeMap[tov];
            else
                throw new Exception("Cannot guess type of value");

            return Param(name, value, dbType);
        }

        /// <summary>
        /// Execute sql non-query command
        /// </summary>
        public int NonQuery(string cmdText, params SqlParameter[] @params)
        {
            // insert, delete, update
            var cmd = createCmd(cmdText, @params);
            var result = cmd.ExecuteNonQuery();
            destroyCmd(cmd);
            return result;
        }

        /// <summary>
        /// Execute sql command and return single value
        /// </summary>
        public TResult Scalar<TResult>(string cmdText, params SqlParameter[] @params)
        {
            var cmd = createCmd(cmdText, @params);
            var result = (TResult) cmd.ExecuteScalar();
            destroyCmd(cmd);
            return result;
        }

        /// <summary>
        /// Query and return single record store in Dictionary
        /// </summary>        
        public Dictionary<string, object> Query(string cmdText, params SqlParameter[] @params)
        {
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            var item = Mapper.Map(dr);
            destroyCmd(cmd);
            return item;
        }

        public List<Dictionary<string, object>> Queries(string cmdText, params SqlParameter[] @params)
        {
            var items = new List<Dictionary<string, object>>();
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            items.AddRange(Mapper.Maps(dr).ToArray());
            destroyCmd(cmd);
            return items;
        }

        public TModel Query<TModel>(string cmdText, params SqlParameter[] @params) where TModel: new()
        {
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            var item = Mapper.Map<TModel>(dr);
            destroyCmd(cmd);
            return item;
        }

        public List<TModel> Queries<TModel>(string cmdText, params SqlParameter[] @params) where TModel : new()
        {
            var cmd = createCmd(cmdText, @params);
            var dr = cmd.ExecuteReader();
            var items = Mapper.Maps<TModel>(dr);
            destroyCmd(cmd);
            return items;
        }

        private SqlCommand createCmd(string cmdText, SqlParameter[] @params)       
        {
            // connection will be close when sqlCmd dispose
            var sqlConn = new SqlConnection(_conn);
            sqlConn.Open();
            var sqlCmd = new SqlCommand(cmdText, sqlConn);
            sqlCmd.Parameters.AddRange(@params);        
            return sqlCmd;
        }
        private void destroyCmd(SqlCommand cmd)
        {
            var conn = cmd.Connection;
            if (conn != null && conn.State != ConnectionState.Closed)
            {
                conn.Close();
                conn.Dispose();
            }
            cmd.Dispose();
        }
    }
}