using System.ComponentModel.DataAnnotations;
using Karpaty.Api.Models;

namespace Karpaty.Api.DTO
{
    /// <summary>
    /// Дані для створення нового місця.
    /// </summary>
    public class CreatePlaceDTO
    {
        /// <summary>Назва місця (2-100 символів, унікальна).</summary>
        /// <example>Водоспад Труфанець</example>
        [Required(ErrorMessage = "Назва обов'язкова")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів")]
        public string name { get; set; } = string.Empty;

        /// <summary>Категорія: waterfall, viewpoint або meadow.</summary>
        /// <example>waterfall</example>
        [Required(ErrorMessage = "Категорія обов'язкова")]
        [EnumDataType(typeof(PlaceCategory), ErrorMessage = "Недопустима категорія. Дозволені: waterfall, viewpoint, meadow")]
        public PlaceCategory? category { get; set; }

        /// <summary>Регіон Карпат.</summary>
        /// <example>Західні_Карпати</example>
        [Required(ErrorMessage = "Регіон обов'язковий")]
        [EnumDataType(typeof(PlaceRegion), ErrorMessage = "Недопустимий регіон")]
        public PlaceRegion? region { get; set; }

        /// <summary>Опис місця (10-2000 символів).</summary>
        /// <example>Мальовничий каскадний водоспад серед лісу.</example>
        [Required(ErrorMessage = "Опис обов'язковий")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Опис має містити від 10 до 2000 символів")]
        public string description { get; set; } = string.Empty;

        /// <summary>Складність: easy, medium або hard.</summary>
        /// <example>easy</example>
        [Required(ErrorMessage = "Складність обов'язкова")]
        [EnumDataType(typeof(PlaceDifficulty), ErrorMessage = "Недопустима складність. Дозволені: easy, medium, hard")]
        public PlaceDifficulty? difficulty { get; set; }

        /// <summary>Широта (від -90 до 90).</summary>
        /// <example>48.5</example>
        [Required(ErrorMessage = "Широта обов'язкова")]
        [Range(-90.0, 90.0, ErrorMessage = "Широта має бути від -90 до 90")]
        public float? latitude { get; set; }

        /// <summary>Довгота (від -180 до 180).</summary>
        /// <example>24.0</example>
        [Required(ErrorMessage = "Довгота обов'язкова")]
        [Range(-180.0, 180.0, ErrorMessage = "Довгота має бути від -180 до 180")]
        public float? longitude { get; set; }

        /// <summary>Посилання на зображення (необов'язкове).</summary>
        /// <example>https://example.com/trufanets.jpg</example>
        [StringLength(500, ErrorMessage = "Посилання на зображення занадто довге (максимум 500 символів)")]
        public string? imageUrl { get; set; }
    }
}