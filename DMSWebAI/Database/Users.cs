using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Users
    {
        public string Userid { get; set; }
        public string CompCode { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public string Street { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string StateID { get; set; }
        public string CountryID { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CreatedBy { get; set; }
        public string ChangedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ChangedOn { get; set; }
        //For t_user_credential table
        public string Password { get; set; }
        public int? Role { get; set; }
        public DateTime Expiry_Date { get; set; }
        public int Active_Inactive { get; set; }
        public string BPCode { get; set; }
    }
}
