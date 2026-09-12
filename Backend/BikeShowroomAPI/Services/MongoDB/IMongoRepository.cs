using MongoDB.Driver;
using MongoDB.Bson;

namespace BikeShowroomAPI.Services.MongoDB;

public interface IMongoRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter);
    Task<T> CreateAsync(T entity);
    Task<bool> UpdateAsync(string id, T entity);
    Task<bool> DeleteAsync(string id);
    Task<long> CountAsync(FilterDefinition<T> filter);
    Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement);
    Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
    Task<T?> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
}