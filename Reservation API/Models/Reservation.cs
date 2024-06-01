using System.ComponentModel.DataAnnotations;

namespace Reservation_API.Models
{
    public enum Status
    {
        Validated = 0,

        Completed = 1,

        Reserved = 2,

        AwaitingConfirmation = 3,

        Canceled = 4
    }

    public enum PaymentType
    {
        Card = 1,

        Cash = 2
    }

    public class Reservation
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public int Cabin { get; set; }

        [Required]
        public Status Status { get; set; }

        [Required]
        public DateTime Arrive { get; set; }

        [Required]
        public DateTime Departure { get; set; }

        [Required]
        public string GuestNumber { get; set; } = null!;

        [Required]
        public int PaymentAmount { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }

        public string? Promocode { get; set; }

        public void UpdateReservation()
        {
            Id = Guid.NewGuid().ToString();
            Status = Status.AwaitingConfirmation;
            Arrive = Arrive.Date.AddHours(14);
            Departure = Departure.Date.AddHours(11);
        }

    }
}
