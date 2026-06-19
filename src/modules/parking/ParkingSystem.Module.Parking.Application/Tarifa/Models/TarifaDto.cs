using System.Text.Json.Serialization;

namespace ParkingSystem.Module.Parking.Application.Tarifa.Models;

public class TarifaDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("tipo_vaga")]
    public int TipoVaga { get; set; }

    [JsonPropertyName("valor_hora")]
    public decimal ValorHora { get; set; }

    [JsonPropertyName("valor_diaria")]
    public decimal ValorDiaria { get; set; }

    [JsonPropertyName("valor_mensal")]
    public decimal ValorMensal { get; set; }

    [JsonPropertyName("vigente_ate")]
    public DateTime? VigenteAte { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
