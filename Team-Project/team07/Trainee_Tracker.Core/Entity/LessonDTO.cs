using System.Text.Json.Serialization;

namespace Trainee_Tracker.Core.DTO;
//Only needed for CurriculumImports, should not be used elsewhere
public class LessonDTO
{
    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("title")]
    public string title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string url { get; set; } = string.Empty;

    [JsonPropertyName("estimate")]
    public double ?estimate { get; set; }

    [JsonPropertyName("deprecated")]
    public bool deprecated { get; set; }
}