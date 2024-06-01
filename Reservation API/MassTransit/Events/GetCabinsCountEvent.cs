using MassTransit;

namespace Cabin_API.MassTransit.Events
{
    [EntityName("reservation-api-get-cabins-count")]
    [MessageUrn("GetCabinsCountEvent")]
    public class GetCabinsCountEvent
    {
    }
}
