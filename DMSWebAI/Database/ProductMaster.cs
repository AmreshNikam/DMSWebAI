using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class ProductMaster
    {
        public string CompCode { get; set; }
        public string ProductID { get; set; }
        public string ProductDesc { get; set; }
        public string ProductOldID { get; set; }
        public string ProductLongDesc { get; set; }
        public string ProductGroupID { get; set; }
        public string UOM { get; set; }
        public string PricingGroupID { get; set; }
        public string MaterialPricingDesc { get; set; }
        public string Description { get; set; }
        public decimal HSNCode { get; set; }
        public string PackingLotSize { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ChangedBy { get; set; }
        public string ChangedOn { get; set; }
        public decimal MRP { get; set; }
        public string MRPCurrency { get; set; }
        public decimal ListPrice { get; set; }
        public string ListPriceCurrency { get; set; }
        public decimal GSTPer { get; set; }
        public decimal Discount { get; set; }
        public bool Status { get; set; }
    }
}
