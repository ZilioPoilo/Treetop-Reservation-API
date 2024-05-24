using System.ComponentModel.DataAnnotations;

namespace Reservation_API.Models
{
    public enum Status
    {
        Completed = 1,

        Reserved = 2,

        AwaitingConfirmation = 3,

        Canceled = 4
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

        public Reservation()
        {
            Id = Guid.NewGuid().ToString();
        }

    }
}
