using Domain.Entities;

namespace Application.Commom.Interfaces;

public interface IUnitOfWork
{
    IRepositoryMongo<Product> Products { get; }
    Task CommitAsync();
}