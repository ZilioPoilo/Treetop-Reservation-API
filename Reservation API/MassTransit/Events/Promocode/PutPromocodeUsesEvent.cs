using MassTransit;
using System.ComponentModel.DataAnnotations;

namespace Reservation_API.MassTransit.Events.Promocode
{
    [EntityName("reservation-api-put-promocode-uses")]
    [MessageUrn("PutPromocodeUsesEvent")]
    public class PutPromocodeUsesEvent
    {
        [Required]
        public string Code { get; set; }
    }
}
