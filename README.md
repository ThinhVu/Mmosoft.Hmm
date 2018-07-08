# Hmm
Simple ORM implementation which help working with db in C# more quicker.

# How to use
There are some example using this libary to do some insert, query action.

1. Create model
Model in Hmm is just POCO with Field attribute declared like so, at the moment, no rule is applied (for example length of string):

```
public class ProductBasicInfo
{
    [Field("Id", SqlDbType.Int)]
    public int Id { get; set; }
    
    [Field("Name", SqlDbType.NVarChar)]
    public string ProductName { get; set; }
}

public class ProductFullInfo : ProductBasicInfo
{
    [Field("Price", SqlDbType.Int)]
    public int Price { get; set; }

    // Mapping with custom name in db
    [Field("MFG", SqlDbType.DateTime2)]
    public DateTime ManufacturingDate { get; set; }
    
    // Mapping with custom name in db
    [Field("EXP", SqlDbType.DateTime2)]
    public DateTime ExpiredDate { get; set; }
}
```
2. Insert/update/delete 1 item product
```
int InsertProduct(ProductFullInfo s)
{
    return sql.NonQuery(
        "INSERT INTO ProductTbl(Name, Price, MFG, EXP) OUTPUT INSERTED.Id VALUES(@Name, @Price, @MFG, @EXP)", 
        Sql.Param("@Name", s.ProductName, SqlDbType.NVarChar),
        Sql.Param("@Price", s.Price, SqlDbType.Int),
        Sql.Param("@MFG", s.ManufacturingDate, SqlDbType.DateTime2),
        Sql.Param("@EXP", s.ExpiredDate, SqlDbType.DateTime2)
    );
}
```

3. Bulk insert:

```
int InsertProducts(List<ProductFullInfo> ps)
{
    // build sql values, params
    var values = new List<string>();
    var @params = new List<SqlParameter>();
    for(var i=0;i<ps.Count;++i)
    {
        values.Add(string.Format("(@Name{0}, @Price{0}, @MFG{0}, @EXP{0})", i));
        @params.Add(Sql.Param("@Name" + i, ps[i].ProductName, SqlDbType.NVarChar));
        @params.Add(Sql.Param("@Price" + i, ps[i].Price, SqlDbType.Int));
        @params.Add(Sql.Param("@MFG" + i, ps[i].ManufacturingDate, SqlDbType.DateTime2));
        @params.Add(Sql.Param("@EXP" + i, ps[i].ExpiredDate, SqlDbType.DateTime2));
    }
    return sql.NonQuery(
        "INSERT INTO ProductTbl(Name, Price, MFG, EXP) VALUES " + string.Join(", ", values.ToArray()), 
        @params.ToArray());
}
```

4. Query in flexible mode allow you set alias for returned column
```
List<ProductBasicInfo> GetProducts()
{
    // note that Iden and Alias as a column name of returned data
    // so we need to access by these key to get column value.
    var ids = new List<ProductBasicInfo>();
    var qoutput = sql.Queries("SELECT Id as [Iden], Name as [Alias] FROM ProductTbl");
    foreach (var item in qoutput)
        ids.Add(new ProductBasicInfo { Id = (int)item["Iden"], ProductName = (string)item["Alias"] });
    return ids;
}
```

5. Query in strict mode make your code cleaner
```
List<ProductBasicInfo> GetProducts()
{
    return sql.Queries<ProductBasicInfo>("SELECT Id, Name FROM ProductTbl");
}
```

6. Using join, view, stored procedure...just like above.