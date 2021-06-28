using Newtonsoft.Json;

namespace WinBoard
{
    [JsonObject("Color")]
    public class Color
    {
        [JsonProperty("r")] public int r { get; set; }
        [JsonProperty("g")] public int g { get; set; }
        [JsonProperty("b")] public int b { get; set; }

        public Color(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}