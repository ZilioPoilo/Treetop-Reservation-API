using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using Reservation_API.Models;

namespace Reservation_API.Dtos
{
    public class CreateReservationDto
    {
        [Required]
        public DateTime Arrive { get; set; }

        [Required]
        public DateTime Departure { get; set; }

        [Required]
        public string GuestId { get; set; } = null!;

        [Required]
        public int PaymentAmount { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }
    }
}
