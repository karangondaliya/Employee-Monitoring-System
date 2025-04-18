using System;

namespace Employee_Monitoring_System.Models
{
    public class Holiday
    {
        public int HolidayId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsRecurring { get; set; }
    }

    public class LeaveBalances
    {
        public int UserId { get; set; }
        public int AnnualLeave { get; set; } = 12;
        public int SickLeave { get; set; } = 3;
        public int PersonalLeave { get; set; } = 1;
        public int UnpaidLeave { get; set; } = 0;
    }
}