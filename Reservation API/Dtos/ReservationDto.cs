﻿using Reservation_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Reservation_API.Dtos
{
    public class ReservationDto
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
        public string GuestId { get; set; } = null!;

        [Required]
        public int PaymentAmount { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }
    }
}
