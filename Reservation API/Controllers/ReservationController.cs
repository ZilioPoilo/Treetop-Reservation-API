﻿using Amazon.Runtime;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Reservation_API.AppMapping;
using Reservation_API.Dtos;
using Reservation_API.MassTransit.Events.Cabin;
using Reservation_API.MassTransit.Events.Price;
using Reservation_API.MassTransit.Events.Promocode;
using Reservation_API.MassTransit.Responses.Cabin;
using Reservation_API.MassTransit.Responses.Price;
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
        private readonly IRequestClient<GetCabinsCountEvent> _clientGetCabinsCount;
        private readonly IPublishEndpoint _clientPutPromocode;
            
        public ReservationController(
            IMapper mapper, 
            ReservationService service, 
            IRequestClient<GetPriceEvent> clientGetPrice,
            IRequestClient<GetCabinsCountEvent> clientGetCabinsCount,
            IPublishEndpoint clientPutPromocode
            )
        {
            _service = service;
            _mapper = mapper;
            _clientGetPrice = clientGetPrice;
            _clientGetCabinsCount = clientGetCabinsCount;
            _clientPutPromocode = clientPutPromocode;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservationDto dto)
        {
            Reservation model = _mapper.Map<Reservation>(dto);
            model.UpdateReservation();
            if (model.Promocode != null)
            {
                await _clientPutPromocode.Publish(new PutPromocodeUsesEvent() { Code = model.Promocode });
            }
            await _service.CreateAsync(model);

            ReservationDto result = _mapper.Map<ReservationDto>(model);
            return StatusCode(200, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            Reservation result = await _service.GetByIdAsync(id);
            if (result == null)
                return StatusCode(404, new ErrorDto("Reservation not found"));

            return StatusCode(200, result);
        }

        [HttpDelete("id")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteById([FromBody] string id)
        {
            DeleteResult result = await _service.DeleteByIdAsync(id);
            if (result.DeletedCount == 0)
                return StatusCode(404, new ErrorDto("Reservation not found"));

            return StatusCode(200);
        }

        [HttpPut]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Update([FromBody] ReservationDto reservationDto)
        {
            Reservation result = await _service.PutAsync(_mapper.Map<Reservation>(reservationDto));
            if (result == null)
                return StatusCode(400, new ErrorDto("Update error"));

            return StatusCode(200, result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
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

            var responseCabins = await _clientGetCabinsCount.GetResponse<GetCabinsCountResponse>(new GetCabinsCountEvent());

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
                .Where(d => d.Value == responseCabins.Message.Count)
                .Select(d => d.Key)
                .ToList();

            return StatusCode(200, fullyOccupiedDays);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateReservationDto dto)
        {
            var responseCabins = await _clientGetCabinsCount.GetResponse<GetCabinsCountResponse>(new GetCabinsCountEvent());

            int cabin = 0;
            for (int i = 1; i <= responseCabins.Message.Count; i++)
            {
                List<Reservation> list = await _service.GetForValidate(dto.Departure.Date.AddHours(11), dto.Arrive.Date.AddHours(14), i);
                if (list.Count == 0)
                {
                    cabin = i;
                    break;
                }
                if (i == responseCabins.Message.Count && list.Count != 0)
                    return StatusCode(404, new ErrorDto("No rooms available"));
            }

            ReservationDto result = _mapper.Map<ReservationDto>(dto);
            result.Status = Status.Validated;
            result.Cabin = cabin;

            //Get Price
            GetPriceEvent getPriceEvent = new GetPriceEvent()
            {
                Departure = dto.Departure,
            };

            var responsePrice = await _clientGetPrice.GetResponse<GetPriceResponse>(getPriceEvent);

            //Calculate difference between days
            TimeSpan difference = dto.Departure.Subtract(dto.Arrive);
            int daysDifference = (int)difference.TotalDays;

            //multiply price on days
            result.PaymentAmount = responsePrice.Message.Price * daysDifference;
            return StatusCode(200, result);
        }

    }
}
