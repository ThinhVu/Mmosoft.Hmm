using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Hmm.Test.Models;

namespace Hmm.Test
{
    public class Examples
    {
        static S sql = new S("Server=(local);Database=HmmTest;Trusted_Connection=True;");

        static void Main(string[] args)
        {
            // ClearProducts();
            // InsertProduct();
            // InsertProducts();
            // GetProductsFlexibleWay();
            // GetProductsStronglyTypedWay();
            // GetProducts_GoneWrong();
            ProductAndBill();
        }

        static void ClearProducts()
        {
            sql.NonQuery("TRUNCATE TABLE ProductTbl");
        }

        static void InsertProduct()
        {
            ClearProducts();
            var s = new ProductFullInfo { 
                ProductName = "Coconut", 
                Price = 12, 
                ManufacturingDate = new DateTime(2012, 1, 12), 
                ExpiredDate = new DateTime(2012, 2, 12) };

            int id = sql.NonQuery(
                "INSERT INTO ProductTbl(Name, Price, MFG, EXP) OUTPUT INSERTED.Id VALUES(@Name, @Price, @MFG, @EXP)", 
                S.Param("@Name", s.ProductName),
                S.Param("@Price", s.Price),
                S.Param("@MFG", s.ManufacturingDate),
                S.Param("@EXP", s.ExpiredDate)
            );
        }

        static void InsertProducts()
        {
            ClearProducts();
            var ps = new List<ProductFullInfo> 
            {
                new ProductFullInfo { ProductName = "Banana", Price = 5, ManufacturingDate = new DateTime(2012, 1, 12), ExpiredDate = new DateTime(2012, 1, 15) },
                new ProductFullInfo { ProductName = "Pineapple", Price = 6, ManufacturingDate = new DateTime(2012, 1, 12), ExpiredDate = new DateTime(2012, 1, 20) },
                new ProductFullInfo { ProductName = "Apple", Price = 7, ManufacturingDate = new DateTime(2012, 1, 12), ExpiredDate = new DateTime(2012, 1, 20) },
                new ProductFullInfo { ProductName = "Watermelon", Price = 8, ManufacturingDate = new DateTime(2012, 1, 12), ExpiredDate = new DateTime(2012, 1, 20) }
            };

            // build sql values, params
            var values = new List<string>();
            var @params = new List<SqlParameter>();

            for(var i=0;i<ps.Count;++i)
            {
                values.Add(string.Format("(@Name{0}, @Price{0}, @MFG{0}, @EXP{0})", i));
                @params.Add(S.Param("@Name" + i, ps[i].ProductName));
                @params.Add(S.Param("@Price" + i, ps[i].Price));
                @params.Add(S.Param("@MFG" + i, ps[i].ManufacturingDate));
                @params.Add(S.Param("@EXP" + i, ps[i].ExpiredDate));
            }

            // exec
            sql.NonQuery("INSERT INTO ProductTbl(Name, Price, MFG, EXP) VALUES " + string.Join(", ", values.ToArray()), @params.ToArray());
        }                      

        static void GetProductsFlexibleWay()
        {
            ClearProducts();
            InsertProducts();
            // get product with only basic info in flexible way incase you don't want to create class to hold returned data
            // note that we use Iden and Alias as a column name of returned data, so we need to access by these key to get column value.
            List<ProductBasicInfo> pds = sql
                .Queries("SELECT Id as [Iden], Name as [Alias] FROM ProductTbl")
                .ConvertAll<ProductBasicInfo>(item => new ProductBasicInfo { Id = (int)item["Iden"], ProductName = (string)item["Alias"] });

            for (int i = 0; i < pds.Count; i++)
                Console.WriteLine(string.Format("Product: Id: {0}\tName: {1}", pds[i].Id, pds[i].ProductName));
            Console.ReadLine();
        }

        static void GetProductsStronglyTypedWay()
        {
            // get product with only basic info in strongly typed
            // Note that Name prop of Field attribute in model should match with returned column
            List<ProductBasicInfo> pds = sql.Queries<ProductBasicInfo>("SELECT Id, Name FROM ProductTbl");
            for (int i = 0; i < pds.Count; i++)
                Console.WriteLine(string.Format("Product: Id: {0}\tName: {1}", pds[i].Id, pds[i].ProductName));
            Console.ReadLine();
        }
   
        static void GetProducts_GoneWrong()
        {
            // Note that ProductBasicInfo doesn't have any dbfield Iden or ProductName so auto mapping will not work in this case
            // returned result will be Id: 0 and Name: null.
            List<ProductBasicInfo> pds = sql.Queries<ProductBasicInfo>("SELECT Id as [Iden], Name as [ProductName] FROM ProductTbl");
            for (int i = 0; i < pds.Count; i++)
                Console.WriteLine(string.Format("Product: Id: {0}\tName: {1}", pds[i].Id, pds[i].ProductName));
            Console.ReadLine();
        }
        
        static void ProductAndBill()
        {
            sql.NonQuery("TRUNCATE TABLE Bill");
            sql.NonQuery("TRUNCATE TABLE BillDetail");

            ClearProducts();
            InsertProducts();

            // Insert bill            
            var b = new Bill { CreatedDate = DateTime.Now };
            // Insert and return inserted id
            int billId = sql.NonQuery(
                "INSERT INTO Bill(CreatedDate) OUTPUT INSERTED.Id VALUES (@CreatedDate)", S.Param("@CreatedDate", b.CreatedDate, SqlDbType.DateTime2));

            // Insert build detail
            var bds = new List<BillDetail> 
            {
                new BillDetail{ BillId = billId, ProductId = 1, Quantity = 5 },
                new BillDetail{ BillId = billId, ProductId = 2, Quantity = 3 },
                new BillDetail{ BillId = billId, ProductId = 3, Quantity = 4 },
            };

            // Insert bill detail information
            var values = new List<string>();
            var @params = new List<SqlParameter>();

            for (var i = 0; i < bds.Count; ++i)
            {
                values.Add(string.Format("(@BillId{0}, @ProductId{0}, @Quantity{0})", i));
                @params.Add(S.Param("@BillId" + i, bds[i].BillId));
                @params.Add(S.Param("@ProductId" + i, bds[i].ProductId));
                @params.Add(S.Param("@Quantity" + i, bds[i].Quantity));
            }

            sql.NonQuery(
                "INSERT INTO BillDetail(BillId, ProductId, Quantity) VALUES " + string.Join(", ", values.ToArray()),
                @params.ToArray());

            // using "complex" t-sql
            // actually I don't recommend this way, let put everything in store procedure or view to make your code easier to debug, maintain.
            var bvm = new BillVM { Id = billId, Products = new Dictionary<ProductBasicInfo, int>() };
            sql.Queries(@"
SELECT bo.CreatedDate as [Date], p.Id as [ProdId], p.Name as [ProdName], bo.Quantity as [Qty]
FROM ProductTbl as p 
JOIN (SELECT bi.Id, bi.CreatedDate, bd.ProductId, bd.Quantity
      FROM Bill as bi
      JOIN BillDetail as bd 
      ON bi.Id = bd.BillId) as bo
ON bo.Id = @Id AND p.Id = bo.ProductId", S.Param("@Id", billId))
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

            Console.ReadLine();
        }
    }
}
