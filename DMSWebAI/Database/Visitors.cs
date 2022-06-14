using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Visitors
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public DateTime VisitedOn { get; set; }
        public List<visitorlogs> logs { get; set; }
    }
    public class visitorlogs
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public DateTime LogDateTime { get; set; }
        public string BusinessType { get; set; }
    }

}
