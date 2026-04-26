using Newtonsoft.Json;

public class Indicator
{
    [JsonProperty("n")]
    public string Name { get; set; }

    public int NormalProperty { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }
}
