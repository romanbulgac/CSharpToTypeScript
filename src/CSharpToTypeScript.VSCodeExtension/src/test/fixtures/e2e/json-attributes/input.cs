using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Contracts
{
    public class AttributeDto
    {
        [JsonPropertyName("snake_case_name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "legacy_name")]
        public string DisplayName { get; set; }

        [JsonProperty("arg_name")]
        public string Code { get; set; }

        [JsonIgnore]
        public string Hidden { get; set; }

        public static int GlobalCounter;

        private string Secret { get; set; }

        public string PrivateGetter { private get; set; }

        public string ProtectedGetter { protected get; set; }

        public string InternalGetter { internal get; set; }

        public string Visible { get; set; }
    }
}
