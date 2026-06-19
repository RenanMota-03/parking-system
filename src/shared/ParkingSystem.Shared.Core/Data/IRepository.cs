namespace ParkingSystem.Shared.Core.Data;

public interface IRepository
{
    IUnitOfWork UnitOfWork { get; }
}
