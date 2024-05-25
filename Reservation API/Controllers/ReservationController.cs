using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Reservation_API.AppMapping;
using Reservation_API.Dtos;
using Reservation_API.Models;
using Reservation_API.Services.DataServices;

namespace Reservation_API.Controllers
{
    [Route("api/reservation-api/v1/reservation")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private static Serilog.ILogger Logger => Serilog.Log.ForContext<ReservationController>();

        public readonly ReservationService _service;
        public readonly IMapper _mapper;

        public ReservationController(IMapper mapper, ReservationService service)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto createReservationDto)
        {
            Reservation model = await _service.CreateAsync(_mapper.Map<Reservation>(createReservationDto));
            ReservationDto dto = _mapper.Map<ReservationDto>(model);
            return StatusCode(200, dto);
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteById([FromBody] string id)
        {
            DeleteResult result = await _service.DeleteByIdAsync(id);
            if (result.DeletedCount == 0)
                return StatusCode(404, new ErrorDto("Reservation not found"));

            return StatusCode(200);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ReservationDto reservationDto)
        {
            Reservation result = await _service.PutAsync(_mapper.Map<Reservation>(reservationDto));
            if (result == null)
                return StatusCode(400, new ErrorDto("Update error"));

            return StatusCode(200);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            List<Reservation> model = await _service.GetAllAsync();

            List<ReservationDto> dto = _mapper.Map<List<ReservationDto>>(model);
            Logger.Debug($"Get {dto}");

            return StatusCode(200, dto);
        }

        [HttpGet("reserved-days")]
        public async Task<IActionResult> GetReservedDays()
        {
            List<Reservation> reservations = _mapper.Map<List<Reservation>>(await _service.GetReservedAsync());
            int totalCabins = 3;

            Dictionary<DateTime, int> occupiedDays = new Dictionary<DateTime, int>();

            foreach (Reservation reservation in reservations)
            {
                for (DateTime date = reservation.Arrive.Date; date < reservation.Departure.Date; date = date.AddDays(1))
                {
                    if (occupiedDays.ContainsKey(date))
                    {
                        occupiedDays[date]++;
                    }
                    else
                    {
                        occupiedDays[date] = 1;
                    }
                }
            }

            List<DateTime> fullyOccupiedDays = occupiedDays
                .Where(d => d.Value == totalCabins)
                .Select(d => d.Key)
                .ToList();

            return StatusCode(200, fullyOccupiedDays);
        }
    }
}
