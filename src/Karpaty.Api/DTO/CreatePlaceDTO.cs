using Karpaty.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO
{
    public class CreatePlaceDTO
    {
        [Required]
        public string name { get; set; }
        [Required]
        public PlaceCategory category { get; set; }
        [Required]
        public PlaceRegion region { get; set; }
        public string description { get; set; }
        [Required]
        public PlaceDifficulty difficulty { get; set; }
        [Required]
        public float latitude { get; set; }
        [Required]
        public float longitude { get; set; }
        public string imageUrl { get; set; }
    }
}
