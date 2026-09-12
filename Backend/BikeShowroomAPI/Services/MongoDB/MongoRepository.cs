using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(MongoDbContext context, string collectionName)
    {
        // This is a workaround since we can't easily get the collection from context generically
        // Each service will need to inject the specific collection
        throw new NotImplementedException("Use specific repository implementations instead");
    }

    public MongoRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(string id, T entity)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var result = await _collection.ReplaceOneAsync(filter, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<long> CountAsync(FilterDefinition<T> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement)
    {
        return await _collection.ReplaceOneAsync(filter, replacement);
    }

    public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
    {
        return await _collection.UpdateOneAsync(filter, update);
    }

    public async Task<T?> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
    {
        return await _collection.FindOneAndUpdateAsync(filter, update,
            new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After });
    }
}