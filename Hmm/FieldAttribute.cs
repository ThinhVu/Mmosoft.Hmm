using System;
using System.Data;

namespace Hmm
{
    /// <summary>
    /// Mark a property as a db field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FieldAttribute : Attribute
    {
        // db name
        public string Name { get; private set; }
        // db type
        public SqlDbType DbType { get; private set; }

        public object DefaultValue { get; set; }

        public FieldAttribute(string columnName, SqlDbType dbType)
            : base()
        {
            Name = columnName;
            DbType = dbType;
        }
    }
}
