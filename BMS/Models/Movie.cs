using System.ComponentModel.DataAnnotations;
namespace BMS.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public int TmdbId { get; set; }

        public required string Title { get; set; }   

        public required string Description { get; set; }
        
        [Display(Name = "Poster File Name")]        
        public string? PosterFileName { get; set; }

        [Display(Name = "Runtime (mins)")]
        public int Duration { get; set; }

        [Display(Name = "Age Rating")]
        public required string AgeRating { get; set; }

        public double ImdbRating { get; set; }
        public int VoteCount { get; set; }

    }
}
