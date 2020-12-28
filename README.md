# Hmm
## Hmm is not good (or still good) for you if
- You can spend your time to learning well-known .NET ORM framework like EF, NHibernate, NPOCO,... 
- You don't have a job yet. You know what, now a day, EF is one of .NET developer job basic requirements. So learning EF is better.

## Hmm is good for you if
- Performance is important part in your app.
- You want to spend only about 3-5 minutes to learn tiny ORM and get ready to play with it.
- You don't have a lot of time to learn EF, Dapper, NPOCO, NHibernate,...
- You don't want to fully understand the complexity and abstract layer of these fw or you just don't have a time to explore it.
- You still using SqlDataReader, SqlConnection, SqlParameter... to work with db. (It's take time).
- You still using string concat to make T-SQL query, un-awareness of SQLi.

## So what Hmm is

Simple ORM library which help user works with MS SQL Server in C# easier.

# Learn

## 1. S
S is a class which:
1. Execute T-SQL command line.
2. Create SqlParameter in a easy way.
3. Auto convert query result into Dictionary<string, object> object.
4. Auto convert query result into Strongly-typed model.

### S Ctor

```cs
// Create instance of S object
public S(string connectionString) {...}

// Example
S s = new S("connection string");
```
### S Properties
1. TypeMap property
Define map table to help AutoMap feature working correctly.
```cs
// This is default type map dictionary with simple mapping
// You can add or edit exist item to fit with your database.
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
```

### S Methods
1. Param methods
    ```cs
    // Create sql param by supply name, value and sql type
    public static SqlParameter Param(string name, object value, SqlDbType type){...}

    // Example
    S.Param("@Name", "Josh", SqlDbType.NVarchar)
    ```

    ```cs
    // Create sql param by supply name and value
    // corresponding sql db type will be guessed.
    // See more at S.TypeMap property
    public static SqlParameter Param(string name, object value){...}

    // Example
    S.Param("@Name", "Josh")
    ```

2. NonQuery
    ```cs
    // Execute T-SQL non query like insert/delete/update
    public int NonQuery(string cmdText, params SqlParameter[] @params){...}

    // Example
    var sql = new S("your connection string");
    int affected = sql.NonQuery("INSERT INTO Product(Name) VALUES (@Name)", S.Param("@Name", "Nokia 1202"));
    ```

3. Scalar
    ```cs
    // Execute T-SQL and return single value
    public T Scalar<T>(string cmdText, params SqlParameter[] @params){...}

    // Example
    var sql = new S("your connection string");
    int count = sql.Scalar<int>("SELECT COUNT(*) FROM ProductTbl");
    ```

4. Query
    ```cs
    // Execute and return data in Dictionary<string, object> with key is column id, value is column value.
    public Dictionary<string, object> Query(string cmdText, params SqlParameter[] @params){...}

    // Example
    var sql = new S("your connection string");
    var data = sql.Query("SELECT Id, Name FROM ProductTBL");
    var product = new Product { Id = (int)data["Id"], Name = (string)data["Name"] };
    ```


    ```cs
    // Execute and return data in strongly-typed T object
    // Auto mapping will be run in this case.
    public T Query<T>(string cmdText, params SqlParameter[] @params) where T: new() {...}

    // Example
    S sql = new S("your connection string");
    Product product = sql.Query<Product>("SELECT Id, Name FROM ProductTBL");
    ```

5. Queries
    ```cs
    // Execute and return a list of rows of data in List<Dictionary<string, object>>.
    public List<Dictionary<string, object>> Queries(string cmdText, params SqlParameter[] @params) {...}

    // Example    
    S sql = new S("your connection string");
    List<Dictionary<string, object>> pds = sql.Queries("SELECT Id, Name FROM ProductTBL");
    Product p1 = new Product{ Id=(int)pds[0]["Id"], Name=(string)pds[0]["Name"] }; 
    ```

    ```cs
    // Execute and return a list of rows of data in List<T>
    // Auto mapping will be run in this case
    public List<T> Queries<T>(string cmdText, params SqlParameter[] @params) where T : new() {...}

    // Example    
    S sql = new S("your connection string");
    List<Product> pds = sql.Queries<Product>("SELECT Id, Name FROM ProductTBL");
    ```

That's all. Quite simple, huh?

## 2. FieldAttr
To make Auto mapper work correctly, we need some way to define what field of model object map with what field in database.
It's a reason why we need FieldAttr class.

FieldAttr is a class which inherit Attribute class. And you need to decorate your model property with it. 

### Declaration
```cs
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
```

### Usage:

```cs
// Define ProductBasicInfo class with Id and Name field
public class ProductBasicInfo
{
    [FieldAttr("Id", SqlDbType.Int)]
    public int Id { get; set; }

    [FieldAttr("Name", SqlDbType.NVarChar)]
    public string ProductName { get; set; }
}

// Child class also inherit field mapping from base class
public class ProductFullInfo : ProductBasicInfo
{
    [FieldAttr("Price", SqlDbType.Int)]
    public int Price { get; set; }

    // Mapping with MFG column in db
    [FieldAttr("MFG", SqlDbType.DateTime2)]
    public DateTime ManufacturingDate { get; set; }
    
    // Mapping with EXP column in db
    [FieldAttr("EXP", SqlDbType.DateTime2)]
    public DateTime ExpiredDate { get; set; }
}
```

## Examples
See example in Hmm.Test project
