using System;
using System.Data;

namespace Hmm.Test.Models
{
    public class ProductFullInfo : ProductBasicInfo
    {
        [FieldAttr("Price", SqlDbType.Int)]
        public int Price { get; set; }

        // Mapping with custom name in db
        [FieldAttr("MFG", SqlDbType.DateTime2)]
        public DateTime ManufacturingDate { get; set; }

        // Mapping with custom name in db
        [FieldAttr("EXP", SqlDbType.DateTime2)]
        public DateTime ExpiredDate { get; set; }
    }   
}
