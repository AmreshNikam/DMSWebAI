using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class ContactUs
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Support { get; set; }
        public string City { get; set; }
        public string CountryID { get; set; }
        public string Country {get;set;}
        public string StateID { get; set; }
        public string State { get; set; }
        public string Subject { get; set; }
        public string LongText { get; set; }
        public DateTime DateOfPosting { get; set; }
    }
}
