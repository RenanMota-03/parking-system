namespace ParkingSystem.Module.Identity.Application.Usuario.Models;

public class UsuarioLoginDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public int Role { get; set; }
}
