using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Employee_Monitoring_System.Models
{
    public class Employee
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("branchId")]
        public int BranchId { get; set; }

        [JsonPropertyName("branchName")]
        public string BranchName { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("lastLogin")]
        public DateTime LastLogin { get; set; }

        [JsonPropertyName("jobTitle")]
        public string JobTitle { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("technicalSkills")]
        public Dictionary<string, string> TechnicalSkills { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("certifications")]
        public List<string> Certifications { get; set; } = new List<string>();

        [JsonPropertyName("profileImageBase64")]
        public string ProfileImageBase64 { get; set; } = string.Empty;

        [JsonPropertyName("tasks")]
        public List<UserTask> Tasks { get; set; } = new List<UserTask>();

        [JsonPropertyName("projects")]
        public List<UserProject> Projects { get; set; } = new List<UserProject>();

        // For display purposes - get first letter for profile image
        public string FirstLetter
        {
            get
            {
                if (!string.IsNullOrEmpty(FullName))
                {
                    return FullName[0].ToString().ToUpper();
                }
                return "U";
            }
        }

        // For display - check if user has phone number
        public string Phone
        {
            get
            {
                return !string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber : "Not provided";
            }
        }

        // For display - check if user is active
        public string Status
        {
            get
            {
                return IsActive ? "Active" : "Inactive";
            }
        }

        // For debugging
        public override string ToString()
        {
            return $"User {Id}: {FullName} ({Email})";
        }
    }

    public class UserTask
    {
        [JsonPropertyName("taskId")]
        public int TaskId { get; set; }

        [JsonPropertyName("taskName")]
        public string TaskName { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class UserProject
    {
        [JsonPropertyName("projectId")]
        public int ProjectId { get; set; }

        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}