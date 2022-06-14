using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class OutstandingStatement
    {
        public int OutstandingID { get; set; }
        public string CompCode { get; set; }
        public string Account { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string UploadBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public List<Outstanding> outstandings { get; set; }
        //**************Derived 
        public string Customer { get; set; }
        public string City { get; set; }
        public string RegionID { get; set; }
    }
    public class Outstanding
    {
        public string BillDoc { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime Net_due_dt { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal DueMonthPluseOne { get; set; }
        public decimal DueMonth { get; set; }
        public decimal D0_10 { get; set; }
        public decimal D11_30 { get; set; }
        public decimal D31_60 { get; set; }
        public decimal D61_90 { get; set; }
        public decimal DG91 { get; set; }
        public decimal ON_Account { get; set; }
        public string Comments { get; set; }
        public int OutstandingID { get; set; }
    }
}
