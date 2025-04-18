﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Monitoring_System.Models
{
    public class ApiTaskModel
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<UserMiniDTO> AssignedUsers { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public int TimeSpent { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
