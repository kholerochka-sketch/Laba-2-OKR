using Karpaty.Api.DBCommunication.Documents;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Karpaty.Api.DBCommunication.Reposetories
{
    public class TipRepository : ITipRepository
    {
        private readonly IMongoCollection<TipDocument> _collection;

        public TipRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<TipDocument>("tips");
        }

        public async Task<List<Tip>> GetAllAsync()
        {
            var documents = await _collection.Find(_ => true).ToListAsync();
            return documents.Select(ToModel).ToList();
        }

        public async Task<Tip?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return null;
            }

            var document = await _collection.Find(x => x.id == id).FirstOrDefaultAsync();
            return document is null ? null : ToModel(document);
        }

        public async Task CreateAsync(Tip tip)
        {
            var document = ToDocument(tip);
            await _collection.InsertOneAsync(document);
            tip.id = document.id;
        }

        public async Task UpdateAsync(Tip tip)
        {
            await _collection.ReplaceOneAsync(x => x.id == tip.id, ToDocument(tip));
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return false;
            }

            var result = await _collection.DeleteOneAsync(x => x.id == id);
            return result.DeletedCount > 0;
        }

        private static Tip ToModel(TipDocument d) => new()
        {
            id = d.id,
            title = d.title,
            category = d.category,
            text = d.text
        };

        private static TipDocument ToDocument(Tip t) => new()
        {
            id = t.id,
            title = t.title,
            category = t.category,
            text = t.text
        };
    }
}