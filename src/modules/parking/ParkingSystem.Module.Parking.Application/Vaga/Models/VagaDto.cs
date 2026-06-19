using System.Text.Json.Serialization;

namespace ParkingSystem.Module.Parking.Application.Vaga.Models;

public class VagaDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("tipo_vaga")]
    public int TipoVaga { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
