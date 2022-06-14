using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Role
    {
        public int RoleID { get; set; }
        public string CompCode { get; set; }
        public string RoleName { get; set; }
        //********Associate with Auth Objects***************
        public List<AuthObjectAccess> AuthObjects { get; set; }
    }
    public class AuthObjectAccess
    {
        public int? AuthObjectID { get; set; }
        public int? Auth_Control { get; set; }
        public string Label { get; set; }
        public string Page_route { get; set; }
        public string Icon { get; set; }
    }
}
