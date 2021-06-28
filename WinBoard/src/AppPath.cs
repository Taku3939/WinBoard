using Newtonsoft.Json;

namespace WinBoard
{
    [JsonObject("AppPath")]
    public class AppPath
    {
        [JsonProperty("ApplicationPath")]
        public string ApplicationPath { get; set; }

        [JsonProperty("BackgroundPath")]
        public string BackgroundPath { get; set; }

        [JsonProperty("Color")] public Color color { get; set; }
    }
    
}