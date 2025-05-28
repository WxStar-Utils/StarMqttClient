using System.Text.Json.Serialization;

namespace StarMqttClient.Commands;

public class StarData
{
    [JsonPropertyName("data")] public string Data { get; set; }
    [JsonPropertyName("cmd")] public string Command { get; set; }
}

public class GenericCmd
{
    [JsonPropertyName("cmd")] public string Command { get; set; }
}

public class LoadCue
{
    [JsonPropertyName("star")] public string StarUuid { get; set; }
    [JsonPropertyName("duration")] public int Duration { get; set; } = 3600;
    [JsonPropertyName("flavor")] public string Flavor { get; set; } = "domestic/ldlC";
}

public class LoadCueRoot
{
    [JsonPropertyName("cue_id")] public string CueId { get; set; }
    [JsonPropertyName("cues")] public List<LoadCue> Cues { get; set; } = new();
}

public class RunCue
{
    [JsonPropertyName("cue_id")] public string CueId { get; set; }
    [JsonPropertyName("start_time")] public string StartTime { get; set; }
}

