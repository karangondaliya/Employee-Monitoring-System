using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Monitoring_System.Models
{
    public class Project
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Progress { get; set; }
        public double ProgressPercentage => Progress / 100.0;
        public DateTime Deadline { get; set; }
        public string Team { get; set; }
        public string Status { get; set; }
    }
}
