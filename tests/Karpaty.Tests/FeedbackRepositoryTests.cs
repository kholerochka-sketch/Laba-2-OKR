
using Karpaty.Api.DBCommunication.Documents;
using Karpaty.Api.DBCommunication.Repositories;
using Karpaty.Api.Models;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Karpaty.Tests;

public class FeedbackRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly FeedbackRepository _repository;
    private readonly string _databaseName;

    public FeedbackRepositoryTests()
    {
        _runner = MongoDbRunner.Start();

        _client = new MongoClient(_runner.ConnectionString);

        _databaseName = $"FeedbackTests_{Guid.NewGuid():N}";
        _database = _client.GetDatabase(_databaseName);

        _repository = new FeedbackRepository(_database);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllFeedbacks()
    {
        var collection = _database.GetCollection<FeedbackDocument>("feedbacks");

        await collection.InsertManyAsync(new[]
        {
            new FeedbackDocument
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Andriy",
                Email = "andriy@example.com",
                Rating = 5,
                Message = "Great place"
            },
            new FeedbackDocument
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Ivan",
                Email = "ivan@example.com",
                Rating = 4,
                Message = "Good service"
            }
        });

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Andriy");
        Assert.Contains(result, x => x.Name == "Ivan");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsFeedback()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var collection = _database.GetCollection<FeedbackDocument>("feedbacks");

        await collection.InsertOneAsync(new FeedbackDocument
        {
            Id = id,
            Name = "Andriy",
            Email = "andriy@example.com",
            Rating = 5,
            Message = "Excellent"
        });

        var result = await _repository.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Andriy", result.Name);
        Assert.Equal("andriy@example.com", result.Email);
        Assert.Equal(5, result.Rating);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsInvalid_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync("invalid-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFeedbackDoesNotExist_ReturnsNull()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var result = await _repository.GetByIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_InsertsFeedbackAndSetsId()
    {
        var feedback = new Feedback
        {
            Name = "Andriy",
            Email = "andriy@example.com",
            Rating = 5,
            Message = "Very good"
        };

        await _repository.CreateAsync(feedback);

        Assert.NotNull(feedback.Id);
        Assert.True(ObjectId.TryParse(feedback.Id, out _));

        var collection = _database.GetCollection<FeedbackDocument>("feedbacks");

        var saved = await collection
            .Find(x => x.Id == feedback.Id)
            .FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal("Andriy", saved.Name);
        Assert.Equal("andriy@example.com", saved.Email);
        Assert.Equal(5, saved.Rating);
        Assert.Equal("Very good", saved.Message);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesExistingFeedback()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var collection = _database.GetCollection<FeedbackDocument>("feedbacks");

        await collection.InsertOneAsync(new FeedbackDocument
        {
            Id = id,
            Name = "Old name",
            Email = "old@example.com",
            Rating = 2,
            Message = "Old message"
        });

        var feedback = new Feedback
        {
            Id = id,
            Name = "New name",
            Email = "new@example.com",
            Rating = 5,
            Message = "New message"
        };

        await _repository.UpdateAsync(feedback);

        var updated = await collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        Assert.NotNull(updated);
        Assert.Equal("New name", updated.Name);
        Assert.Equal("new@example.com", updated.Email);
        Assert.Equal(5, updated.Rating);
        Assert.Equal("New message", updated.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenFeedbackExists_ReturnsTrue()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var collection = _database.GetCollection<FeedbackDocument>("feedbacks");

        await collection.InsertOneAsync(new FeedbackDocument
        {
            Id = id,
            Name = "Andriy",
            Email = "andriy@example.com",
            Rating = 5,
            Message = "Test"
        });

        var result = await _repository.DeleteAsync(id);

        Assert.True(result);

        var deleted = await collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdIsInvalid_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync("invalid-id");

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenFeedbackDoesNotExist_ReturnsFalse()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var result = await _repository.DeleteAsync(id);

        Assert.False(result);
    }

    public void Dispose()
    {
        _client.DropDatabase(_databaseName);
        _runner.Dispose();
    }
}
