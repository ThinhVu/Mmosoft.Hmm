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
    // using System.Linq
    return sql
        .Queries("SELECT Id as [Iden], Name as [Alias] FROM ProductTbl")
        .ConvertAll<ProductBasicInfo>(item => new ProductBasicInfo { Id = (int)item["Iden"], ProductName = (string)item["Alias"] });
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
Hmm, seem like complex sample which I don't want to show you but:
Now we have db structure like so: 
`Bill <1 --- N> BillDetail <N --- 1> Product`


```
// Added 3 more models
public class BillDetail
{
    [Field("Id", SqlDbType.Int)]
    public int Id { get; set; }

    [Field("BillId", SqlDbType.Int)]
    public int BillId { get; set; }

    [Field("ProductId", SqlDbType.Int)]
    public int ProductId { get; set; }

    [Field("Quantity", SqlDbType.Int)]
    public int Quantity { get; set; }
}

public class Bill
{
    [Field("Id", SqlDbType.Int)]
    public int Id { get; set; }

    [Field("CreatedDate", SqlDbType.DateTime2)]
    public DateTime CreatedDate { get; set; }        
}

public class BillVM : Bill
{
    // Non database field
    public Dictionary<ProductBasicInfo, int> Products { get; set; }
}

// Insert bill infor
int InsertBill(Bill b)
{
    // Insert and return inserted id
    return sql.NonQuery(
        "INSERT INTO Bill(CreatedDate) OUTPUT INSERTED.Id VALUES (@CreatedDate)", Sql.Param("@CreatedDate", b.CreatedDate, SqlDbType.DateTime2));
}

// Insert bill detail info
int InsertBillDetails(List<BillDetail> bds)
{
    var values = new List<string>();
    var @params = new List<SqlParameter>();
    for (var i = 0; i < bds.Count; ++i)
    {
        values.Add(string.Format("(@BillId{0}, @ProductId{0}, @Quantity{0})", i));
        @params.Add(Sql.Param("@BillId" + i, bds[i].BillId, SqlDbType.Int));
        @params.Add(Sql.Param("@ProductId" + i, bds[i].ProductId, SqlDbType.Int));
        @params.Add(Sql.Param("@Quantity" + i, bds[i].Quantity, SqlDbType.Int));
    }
    return sql.NonQuery(
        "INSERT INTO BillDetail(BillId, ProductId, Quantity) VALUES " + string.Join(", ", values.ToArray()),
        @params.ToArray());
}

// 
var billId = InsertBill(new Bill { CreatedDate = DateTime.Now });
InsertBillDetails(new List<BillDetail> 
{
    new BillDetail{ BillId = billId, ProductId = 1, Quantity = 5 },
    new BillDetail{ BillId = billId, ProductId = 2, Quantity = 3 },
    new BillDetail{ BillId = billId, ProductId = 3, Quantity = 4 },
});

// Show bill

var bvm = new BillVM { Id = billId, Products = new Dictionary<ProductBasicInfo, int>() };
sql.Queries(@"
SELECT bo.CreatedDate as [Date], p.Id as [ProdId], p.Name as [ProdName], bo.Quantity as [Qty]
FROM ProductTbl as p 
JOIN (SELECT bi.Id, bi.CreatedDate, bd.ProductId, bd.Quantity
      FROM Bill as bi
      JOIN BillDetail as bd 
      ON bi.Id = bd.BillId) as bo
ON bo.Id = @Id AND p.Id = bo.ProductId", Sql.Param("@Id", billId, SqlDbType.Int))
    .ForEach(x => {
        bvm.CreatedDate = (DateTime)x["Date"]; // assign multiple time is stupid, right?
        bvm.Products[new ProductBasicInfo { Id = (int)x["ProdId"], ProductName = (string)x["ProdName"] }] = (int)x["Qty"];
    });
// show data
Console.WriteLine("Bill: " + billId);
Console.WriteLine("Created Date: " + bvm.CreatedDate.ToString());
Console.WriteLine("Products:");
Console.WriteLine("============================================");            
Console.WriteLine("Id     |Name                      |Quantity ");
Console.WriteLine("--------------------------------------------");
foreach (var item in bvm.Products)
{
    Console.WriteLine(
        item.Key.Id.ToString().PadRight(7, ' ') + '|' + 
        item.Key.ProductName.PadRight(26, ' ') + '|' + 
        item.Value.ToString().PadRight(8, ' '));
}
Console.WriteLine("============================================");
```

Result:

```
Bill: 1
Created Date: 7/9/18 1:33:37 AM
Products:
============================================
Id     |Name                      |Quantity
--------------------------------------------
1      |Coconut                   |5
2      |Banana                    |3
3      |Pineapple                 |4
============================================
```