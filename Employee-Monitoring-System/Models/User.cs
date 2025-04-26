using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Employee_Monitoring_System.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("jobTitle")]
        public string JobTitle { get; set; }

        [JsonPropertyName("branchId")]
        public int? BranchId { get; set; }

        [JsonPropertyName("branchName")]
        public string BranchName { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        // Note: The profile image is fetched separately, so we don't include it here
        // The API returns this as a byte[] through a separate endpoint

        // Helper properties that can be used in the UI
        [JsonIgnore]
        public string FirstName => FullName?.Split(' ').FirstOrDefault() ?? string.Empty;

        [JsonIgnore]
        public string LastName => FullName?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty;
    }
}
