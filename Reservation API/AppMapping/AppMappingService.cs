using AutoMapper;
using Reservation_API.Dtos;
using Reservation_API.Models;

namespace Reservation_API.AppMapping
{
    public class AppMappingService : Profile
    {
        public AppMappingService()
        {
            CreateMap<Reservation, ReservationDto>().ReverseMap();
            CreateMap<ReservationDto, ValidateReservationDto>().ReverseMap();
        }
    }
}
