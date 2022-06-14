using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Order
    {
    }
    public class OrdeHeader
    {
        public string Order_Id { get; set; }
        public string CompCode { get; set; }
        public string CRN_No { get; set; }
        public string BPCode { get; set; }
        public string Shift_TO_Party { get; set; }
        public decimal Total_Value { get; set; }
        public string DeliveryBlockStatus { get; set; }
        public string GSTNumber { get; set; }
        public string OrderCreatedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ChangedBy { get; set; }
        public DateTime? ChangedOn { get; set; }
        public int OrderStatus { get; set; }
        public List<OrderLineItems> Items { get; set; }
        //************************Deived Attribute*************************
        public string SoldTo { get; set; }
        public string SoldToAddress { get; set; }
        public string ShipTo { get; set; }
        public string ShipToAddress { get; set; }

        //added by sohel
        public string SoldToStreet { get; set; }
        public string SoldToAddressLine { get; set; }
        public string SoldToPostalCode { get; set; }
        public string SoldToCity { get; set; }
        public string SoldToStateDesc { get; set; }
        public string SoldToCountryDesc { get; set; }

        public string ShipToStreet { get; set; }
        public string ShipToAddressLine { get; set; }
        public string ShipToPostalCode { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToStateDesc { get; set; }
        public string ShipToCountryDesc { get; set; }


    }
    public class OrderLineItems
    {
        public string Order_Id { get; set; }
        public string CompCode { get; set; }
        public int Line_Item { get; set; }
        public string Product_Id { get; set; }
        public string ProductDesc { get; set; }
        public string Product_Old_Id { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string PricingGroupID { get; set; }
        public decimal Rate { get; set; }
        public string Currency { get; set; }
        public decimal Discount { get; set; }
        public decimal HSNCode { get; set; }
        public decimal GSTPercentage { get; set; }
        public string Status { get; set; }
    }
    public class OrderHistory
    {
        public int Order_Id { get; set; }
        public int Line_Item { get; set; }
        public string ReferenceDocType { get; set; }
        public string ReferenceDocNo { get; set; }
        public decimal Quantity { get; set; }
    }
}
