using System.Text.Json.Serialization;

namespace Employee_Monitoring_System.Models
{
    public class Project
    {
        // API returns "projectId" but your model has "Id"
        [JsonPropertyName("projectId")]
        public int Id { get; set; }

        // API returns "projectName" but your model has "Title"
        [JsonPropertyName("projectName")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        // API returns "completionPercentage" but your model has "Progress"
        [JsonPropertyName("completionPercentage")]
        public double Progress { get; set; }

        public double ProgressPercentage => Progress / 100.0;

        // API returns "endDate" but your model has "Deadline"
        [JsonPropertyName("endDate")]
        public DateTime Deadline { get; set; }

        // Your model expects "Team" but API has no direct equivalent
        // We'll use branchName or create a computed property
        private string _team = string.Empty;
        public string Team
        {
            get
            {
                // Try to use branch name if available, or show team members count
                if (!string.IsNullOrEmpty(BranchName))
                    return BranchName;
                else if (TeamMembers != null && TeamMembers.Any())
                    return $"{TeamMembers.Count} team members";
                else
                    return _team;
            }
            set => _team = value;
        }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("startDate")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("branchId")]
        public int BranchId { get; set; }

        [JsonPropertyName("branchName")]
        public string BranchName { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [JsonPropertyName("teamMembers")]
        public List<string> TeamMembers { get; set; } = new List<string>();

        [JsonPropertyName("budget")]
        public decimal? Budget { get; set; }

        // For debugging
        public override string ToString()
        {
            return $"Project {Id}: {Title} ({Status})";
        }
    }
}