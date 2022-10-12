using QFileServer.Definitions.DTOs;
using System.Text.Json.Serialization;

namespace QFileServer.Mvc.DTOs
{
    public class ODataQFileServerModelDTO
    {
        [JsonPropertyName("@odata.context")]
        public string ODataContext { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public IEnumerable<QFileServerDTO> Items { get; set; } = Enumerable.Empty<QFileServerDTO>();

        [JsonPropertyName("@odata.count")]
        public long Count { get; set; }
    }
}
