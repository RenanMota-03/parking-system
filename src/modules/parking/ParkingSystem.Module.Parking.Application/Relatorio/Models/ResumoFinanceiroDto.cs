namespace ParkingSystem.Module.Parking.Application.Relatorio.Models;

public class ResumoFinanceiroDto
{
    public decimal ReceitaHoje { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal TicketMedio { get; set; }
    public int TransacoesHoje { get; set; }
    public int ReservasAtivas { get; set; }
}
