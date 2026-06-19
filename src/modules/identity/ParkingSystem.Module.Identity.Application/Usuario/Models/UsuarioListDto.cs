namespace ParkingSystem.Module.Identity.Application.Usuario.Models;

public class UsuarioListDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
