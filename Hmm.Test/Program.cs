using System;
using System.Collections.Generic;
using System.Data;

namespace Hmm.Test
{
    class Program
    {
        static Sql sql = new Sql("");

        static void Main(string[] args)
        {
                     
        }

        // Insert product
        static int InsertProduct(ProductFullInfo s)
        {
            return sql.NonQuery(
                "INSERT INTO ProductTbl(Name, Price, MFG, EXP) OUTPUT INSERTED.Id VALUES(@Name, @Price)", 
                Sql.Param("@Name", s.Name, SqlDbType.NVarChar),
                Sql.Param("@Price", s.Price, SqlDbType.Int),
                Sql.Param("@MFG", s.ManufacturingDate, SqlDbType.DateTime2),
                Sql.Param("@EXP", s.ExpiredDate, SqlDbType.DateTime2)
            );
        }

        // -- query single items --

        // query product full info
        static ProductFullInfo Find(int id)
        {
            return sql.Query<ProductFullInfo>(
                "SELECT * FROM ProductTbl WHERE Id = @Id",
                Sql.Param("@Id", 1, SqlDbType.Int)
            );
        }
        
        // query product basic info with more memory consume
        // query * will return data of product full info
        static ProductBasicInfo FindBasic(int id)
        {            
            return sql.Query<ProductBasicInfo>(
                "SELECT * FROM ProductTbl WHERE Id = @Id", 
                Sql.Param("@Id", 1, SqlDbType.Int)
            );
        }

        // query product basic info with less memory consume
        static ProductBasicInfo FindBasic(int id)
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
                ids.Add(new ProductBasicInfo { Id = (int)item["Iden"], Name = (string)item["Alias"] });
            return ids;
        }

        // get product with only basic info in strict way (strongly type)
        static List<ProductBasicInfo> GetProductIds_2()
        {
            return sql.Queries<ProductBasicInfo>("SELECT Id, Name FROM ProductTbl");
        }

        // get product with only basic info in strict way (strongly type) with mistake
        static List<ProductBasicInfo> GetProductIds_2()
        {
            // in this case Iden and ProductName has been used as alias.
            // but query with strongly type doesn't auto mapping like that.
            // so Id and Name will be default value of new ProductBasicInfo item.
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
        public string Name { get; set; }
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
