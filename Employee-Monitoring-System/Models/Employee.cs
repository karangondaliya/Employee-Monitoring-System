using System.Text.Json.Serialization;

namespace Employee_Monitoring_System.Models
{
    public class Employee
    {
        [JsonPropertyName("employeeId")]
        public int Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("hireDate")]
        public DateTime HireDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("branchId")]
        public int BranchId { get; set; }

        [JsonPropertyName("branchName")]
        public string BranchName { get; set; } = string.Empty;

        // Calculated property for full name
        public string FullName => $"{FirstName} {LastName}";

        // For debugging
        public override string ToString()
        {
            return $"Employee {Id}: {FullName} ({Position})";
        }
    }
}