using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class DetailsPlant
    {
        public string ComapCode { get; set; }
        public string CompanyName { get; set; }
        public string PlantCode { get; set; }
        public string PlantName { get; set; }
        public string ShortKey { get; set; }
        public int? RegionId { get; set; }
        public string RegionName { get; set; }
        public int? AddrNo { get; set; }
        public string CIN { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string GSTNo { get; set; }
        public string AddressLine_1 { get; set; }
        public string AddressLine_2 { get; set; }
        public string PostalCode { get; set; }
        public int? CityId { get; set; }
        public string City { get; set; }
        public int? StateId { get; set; }
        public string State { get; set; }
        public int? Country_id { get; set; }
        public string Country { get; set; }
        public bool Status { get; set; }
    }
}
