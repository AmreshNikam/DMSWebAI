using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Ledgerstatement
    {
        public int ledgerID { get; set; }
        public string CompCode { get; set; }
        public string Account { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public List<ledger> ledgers { get; set; }
        //************* Derived
        public string Customer { get; set; }
        public string City { get; set; }
        public string RegionID { get; set; }

    }

    public class ledger
    {
        public string Assignment { get; set; }
        public string BillDoc { get; set; }
        public string Reference { get; set; }
        public string PostingKey { get; set; }
        public string DocumentNo { get; set; }
        public string DocType { get; set; }
        public string DocTypeDesc { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime PostingDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Currency { get; set; }
        public string ClearingDoc { get; set; }
        public DateTime ClearingDate { get; set; }
        public string Text { get; set; }
        public string Period { get; set; }
        public int ledgerID { get; set; }
    }
}
