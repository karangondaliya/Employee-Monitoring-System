using System.Text.Json.Serialization;

namespace Employee_Monitoring_System.Models
{
    public class Branch
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        public override string ToString()
        {
            return Name;
        }
    }
}