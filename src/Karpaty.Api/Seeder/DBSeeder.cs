using Karpaty.Api.DBCommunication.Reposetories;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.Models;

namespace Karpaty.Api.Seeder
{
    
    public static class DBSeeder
    {
        public static async Task SeedPlacesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPlaceRepository>();

            var existingPlaces = await repository.GetAllAsync();

            if (existingPlaces == null || existingPlaces.Count == 0)
            {
                var placesToSeed = new List<Place>
                {
                    new Place
                    {
                        name = "Водоспад Шипіт",
                        category = PlaceCategory.Водоспад,
                        region = PlaceRegion.Західні_Карпати,
                        difficulty = PlaceDifficulty.Легко,
                        latitude = 48.6565f,
                        longitude = 23.2694f,
                        description = "Один з найкрасивіших водоспадів Закарпаття, розташований біля підніжжя гори Гемба. Висота падіння води — 14 метрів.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Shypit_waterfall.jpg/800px-Shypit_waterfall.jpg"
                    },
                    new Place
                    {
                        name = "Полонина Боржава",
                        category = PlaceCategory.Полонина,
                        region = PlaceRegion.Західні_Карпати,
                        difficulty = PlaceDifficulty.Середнє,
                        latitude = 48.6253f,
                        longitude = 23.2798f,
                        description = "Гірський масив (полонина) на Закарпатті. Найдовша полонина Закарпаття, ідеальна для піших походів і збору чорниці.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/ca/Borzhava_panorama.jpg/800px-Borzhava_panorama.jpg"
                    },
                    new Place
                    {
                        name = "Говерла",
                        category = PlaceCategory.Оглядовий_Майданчик,
                        region = PlaceRegion.Східні_Карпати,
                        difficulty = PlaceDifficulty.Тяжко,
                        latitude = 48.1602f,
                        longitude = 24.5000f,
                        description = "Найвища вершина Українських Карпат та найвища точка України (2061 м). Популярний туристичний маршрут.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/12/Hoverla_mountain.jpg/800px-Hoverla_mountain.jpg"
                    },
                    new Place
                    {
                        name = "Манявський водоспад",
                        category = PlaceCategory.Водоспад,
                        region = PlaceRegion.Східні_Карпати,
                        difficulty = PlaceDifficulty.Середнє,
                        latitude = 48.6247f,
                        longitude = 24.3644f,
                        description = "Один із найвищих водоспадів Українських Карпат (близько 20 метрів). Розташований у мальовничій гірській ущелині.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7b/Manyava_waterfall.jpg/800px-Manyava_waterfall.jpg"
                    },
                    new Place
                    {
                        name = "Полонина Хом'яків",
                        category = PlaceCategory.Полонина,
                        region = PlaceRegion.Східні_Карпати,
                        difficulty = PlaceDifficulty.Середнє,
                        latitude = 48.3739f,
                        longitude = 24.5004f,
                        description = "Велика галявина біля підніжжя гори Хом'як, де влітку випасають худобу. Звідси відкриваються чудові краєвиди на Чорногірський хребет.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/36/Khomyak_mountain.jpg/800px-Khomyak_mountain.jpg"
                    },
                    new Place
                    {
                        name = "Водоспад Пробій",
                        category = PlaceCategory.Водоспад,
                        region = PlaceRegion.Східні_Карпати,
                        difficulty = PlaceDifficulty.Легко,
                        latitude = 48.4390f,
                        longitude = 24.5397f,
                        description = "Повноводний водоспад на річці Прут у місті Яремче. Вода тут падає з 8-метрової висоти. Є пішохідний місток над водоспадом.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/Probij_waterfall.jpg/800px-Probij_waterfall.jpg"
                    },
                    new Place
                    {
                        name = "Скелі Довбуша",
                        category = PlaceCategory.Оглядовий_Майданчик,
                        region = PlaceRegion.Східні_Карпати,
                        difficulty = PlaceDifficulty.Легко,
                        latitude = 49.0436f,
                        longitude = 23.6828f,
                        description = "Унікальний скельно-печерний комплекс. Скелі утворилися понад 70 млн років тому на дні палеогенового моря.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e0/Dovbush_rocks.jpg/800px-Dovbush_rocks.jpg"
                    },
                    new Place
                    {
                        name = "Піп Іван Чорногірський",
                        category = PlaceCategory.Оглядовий_Майданчик,
                        region = PlaceRegion.Східні_Карпати,
                        difficulty = PlaceDifficulty.Тяжко,
                        latitude = 48.0475f,
                        longitude = 24.6277f,
                        description = "Одна з найвищих вершин (2028 м). Відома завдяки кам'яній обсерваторії «Білий Слон» на самій вершині.",
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/36/Pip_Ivan_observatory.jpg/800px-Pip_Ivan_observatory.jpg"
                    }
                };

                foreach (var place in placesToSeed)
                {
                    await repository.CreateAsync(place);
                }
            }
        }
    }
}
