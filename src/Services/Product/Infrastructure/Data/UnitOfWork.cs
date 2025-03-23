
using Application.Commom.Interfaces;
using Domain.Entities;
using Infrastructure.Data.Mongo;
using MongoDB.Driver;

namespace Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly IMongoDatabase _database;
    public IRepositoryMongo<Product> Products { get; }
    public UnitOfWork(IMongoClient mongoClient, string databaseName)
    {
        _database = mongoClient.GetDatabase(databaseName);
        Products = new RepositoryMongo<Product>(_database);
    }
    
    public async Task CommitAsync()
    {
        // MongoDB không hỗ trợ transaction mặc định cho các thao tác đơn giản.
        // Nếu cần transaction, bạn phải sử dụng session:
        // var session = await _database.Client.StartSessionAsync();
        // session.StartTransaction();
        // try
        // {
        //     // Commit logic here
        //     await session.CommitTransactionAsync();
        // }
        // catch
        // {
        //     await session.AbortTransactionAsync();
        //     throw;
        // }
    }
}