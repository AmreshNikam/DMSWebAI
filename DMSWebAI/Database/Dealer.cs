using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class DealerAssigned
    {
        public string RelationBPCode { get; set; }
        public string RelationActGroupID { get; set; }
        public string Name { get; set; }
        public string ShortKey { get; set; }
        public string GSTNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CountryID { get; set; }
        public string CountryDesc { get; set; }
        public string StateID { get; set; }
        public string StateDesc { get; set; }
        public string City { get; set; }
        public string RegionID { get; set; }
        public string RegionDesc { get; set; }
        public string Street { get; set; }
        public string AddressLine { get; set; }
        public string PostalCode { get; set; }
        public string RelationBPDesignation { get; set; }
    }
    public class DealerProductGroup
    {
        public string ProductGroupID { get; set; }
        public string Description { get; set; }
    }
    public class DealerDetails
    {
        public string BPCode { get; set; }
        public string CompCode { get; set; }
        public string PlantCode { get; set; }
        public string ActGroupID { get; set; }
        public string ActGroupDesc { get; set; }
        public string BPFName { get; set; }
        public string BPMName { get; set; }
        public string BPLName { get; set; }
        public string ShortKey { get; set; }
        public string GSTNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CountryID { get; set; }
        public string CountryDesc { get; set; }
        public string StateID { get; set; }
        public string StateDesc { get; set; }
        public string City { get; set; }
        public string RegionID { get; set; }
        public string RegionDesc { get; set; }
        public string Street { get; set; }
        public string AddressLine { get; set; }
        public string PostalCode { get; set; }
        public List<DealerAssigned> Contact { get; set; }
        public List<DealerProductGroup> ProductGroup { get; set; } //not required at the time of creation.
        public int Status { get; set; }

    }
    //public class NameAddress
    //{
    //    public string BPCode { get; set; }
    //    public string Name { get; set; }
    //    public string Address { get; set; }
    //}

}
