using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class EmployeeDesignations
    {
        public int DesigID { get; set; }
        public string ShortCode { get; set; }
        public string Description { get; set; }
        public int? SeniorDesgID { get; set; }
        public string Senior { get; set; }
        public string CompCode { get; set; }
        public string PlantCode { get; set; }
    }
}
