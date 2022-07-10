using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Viewer.DesktopGL.Models
{
    public partial class BiomeColors
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("colorMap")]
        public BiomeColorMapping[] ColorMap { get; set; }
        
        public static BiomeColors FromJson(string json) => JsonConvert.DeserializeObject<BiomeColors>(json);
        public string ToJson(BiomeColors self) => JsonConvert.SerializeObject(self);
    }

    [JsonConverter(typeof(BiomeColorMappingConverter))]
    public partial class BiomeColorMapping
    {
        public byte BiomeId { get; set; }

        public JColor JColor { get; set; }
    }

    public partial class JColor
    {
        [JsonProperty("r")]
        public int R { get; set; }

        [JsonProperty("g")]
        public int G { get; set; }

        [JsonProperty("b")]
        public int B { get; set; }
    }

    public class BiomeColorMappingConverter : JsonConverter<BiomeColorMapping>
    {
        public override void WriteJson(JsonWriter writer, BiomeColorMapping value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.BiomeId);
            serializer.Serialize(writer, value.JColor);
            writer.WriteEndArray();
        }

        public override BiomeColorMapping ReadJson(JsonReader reader, Type objectType, BiomeColorMapping existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
                return null;

            var v = existingValue ?? new BiomeColorMapping();
            v.BiomeId = (byte)(reader.ReadAsInt32() ?? 0);
            reader.Read();
            v.JColor = serializer.Deserialize<JColor>(reader);

            reader.Read(); // end array
            return v;
        }
    }
}