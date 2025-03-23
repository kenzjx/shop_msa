using System.Linq.Expressions;
using Application.Commom.Interfaces;
using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Data.Mongo;

public class RepositoryMongo<T> : IRepositoryMongo<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection;

    public RepositoryMongo(IMongoDatabase database)
    {
        _collection = database.GetCollection<T>(typeof(T).Name);
    }
    
    public async Task<T> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(string id, T entity)
    {
        await _collection.ReplaceOneAsync(x => x.Id == id, entity);
    }

    public async Task DeleteAsync(string id)
    {
        // var result = await _collection.(x => x.Id == id);
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}