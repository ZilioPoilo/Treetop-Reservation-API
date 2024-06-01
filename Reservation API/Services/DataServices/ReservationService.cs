using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Reservation_API.Models;

namespace Reservation_API.Services.DataServices
{
    public class ReservationService
    {
        private readonly IMongoCollection<Reservation> _collection;

        public ReservationService(IConfiguration configuration, MongoDbConnectionService connectionService)
        {
            string? collection_name = configuration.GetSection("MongoDB:TableReservations").Get<string>();
            _collection = connectionService.Database.GetCollection<Reservation>(collection_name);
        }

        public async Task<Reservation> CreateAsync(Reservation model)
        {
            await _collection.InsertOneAsync(model);

            return model;
        }

        public async Task<List<Reservation>> GetAllAsync()
        {
            var filter = Builders<Reservation>.Filter.Empty;
            List<Reservation> reservations = await _collection.Find(filter).ToListAsync();
            return reservations;
        }

        public async Task<Reservation> GetByIdAsync(string id)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
            Reservation result = await _collection.Find(filter).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<Reservation>> GetReservedAsync()
        { 
            DateTime date = DateTime.Now.Date.AddDays(1);
            var dateFilter = Builders<Reservation>.Filter.Gt(r => r.Departure, date);
            var statusFilter = Builders<Reservation>.Filter.In(r => r.Status, new[] {Status.AwaitingConfirmation, Status.Reserved});
            var combinedFilter = Builders<Reservation>.Filter.And(dateFilter, statusFilter);
            List<Reservation> reservations = await _collection.Find(combinedFilter).ToListAsync();
            return reservations;
        }

        public async Task<DeleteResult> DeleteByIdAsync(string id)
        {
            var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
            DeleteResult result = await _collection.DeleteOneAsync(filter);
            return result;
        }

        public async Task<Reservation> PutAsync(Reservation model)
        {
            var filter = Builders<Reservation>.Filter.Eq("Id", model.Id);
            var options = new FindOneAndReplaceOptions<Reservation>()
            {
                ReturnDocument = ReturnDocument.After
            };
            model = await _collection.FindOneAndReplaceAsync(filter, model, options);

            return model;
        }

        public async Task<List<Reservation>> GetForValidate(DateTime departure, DateTime arrive, int id)
        {
            var departureFilter = Builders<Reservation>.Filter.Gt(r => r.Departure, arrive.Date.AddHours(14));
            var arriveFilter = Builders<Reservation>.Filter.Lt(r => r.Arrive, departure);
            var statusFilter = Builders<Reservation>.Filter.In(r => r.Status, new[] { Status.AwaitingConfirmation, Status.Reserved });
            var idFilter = Builders<Reservation>.Filter.Eq(r => r.Cabin, id);
            var combinedFilter = Builders<Reservation>.Filter.And(departureFilter, arriveFilter, statusFilter, idFilter);

            List<Reservation> reservations = await _collection.Find(combinedFilter).ToListAsync();
            return reservations;
        }
    }
}
