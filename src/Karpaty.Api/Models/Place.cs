namespace Karpaty.Api.Models
{
    /// <summary>
    /// Туристичне місце в Карпатах.
    /// </summary>
    public class Place
    {
        /// <summary>Унікальний ідентифікатор місця.</summary>
        /// <example>6720f1b2c3d4e5f6a7b8c9d0</example>
        public string id { get; set; } = string.Empty;

        /// <summary>Назва місця.</summary>
        /// <example>Водоспад Шипіт</example>
        public string name { get; set; } = string.Empty;

        /// <summary>Категорія: waterfall, viewpoint або meadow.</summary>
        /// <example>waterfall</example>
        public PlaceCategory category { get; set; }

        /// <summary>Регіон Карпат.</summary>
        /// <example>Західні_Карпати</example>
        public PlaceRegion region { get; set; }

        /// <summary>Опис місця.</summary>
        /// <example>Один із найвідоміших водоспадів Закарпаття, висота близько 14 метрів.</example>
        public string description { get; set; } = string.Empty;

        /// <summary>Складність маршруту: easy, medium або hard.</summary>
        /// <example>easy</example>
        public PlaceDifficulty difficulty { get; set; }

        /// <summary>Широта.</summary>
        /// <example>48.5361</example>
        public float latitude { get; set; }

        /// <summary>Довгота.</summary>
        /// <example>23.2417</example>
        public float longitude { get; set; }

        /// <summary>Посилання на зображення.</summary>
        /// <example>https://example.com/shypit.jpg</example>
        public string imageUrl { get; set; } = string.Empty;
    }

    /// <summary>Категорія місця.</summary>
    public enum PlaceCategory
    {
        /// <summary>Водоспад.</summary>
        waterfall,
        /// <summary>Оглядовий майданчик.</summary>
        viewpoint,
        /// <summary>Полонина.</summary>
        meadow
    }

    /// <summary>Регіон Карпат.</summary>
    public enum PlaceRegion
    {
        /// <summary>Східні Карпати.</summary>
        Східні_Карпати,
        /// <summary>Західні Карпати.</summary>
        Західні_Карпати,
        /// <summary>Південні Карпати.</summary>
        Південні_Карпати
    }

    /// <summary>Складність маршруту.</summary>
    public enum PlaceDifficulty
    {
        /// <summary>Легка.</summary>
        easy,
        /// <summary>Середня.</summary>
        medium,
        /// <summary>Складна.</summary>
        hard
    }
}