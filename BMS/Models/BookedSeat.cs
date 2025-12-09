using System.ComponentModel.DataAnnotations;
namespace BMS.Models
{
    public class BookedSeat
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public string Theatre { get; set; } = string.Empty;

        [Required]
        public string Date { get; set; } = string.Empty;

        [Required]
        public string Time { get; set; } = string.Empty;

        [Required]
        public string SeatNumber { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public string Format { get; set; } = "";
    }
}