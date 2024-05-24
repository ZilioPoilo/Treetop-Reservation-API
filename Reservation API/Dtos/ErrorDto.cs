using System.ComponentModel.DataAnnotations;

namespace Reservation_API.Dtos
{
    public class ErrorDto
    {
        public ErrorDto(string message)
        {
            Id = Guid.NewGuid().ToString();
            Message = message;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ErrorDto()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [Required]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or Sets Message
        /// </summary>
        [Required]
        public string Message { get; set; } = null!;
    }
}
