using System.Text.Json.Serialization;

namespace ParkingSystem.Module.Parking.Application.Reserva.Models;

public class ReservaDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("vaga_id")]
    public long VagaId { get; set; }

    [JsonPropertyName("numero_vaga")]
    public string NumeroVaga { get; set; } = string.Empty;

    [JsonPropertyName("usuario_id")]
    public string UsuarioId { get; set; } = string.Empty;

    [JsonPropertyName("data_agendada")]
    public DateTime DataAgendada { get; set; }

    [JsonPropertyName("data_limite")]
    public DateTime DataLimite { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
