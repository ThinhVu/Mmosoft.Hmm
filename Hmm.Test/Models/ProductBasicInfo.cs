using System.Data;

namespace Hmm.Test.Models
{
    // insome case we don't want to retrive entire model, just a part of its
    // we can using the same Product model in this case
    // or just create very lightweight object to hold data.        
    public class ProductBasicInfo
    {
        [FieldAttr("Id", SqlDbType.Int)]
        public int Id { get; set; }

        [FieldAttr("Name", SqlDbType.NVarChar)]
        public string ProductName { get; set; }
    }
}
