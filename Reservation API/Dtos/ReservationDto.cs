using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using Reservation_API.Models;

namespace Reservation_API.Dtos
{
    public class ReservationDto
    {
        [Required]
        public int Cabin { get; set; }

        [Required]
        public Status Status { get; set; }

        [Required]
        public DateTime Arrive { get; set; }

        [Required]
        public DateTime Departure { get; set; }
    }
}
