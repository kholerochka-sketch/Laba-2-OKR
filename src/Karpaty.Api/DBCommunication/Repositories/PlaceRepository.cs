using Karpaty.Api.DBCommunication.Documents;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.Models;
using MongoDB.Driver;
using System.Linq;

namespace Karpaty.Api.DBCommunication.Reposetories
{
    public class PlaceRepository : IPlaceRepository
    {
        private readonly IMongoCollection<PlaceDocument> _collection;

        public PlaceRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PlaceDocument>("places");
        }

        public async Task CreateAsync(Place place)
        {
            var document = ToDocument(place);

            await _collection.InsertOneAsync(document);

            place.id = document.id;
        }

        public async Task<List<Place>> GetAllAsync()
        {
            var documents = await _collection.Find(_ => true).ToListAsync();

            return documents.Select(ToModel).ToList();
        }

        public async Task<Place?> GetByIdAsync(string id)
        {
            var document = await _collection.Find(x => x.id == id).FirstOrDefaultAsync();

            return document != null ? ToModel(document) : null;
        }

        public async Task UpdateAsync(Place place)
        {
            var document = ToDocument(place);
            await _collection.ReplaceOneAsync(x => x.id == place.id, document);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.id == id);
        }


        private Place ToModel(PlaceDocument document)
        {
            return new Place
            {
                id = document.id,
                description = document.description,
                imageUrl = document.imageUrl
            };
        }

        private PlaceDocument ToDocument(Place place)
        {
            return new PlaceDocument
            {
                id = place.id,
                description = place.description,
                imageUrl = place.imageUrl
            };
        }
    }
}