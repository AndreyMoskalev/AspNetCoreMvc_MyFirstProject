using CinemasOfSity.Models;
using CinemasOfSity.Models.Movies;
using CinemasOfSity.Models.Cinemas;
using CinemasOfSity.Models.CinemaSessions;

namespace CinemasOfSity
{
    public static class CreateTestData
    {

        public static void Initialize(DataContext context)
        {
            int cinemasCount = 10;
            int moviesCount = 10;
            int movieGenresCount = 18;
            int movieCountriesCount = 10;
            int movieFormatesCount = 12;
            int numberSessionsDayCinema = 7;
            List<int> movieAgeLimits = new List<int>() { 0, 6, 12, 16, 18 };
            
            Random random = new Random();
            if (!context.Cinemas.Any() && !context.Movies.Any() && !context.CinemaSessions.Any())
            {
                // заполнение таблицы "Кинотеатры"
                IDictionary<int, int[]> cinemaHalls = new Dictionary<int, int[]>();
                for (int i = 0; i < cinemasCount; i++)
                {
                    int hallsNumber = random.Next(1, 10);
                    int numberSets = random.Next(50, 100);
                    cinemaHalls.Add(i, new int[] { hallsNumber, numberSets });

                    context.Cinemas.AddRange(new Cinema
                    {
                        Name = $"Кинотеатр {i + 1}",
                        Address = $"Адрес {i + 1}",
                        Telephone = $"Телефон {i + 1}",
                        Email = $"Почта {i + 1}",
                        Description = $"Небольшое описание кинотеатра {i + 1}",
                        Capacity = hallsNumber * numberSets,
                        NumberOfHalls = hallsNumber
                    });
                }
                context.SaveChanges();

                // заполнение таблицы "Жанры фильма"
                for (int i = 0; i < movieGenresCount; i++)
                {
                    context.MovieGenres.AddRange(new MovieGenre
                    {
                        Name = $"Жанр {i + 1}"
                    });
                }
                context.SaveChanges();

                // заполнение таблицы "Страны фильма"
                for (int i = 0; i < movieCountriesCount; i++)
                {
                    context.MovieCountries.AddRange(new MovieCountry
                    {
                        Name = $"Страна {i + 1}"
                    });
                }
                context.SaveChanges();

                // заполнение таблицы "Фильмы"
                int genreIndex = 0;
                int countryIndex = 0;
                for (int i = 0; i < moviesCount; i++)
                {
                    List<int> genreIndexes = new List<int>();
                    int genresCount = random.Next(1, 4);
                    for (int j = 0; j < genresCount; j++)
                    {
                        genreIndexes.Add(genreIndex);
                        genreIndex = (genreIndex + 1) % movieGenresCount;
                    }
                    List<int> countryIndexes = new List<int>();
                    int countryCount = random.Next(1, 3);
                    for (int j = 0; j < countryCount; j++)
                    {
                        countryIndexes.Add(countryIndex);
                        countryIndex = (countryIndex + 1) % movieCountriesCount;
                    }
                    List<MovieGenre> genres = new List<MovieGenre>();;
                    List<MovieCountry> countries = new List<MovieCountry>();
                    foreach (var index in genreIndexes)
                    {
                        genres.Add(context.MovieGenres.Skip(index).First());
                    }
                    foreach (var index in countryIndexes)
                    {
                        countries.Add(context.MovieCountries.Skip(index).First());
                    }
                    int durationAllMinutes = random.Next(90, 200);
                    int durationMinute = durationAllMinutes % 60;
                    int durationHours = durationAllMinutes / 60;
                    TimeSpan duration = new TimeSpan(durationHours, durationMinute, 0);
                    Movie newMovie = new Movie
                    {
                        Title = $"Фильм {i + 1}",
                        Director = $"Режисcер {random.Next(1, moviesCount)}",
                        Description = $"Небольшое описание фильма {i + 1}",
                        Format = $"Формат {random.Next(1, movieFormatesCount)}",
                        AgeLimit = movieAgeLimits[random.Next(0, movieAgeLimits.Count - 1)],
                        Duration = duration,
                        Country = countries,
                        Genre = genres
                    };
                    context.Movies.AddRange(newMovie);
                }
                context.SaveChanges();

                // заполнение таблицы "Сеансы"
                double timeStepAllMinute = 24 / (double)numberSessionsDayCinema * 60;
                for (int i = 0; i < cinemasCount; i++)
                {
                    var cinema = context.Cinemas.Skip(i).Take(1).First();
                    for (int hall = 0; hall < cinemaHalls[i][0]; hall++)
                    {
                        for (int j = 0; j < numberSessionsDayCinema; j++)
                        {
                            int sessionTimeAllMinute = (j + 1) * (int)timeStepAllMinute;
                            int sessionTimeMinute = (int)timeStepAllMinute % 60;
                            int sessionTimeHours = (sessionTimeAllMinute - sessionTimeMinute) / 60;
                            int minPrice = 100;
                            int maxPrice = 250;
                            int price = sessionTimeHours * 12;
                            price = (price < minPrice) ? minPrice : price;
                            price = (price > maxPrice) ? maxPrice : price;
                            int previusSessionMovie = 0;
                            DateTime dateTimeLocal = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                            DateTime.Now.Day, sessionTimeHours, sessionTimeMinute, 0);
                            for (int day = 1; day <= 7; day++)
                            {
                                int sessionMovie = 0;
                                while (sessionMovie == previusSessionMovie) sessionMovie = random.Next(0, moviesCount - 1);
                                previusSessionMovie = sessionMovie;
                                DateTime thisDateTime = dateTimeLocal;
                                Movie movie = context.Movies.Skip(sessionMovie).First();
                                int numberTickets = random.Next(0, 100);
                                while (numberTickets > cinemaHalls[i][1]) numberTickets = random.Next(0, 100);
                                context.CinemaSessions.AddRange(
                                    new CinemaSession
                                    {
                                        Cinema = cinema,
                                        Movie = movie,
                                        Price = price,
                                        DateTime = thisDateTime,
                                        NumberOfTickets = numberTickets
                                    }
                                );
                                dateTimeLocal = dateTimeLocal.AddDays(1);
                            }
                        }
                    }
                }
                context.SaveChanges();
            }          
        }
    }
}
