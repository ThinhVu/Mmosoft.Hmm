using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Hmm.Test
{
    class Program
    {
        static Sql sql = new Sql("Server=(local);Database=HmmTest;Trusted_Connection=True;");

        static void Main(string[] args)
        {
            #region insert one item
            InsertProduct(new ProductFullInfo 
            { 
                ProductName = "Coconut",
                Price = 12,
                ManufacturingDate = new DateTime(2012, 1, 12),
                ExpiredDate = new DateTime(2012, 2, 12)
            });
            #endregion

            #region bulk insert
            InsertProducts(new List<ProductFullInfo> 
            {
                new ProductFullInfo 
                { 
                    ProductName = "Banana",
                    Price = 5,
                    ManufacturingDate = new DateTime(2012, 1, 12),
                    ExpiredDate = new DateTime(2012, 1, 15)
                },
                new ProductFullInfo 
                { 
                    ProductName = "Pineapple",
                    Price = 6,
                    ManufacturingDate = new DateTime(2012, 1, 12),
                    ExpiredDate = new DateTime(2012, 1, 20)
                },
                new ProductFullInfo 
                { 
                    ProductName = "Apple",
                    Price = 7,
                    ManufacturingDate = new DateTime(2012, 1, 12),
                    ExpiredDate = new DateTime(2012, 1, 20)
                },
                new ProductFullInfo 
                { 
                    ProductName = "Watermelon",
                    Price = 8,
                    ManufacturingDate = new DateTime(2012, 1, 12),
                    ExpiredDate = new DateTime(2012, 1, 20)
                }
            });

            #endregion

            #region find item
            ProductFullInfo product = FindProductFull(1);
            #endregion

            #region FindProductBasicSlow
            var productBasicSlow = FindProductBasicSlow(1);
            #endregion

            #region FindProductBasicFast
            var productBasicFast = FindProductBasicFast(1);
            #endregion

            #region GetProductFlexibleWay
            var items1 = GetProductIds_1();
            #endregion

            #region GetProductStrictWay
            var items2 = GetProductIds_2();
            #endregion


            #region GetProductStrictWayGoneWrong
            var items2GoneWrong = GetProductIds_2_GoneWrong();
            #endregion
        }

        // Insert product
        static int InsertProduct(ProductFullInfo s)
        {
            return sql.NonQuery(
                "INSERT INTO ProductTbl(Name, Price, MFG, EXP) OUTPUT INSERTED.Id VALUES(@Name, @Price, @MFG, @EXP)", 
                Sql.Param("@Name", s.ProductName, SqlDbType.NVarChar),
                Sql.Param("@Price", s.Price, SqlDbType.Int),
                Sql.Param("@MFG", s.ManufacturingDate, SqlDbType.DateTime2),
                Sql.Param("@EXP", s.ExpiredDate, SqlDbType.DateTime2)
            );
        }

        // bulk insert
        static int InsertProducts(List<ProductFullInfo> ps)
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

        // -- query single items --

        // query product full info
        static ProductFullInfo FindProductFull(int id)
        {
            return sql.Query<ProductFullInfo>(
                "SELECT * FROM ProductTbl WHERE Id = @Id",
                Sql.Param("@Id", 1, SqlDbType.Int)
            );
        }
        
        // query product basic info with more memory consume
        // query * will return data of product full info
        static ProductBasicInfo FindProductBasicSlow(int id)
        {
            return sql.Query<ProductBasicInfo>(
                "SELECT * FROM ProductTbl WHERE Id = @Id", 
                Sql.Param("@Id", 1, SqlDbType.Int)
            );
        }

        // query product basic info with less memory consume
        static ProductBasicInfo FindProductBasicFast(int id)
        {
            return sql.Query<ProductBasicInfo>(
                "SELECT Id, Name FROM ProductTbl WHERE Id = @Id",
                Sql.Param("@Id", 1, SqlDbType.Int)
            );
        }

        // -- query list ---
        // get product with only basic info in flexible way
        // use it if you don't want to create class to hold returned data
        static List<ProductBasicInfo> GetProductIds_1()
        {
            // note that we use Iden and Alias as a column name of returned data
            // so we need to access by these key to get column value.
            var ids = new List<ProductBasicInfo>();
            var qoutput = sql.Queries("SELECT Id as [Iden], Name as [Alias] FROM ProductTbl");
            foreach (var item in qoutput)
                ids.Add(new ProductBasicInfo { Id = (int)item["Iden"], ProductName = (string)item["Alias"] });
            return ids;
        }

        // get product with only basic info in strict way (strongly type)
        static List<ProductBasicInfo> GetProductIds_2()
        {
            return sql.Queries<ProductBasicInfo>("SELECT Id, Name FROM ProductTbl");
        }

        // get product with only basic info in strict way (strongly type) with mistake
        static List<ProductBasicInfo> GetProductIds_2_GoneWrong()
        {
            // in this case Iden and ProductName has been used as alias.
            // but query with strongly type doesn't auto mapping like that.            
            return sql.Queries<ProductBasicInfo>("SELECT Id as [Iden], Name as [ProductName] FROM ProductTbl");
        } 
    }

    // insome case we don't want to retrive entire model, just a part of its
    // we can using the same Product model in this case
    // or just create very lightweight object to hold data.        
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
}
