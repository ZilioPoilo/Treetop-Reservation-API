using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservation_API.AppMapping;
using Reservation_API.Dtos;
using Reservation_API.Models;
using Reservation_API.Services.DataServices;

namespace Reservation_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        public readonly ReservationService _service;
        public readonly IMapper _mapper;

        public ReservationController(IMapper mapper, ReservationService service)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservationDto reservationDto)
        {
            var model = await _service.CreateAsync(_mapper.Map<Reservation>(reservationDto));
            var dto = _mapper.Map<ReservationDto>(model);

            return StatusCode(200, dto);
        }
    }
}
