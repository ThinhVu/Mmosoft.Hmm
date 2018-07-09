using System;
using System.Data;

namespace Hmm
{
    /// <summary>
    /// Mark a property as a db field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FieldAttr : Attribute
    {
        // column name
        public string Name { get; private set; }
        // column type
        public SqlDbType DbType { get; private set; }
        // for the future
        public object DefaultValue { get; set; }

        public FieldAttr(string columnName, SqlDbType dbType)
            : base()
        {
            Name = columnName;
            DbType = dbType;
        }
    }
}
