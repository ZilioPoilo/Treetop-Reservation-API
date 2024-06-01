using Amazon.Runtime;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Reservation_API.AppMapping;
using Reservation_API.Dtos;
using Reservation_API.MassTransit.Events;
using Reservation_API.MassTransit.Responses;
using Reservation_API.Models;
using Reservation_API.Services.DataServices;

namespace Reservation_API.Controllers
{
    [Route("api/reservation-api/v1/reservation")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private static Serilog.ILogger Logger => Serilog.Log.ForContext<ReservationController>();

        private readonly ReservationService _service;
        private readonly IMapper _mapper;
        private readonly IRequestClient<GetPriceEvent> _clientGetPrice;
            
        public ReservationController(IMapper mapper, ReservationService service, IRequestClient<GetPriceEvent> clientGetPrice)
        {
            _service = service;
            _mapper = mapper;
            _clientGetPrice = clientGetPrice;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservationDto dto)
        {
            Reservation model = _mapper.Map<Reservation>(dto);
            model.UpdateReservation();
            model = await _service.CreateAsync(model);
            if (model == null)
                return StatusCode(500, "Server error");
            ReservationDto result = _mapper.Map<ReservationDto>(model);
            return StatusCode(200, result);
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

            return StatusCode(200, result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            List<Reservation> model = await _service.GetAllAsync();

            List<ReservationDto> result = _mapper.Map<List<ReservationDto>>(model);
            Logger.Debug($"Get {result}");

            return StatusCode(200, result);
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

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateReservationDto dto)
        {
            //Validating
            //Get cabins from Cabin API
            //Создать цикл по кевинам 
            //Выбрать все бронирование где reservation.cabin == cabin.id
            //отсортировать чтобы reservation.arrive <= dto.Departure && reservation.arrive >= DateTime.Now.Date

            //Get price from Cabin API GetPrice(departure)

            GetPriceEvent getPriceEvent = new GetPriceEvent()
            {
                Departure = dto.Departure,
            };

            var response = await _clientGetPrice.GetResponse<GetPriceResponse>(getPriceEvent);
            ReservationDto result = _mapper.Map<ReservationDto>(dto);
            result.Status = Status.Validating;
            result.Cabin = 1;
            result.PaymentAmount = response.Message.Price;
            return StatusCode(200, result);
        }

    }
}
