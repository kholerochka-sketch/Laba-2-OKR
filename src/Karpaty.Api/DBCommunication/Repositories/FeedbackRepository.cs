using Karpaty.Api.DBCommunication.Documents;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Karpaty.Api.DBCommunication.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly IMongoCollection<FeedbackDocument> _collection;

        public FeedbackRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<FeedbackDocument>("feedbacks");
        }

        public async Task<List<Feedback>> GetAllAsync()
        {
            var documents = await _collection.Find(_ => true).ToListAsync();
            return documents.Select(ToModel).ToList();
        }

        public async Task<Feedback?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return null;
            }

            var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return document is null ? null : ToModel(document);
        }

        public async Task CreateAsync(Feedback feedback)
        {
            var document = ToDocument(feedback);
            await _collection.InsertOneAsync(document);
            feedback.Id = document.Id;
        }

        public async Task UpdateAsync(Feedback feedback)
        {
            await _collection.ReplaceOneAsync(x => x.Id == feedback.Id, ToDocument(feedback));
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return false;
            }

            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        private static Feedback ToModel(FeedbackDocument d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Email = d.Email,
            Rating = d.Rating,
            Message = d.Message,
            CreatedAt = d.CreatedAt
        };

        private static FeedbackDocument ToDocument(Feedback f) => new()
        {
            Id = f.Id,
            Name = f.Name,
            Email = f.Email,
            Rating = f.Rating,
            Message = f.Message,
            CreatedAt = f.CreatedAt
        };
    }
}
