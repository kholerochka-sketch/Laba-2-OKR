namespace Karpaty.Api.Models
{
    /// <summary>
    /// Порада для походу в Карпати.
    /// </summary>
    public class Tip
    {
        /// <summary>Унікальний ідентифікатор поради.</summary>
        /// <example>6720f1b2c3d4e5f6a7b8c9d1</example>
        public string id { get; set; } = string.Empty;

        /// <summary>Заголовок поради.</summary>
        /// <example>Зручне взуття</example>
        public string title { get; set; } = string.Empty;

        /// <summary>Категорія: спорядження, безпека, погода, поведінка, прибирання.</summary>
        /// <example>спорядження</example>
        public string category { get; set; } = string.Empty;

        /// <summary>Текст поради.</summary>
        /// <example>Беріть трекінгове взуття з протектором і вже розношене.</example>
        public string text { get; set; } = string.Empty;
    }
}