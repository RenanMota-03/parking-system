using System.Text.Json.Serialization;

namespace ParkingSystem.Module.Parking.Application.Movimentacao.Models;

public class MovimentacaoDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("vaga_id")]
    public long VagaId { get; set; }

    [JsonPropertyName("numero_vaga")]
    public string NumeroVaga { get; set; } = string.Empty;

    [JsonPropertyName("placa_veiculo")]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [JsonPropertyName("data_entrada")]
    public DateTime DataEntrada { get; set; }

    [JsonPropertyName("data_saida")]
    public DateTime? DataSaida { get; set; }

    [JsonPropertyName("valor_total")]
    public decimal? ValorTotal { get; set; }

    [JsonPropertyName("pago")]
    public bool Pago { get; set; }

    [JsonPropertyName("forma_pagamento")]
    public int? FormaPagamento { get; set; }
}
