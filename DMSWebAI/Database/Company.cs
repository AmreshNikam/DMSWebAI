using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Company
    {

        public string CompCode { get; set; }
        public string CompName { get; set; }
        public string ShortKey { get; set; }
        public string Street { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string StateID { get; set; }
        public string RegionID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string GSTNo { get; set; }
        public string CIN { get; set; }
        public bool Status { get; set; }
        public string CountryID { get; set; }
        //**************derived fiels *************************
        public string RegionName { get; set; }
        public string State { get; set; }
        
        public string Country { get; set; }
    }
}
