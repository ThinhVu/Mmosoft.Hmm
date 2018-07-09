using System;
using System.Collections.Generic;
using System.Data;

namespace Hmm.Test.Models
{
    public class Bill
    {
        [FieldAttr("Id", SqlDbType.Int)]
        public int Id { get; set; }

        [FieldAttr("CreatedDate", SqlDbType.DateTime2)]
        public DateTime CreatedDate { get; set; }
    }

    public class BillVM : Bill
    {
        // Non database field
        public Dictionary<ProductBasicInfo, int> Products { get; set; }
    }
}
