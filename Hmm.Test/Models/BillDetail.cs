using System.Data;

namespace Hmm.Test.Models
{
    public class BillDetail
    {
        [FieldAttr("Id", SqlDbType.Int)]
        public int Id { get; set; }

        [FieldAttr("BillId", SqlDbType.Int)]
        public int BillId { get; set; }

        [FieldAttr("ProductId", SqlDbType.Int)]
        public int ProductId { get; set; }

        [FieldAttr("Quantity", SqlDbType.Int)]
        public int Quantity { get; set; }
    }
}
