using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Emloyees
    {
        public string BPCode { get; set; }
        public string CompCode { get; set; }
        public string PlantCode { get; set; }
        public string ActGroupID { get; set; }
        public string BPFName { get; set; }
        public string BPMName { get; set; }
        public string BPLName { get; set; }
        public string ShortKey { get; set; }
        public string GSTNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CountryID { get; set; }
        public string StateID { get; set; }
        public string City { get; set; }
        public string RegionID { get; set; }
        public string Street { get; set; }
        public string AddressLine { get; set; }
        public string PostalCode { get; set; }
        public int Status { get; set; }

        //**************derived attribute************************

        public string ActGroupDesc { get; set; }
        public string CountryDesc { get; set; }
        public string StateDesc { get; set; }
        public string RegionDesc { get; set; }

        //**********************Data from other table **********************************
        public int DesigID { get; set; } // for t_bp_designation table
        public string SeniorBPCode { get; set; } // for t_bp_designation table
        public string Designation { get; set; } // for t_bp_relation table
        public string SeniorName { get; set; }
        public List<BPCodeWithName> Dealers { get; set; } // for t_bp_relation table
        public List<StateAssignment> StateAssignments { get; set; } // for t_bp_state_assignment table
     }
    public class StateAssignment
    {
        public string CountryID { get; set; }
        public string CountryDesc { get; set; }
        public string StateID { get; set; }
        public string StateDesc { get; set; }
    }
    public class BPCodeWithName
    {
        public string BPCode { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

    }
    

}
