using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Assets.Scripts.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StructureType
    {
        SETTLEMENT,
        CITY,
        ROAD,
        NONE
    }
}
