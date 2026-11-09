using System.ComponentModel.DataAnnotations;
using Karpaty.Api.Models;

namespace Karpaty.Api.DTO
{
    /// <summary>
    /// Дані для зміни місця. Передайте одне поле або кілька, відсутнє поле не змінюється.
    /// </summary>
    public class UpdatePlaceDTO
    {
        /// <summary>Нова назва (2-100 символів, унікальна).</summary>
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів")]
        public string? name { get; set; }

        /// <summary>Нова категорія: waterfall, viewpoint або meadow.</summary>
        [EnumDataType(typeof(PlaceCategory), ErrorMessage = "Недопустима категорія. Дозволені: waterfall, viewpoint, meadow")]
        public PlaceCategory? category { get; set; }

        /// <summary>Новий регіон.</summary>
        [EnumDataType(typeof(PlaceRegion), ErrorMessage = "Недопустимий регіон")]
        public PlaceRegion? region { get; set; }

        /// <summary>Новий опис (10-2000 символів).</summary>
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Опис має містити від 10 до 2000 символів")]
        public string? description { get; set; }

        /// <summary>Нова складність: easy, medium або hard.</summary>
        [EnumDataType(typeof(PlaceDifficulty), ErrorMessage = "Недопустима складність. Дозволені: easy, medium, hard")]
        public PlaceDifficulty? difficulty { get; set; }

        /// <summary>Нова широта (від -90 до 90).</summary>
        [Range(-90.0, 90.0, ErrorMessage = "Широта має бути від -90 до 90")]
        public float? latitude { get; set; }

        /// <summary>Нова довгота (від -180 до 180).</summary>
        [Range(-180.0, 180.0, ErrorMessage = "Довгота має бути від -180 до 180")]
        public float? longitude { get; set; }

        /// <summary>Нове посилання на зображення.</summary>
        [StringLength(500, ErrorMessage = "Посилання на зображення занадто довге (максимум 500 символів)")]
        public string? imageUrl { get; set; }
    }
}