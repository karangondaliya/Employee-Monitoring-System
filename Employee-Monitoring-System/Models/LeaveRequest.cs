using System;

namespace Employee_Monitoring_System.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } // Instead of Reason
        public string Status { get; set; }
        public int? ApprovedById { get; set; } // Instead of ApprovedBy as string
        public string ApproverName { get; set; } // For display purposes
        public DateTime CreatedDate { get; set; }
    }
}