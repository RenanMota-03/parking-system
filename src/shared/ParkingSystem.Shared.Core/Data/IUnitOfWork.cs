namespace ParkingSystem.Shared.Core.Data;

public interface IUnitOfWork
{
    Task<bool> Commit();
}
