using MongoDB.Driver;
using Reservation_API.Models;

namespace Reservation_API.Services.DataServices
{
    public class ReservationService
    {
        private readonly IMongoCollection<Reservation> _collection;

        public ReservationService(IConfiguration configuration, MongoDbConnectionService connectionService)
        {
            var collection_name = configuration.GetSection("MongoDB:TableReservations").Get<string>();
            _collection = connectionService.Database.GetCollection<Reservation>(collection_name);
        }

        public async Task<Reservation> CreateAsync(Reservation model)
        {
            await _collection.InsertOneAsync(model);
            return model;
        }

    }
}
