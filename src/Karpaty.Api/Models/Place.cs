namespace Karpaty.Api.Models
{
    public class Place
    {
        public string id { get; set; }
        public string name { get; set; }
        public PlaceCategory category { get; set; }
        public PlaceRegion region { get; set; }
        public string description { get; set; }
        public PlaceDifficulty difficulty { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string imageUrl { get; set; }
    }
    public enum PlaceCategory
    {
        Водоспад,
        Оглядовий_Майданчик,
        Полонина
    }
    public enum PlaceRegion
    {
        Східні_Карпати,
        Західні_Карпати,
        Південні_Карпати
    }
    public enum PlaceDifficulty
    {
        Легко,
        Середнє,
        Тяжко
    }
}
