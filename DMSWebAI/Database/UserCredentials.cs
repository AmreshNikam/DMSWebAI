using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class UserCredentials
    {
        public string Userid { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public DateTime Expiry_Date { get; set; }
        public bool Active_Inactive { get; set; }
    }
    public class Loginresponse
    {
        public string BPCode { get; set; }
        public int Auth_Control { get; set; }
        public string Label { get; set; }
        public string Page_route { get; set; }
        public string Icon { get; set; }
    }
}
