using CinemasOfSity.Models;
using CinemasOfSity.Models.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemasOfSity.Controllers
{
    public class MoviesController : Controller
    {
        int pageSize = 2;
        DataContext dataContext;

        public MoviesController(DataContext context)
        {
            dataContext = context;
        }

        IDictionary<string, string> GetSortingCriteria()
        {
            return new Dictionary<string, string>()
            {
                { "duration", "Сортировать по продолжительности" },
                { "", "Без сортировки" }
            };
        }

        List<int> GetAgeLimits()
        {
            return new List<int>() { 0, 6, 12, 16, 18 };
        }

        async Task LoadPageData()
        {
            ViewBag.ControllerName = "Movies";
            ViewBag.SortingCriteria = GetSortingCriteria();
            ViewBag.MovieTitles = await dataContext.Movies.Select(x => x.Title).Distinct().ToListAsync();
            ViewBag.MovieGenres = await dataContext.MovieGenres.Select(x => x.Name).ToListAsync();
            ViewBag.MovieCountries = await dataContext.MovieCountries.Select(x => x.Name).ToListAsync();
            ViewBag.MovieFormats = await dataContext.Movies.Select(x => x.Format).Distinct().ToListAsync();
            ViewBag.MovieDirectors = await dataContext.Movies.Select(x => x.Director).Distinct().ToListAsync();
            ViewBag.AgeLimits = GetAgeLimits();
            if (User.Identity.Name != null)
            {
                Models.Account.User user = await dataContext.Users.Include(x => x.Role).Where(x => x.Login == User.Identity.Name).FirstAsync();
                ViewBag.UserRole = user.Role.Name;
                ViewBag.UserLogin = user.Login;
            }
        }

        public async Task<IActionResult> Index(bool partialView = false, MoviesFilter filter = null)
        {
            MoviesView viewModel = new MoviesView();
            List<Movie> listItems = await dataContext.Movies.Include(x => x.Genre). Include(x => x.Country).ToListAsync();
            listItems.Reverse();
            if (filter != null)
            {
                viewModel.Filter = filter;
                listItems = Filtration(listItems, viewModel.Filter);
            }
            else if (HttpContext.Session.Get<MoviesFilter>("MoviesFilter") != null)
            {
                viewModel.Filter = HttpContext.Session.Get<MoviesFilter>("MoviesFilter");
                listItems = Filtration(listItems, viewModel.Filter);
            }
            else viewModel.Filter = new MoviesFilter();
            viewModel.DataList = GetDataList(listItems, 1);
            viewModel.AddNewItem = new Models.AddNewItem.AddNewItem()
            {
                Title = "Добавить фильм",
                Action = Url.Action("Add", "Movies"),
                UpdateTagId = "updatedSectionsPage",
                AccessRoles = new List<string>()
                    {
                        "Администратор",
                        "Оператор"
                    }
            };
            ModelState.Clear();
            await LoadPageData();
            if (partialView) return PartialView(viewModel);
            else return View(viewModel);
        } 

        async Task<IActionResult> UpdatePage(int page = 1)
        {
            UpdatedSectionsPage viewModel = new UpdatedSectionsPage();
            List<Movie> listItems = await dataContext.Movies.Include(x => x.Genre).Include(x => x.Country).ToListAsync();
            listItems.Reverse();
            if (HttpContext.Session.Get<MoviesFilter>("MoviesFilter") != null)
            {
                viewModel.Filter = HttpContext.Session.Get<MoviesFilter>("MoviesFilter");
                listItems = Filtration(listItems, viewModel.Filter);
            }
            else viewModel.Filter = new MoviesFilter();
            viewModel.DataList = GetDataList(listItems, page);
            await LoadPageData();
            return PartialView("UpdatedSectionsPage", viewModel);
        }

        public async Task<IActionResult> NewPage(int page = 1)
        {
            List<Movie> listItems = await dataContext.Movies.Include(x => x.Genre).Include(x => x.Country).ToListAsync();
            listItems.Reverse();
            if (HttpContext.Session.Get<MoviesFilter>("MoviesFilter") != null)
            {
                listItems = Filtration(listItems, HttpContext.Session.Get<MoviesFilter>("MoviesFilter"));
            }
            MoviesDataList dataList = GetDataList(listItems, page);
            await LoadPageData();
            return PartialView("MoviesDataList", dataList);
        }

        public async Task<IActionResult> Filter(MoviesFilter filter)
        {
            List<Movie> listItems = await dataContext.Movies.Include(x => x.Genre).Include(x => x.Country).ToListAsync();
            listItems.Reverse();
            listItems = Filtration(listItems, filter);
            MoviesDataList dataList = GetDataList(listItems, 1);
            await LoadPageData();
            return PartialView("MoviesDataList", dataList);
        }

        [Authorize(Roles = "Администратор, Оператор")]
        public async Task<IActionResult> Add(MovieResult addItem)
        {
            if (ModelState.IsValid && await CheckErrors(addItem))
            {
                Movie item = new Movie();
                item.Genre = new List<MovieGenre>();
                List<string> genres = addItem.Genres.Split(',').ToList();
                foreach (string genre in genres)
                {
                    string genreName = genre.Trim();
                    MovieGenre movieGenre;
                    if (!await dataContext.MovieGenres.AnyAsync(x => x.Name.ToLower() == genreName.ToLower()))
                    {
                        movieGenre = new MovieGenre()
                        {
                            Name = genreName
                        };
                        await dataContext.MovieGenres.AddAsync(movieGenre);
                    }
                    else movieGenre = await dataContext.MovieGenres.FirstAsync(x => x.Name.ToLower() == genreName.ToLower());
                    item.Genre.Add(movieGenre);
                }
                item.Country = new List<MovieCountry>();
                List<string> countries = addItem.Countries.Split(',').ToList();
                foreach (string country in countries)
                {
                    string countryName = country.Trim();
                    MovieCountry movieCountry;
                    if (!await dataContext.MovieCountries.AnyAsync(x => x.Name.ToLower() == countryName.ToLower()))
                    {
                        movieCountry = new MovieCountry()
                        {
                            Name = countryName
                        };
                        await dataContext.MovieCountries.AddAsync(movieCountry);
                    }
                    else movieCountry = await dataContext.MovieCountries.FirstAsync(x => x.Name.ToLower() == countryName.ToLower());
                    item.Country.Add(movieCountry);
                }
                if (addItem.Duration != null) item.Duration = TimeSpan.Parse(addItem.Duration);
                var itemProperties = item.GetType().GetProperties();
                var itemUpdateProperties = addItem.GetType().GetProperties();
                foreach (var itemUpdateProperty in itemUpdateProperties)
                {
                    if (itemProperties.Any(cinemaProperty => cinemaProperty.Name == itemUpdateProperty.Name))
                    {
                        var itemProperty = itemProperties.First(x => x.Name == itemUpdateProperty.Name);
                        if (itemProperty.PropertyType == itemUpdateProperty.PropertyType)
                        {
                            itemProperty.SetValue(item, itemUpdateProperty.GetValue(addItem));
                        }
                    }
                }
                await dataContext.Movies.AddAsync(item);
                await dataContext.SaveChangesAsync();
            }
            else LoadErrors();
            return await UpdatePage(1);


            async Task<bool> CheckErrors(MovieResult addItem)
            {
                List<string> errorList = new List<string>();
                if (await dataContext.Movies.AnyAsync(x => x.Title.ToLower() == addItem.Title.ToLower()))
                {
                    errorList.Add("Ошибка. Названеи фильма. Фильм с таким названием уже есть в базе.");
                }
                if (errorList.Count > 0)
                {
                    foreach (string error in errorList)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return false;
                }
                return true;
            }
        }

        [Authorize(Roles = "Администратор, Оператор")]
        public async Task<IActionResult> Update(MovieResult updateItem, int page)
        {
            if (ModelState.IsValid && await CheckErrors(updateItem))
            {
                Movie item = await dataContext.Movies.Where(x => x.Id == updateItem.Id).Include(x => x.Genre).Include(x => x.Country).FirstAsync();
                if (updateItem.Duration != null) item.Duration = TimeSpan.Parse(updateItem.Duration);
                else item.Duration = new TimeSpan();
                List<string> genres = updateItem.Genres.Split(',').ToList();
                List<MovieGenre> oldGenres = item.Genre;
                item.Genre.Clear();
                foreach (string genre in genres)
                {
                    string genreName = genre.Trim();
                    MovieGenre movieGenre;
                    if (!await dataContext.MovieGenres.AnyAsync(x => x.Name.ToLower() == genreName.ToLower()))
                    {
                        movieGenre = new MovieGenre()
                        {
                            Name = genreName
                        };
                        await dataContext.MovieGenres.AddAsync(movieGenre);
                    }
                    else movieGenre = await dataContext.MovieGenres.FirstAsync(x => x.Name.ToLower() == genreName.ToLower());
                    item.Genre.Add(movieGenre);
                }
                List<string> countries = (updateItem.Countries != null) ? updateItem.Countries.Split(',').ToList() : new List<string>();
                List<MovieCountry> oldCountries = item.Country;
                item.Country.Clear();
                foreach (string country in countries)
                {
                    string countryName = country.Trim();
                    MovieCountry movieCountry;
                    if (!await dataContext.MovieCountries.AnyAsync(x => x.Name.ToLower() == countryName.ToLower()))
                    {
                        movieCountry = new MovieCountry()
                        {
                            Name = countryName
                        };
                        await dataContext.MovieCountries.AddAsync(movieCountry);
                    }
                    else movieCountry = await dataContext.MovieCountries.FirstAsync(x => x.Name.ToLower() == countryName.ToLower());
                    item.Country.Add(movieCountry);
                }
                var itemProperties = item.GetType().GetProperties();
                var itemUpdateProperties = updateItem.GetType().GetProperties();
                foreach (var itemUpdateProperty in itemUpdateProperties)
                {
                    if (itemProperties.Any(cinemaProperty => cinemaProperty.Name == itemUpdateProperty.Name))
                    {
                        var itemProperty = itemProperties.First(x => x.Name == itemUpdateProperty.Name);
                        if (itemProperty.PropertyType == itemUpdateProperty.PropertyType)
                        {
                            itemProperty.SetValue(item, itemUpdateProperty.GetValue(updateItem));
                        }
                    }
                }
                foreach (MovieGenre oldGenre in oldGenres)
                {
                    if (oldGenre.Movie.Count == 0) dataContext.MovieGenres.Remove(oldGenre);
                }
                foreach (MovieCountry oldCountry in oldCountries)
                {
                    if (oldCountry.Movie.Count == 0) dataContext.MovieCountries.Remove(oldCountry);
                }
                await dataContext.SaveChangesAsync();
            }
            else LoadErrors();
            return await UpdatePage(page);


            async Task<bool> CheckErrors(MovieResult updateItem)
            {
                List<string> errorList = new List<string>();
                if (!await dataContext.Movies.AnyAsync(x => x.Id == updateItem.Id))
                {
                    errorList.Add("Ошибка идентификации данных.");
                }
                if (await dataContext.Movies.AnyAsync(x => x.Title.ToLower() == updateItem.Title.ToLower() && x.Id != updateItem.Id))
                {
                    errorList.Add("Ошибка. Названеи фильма. Фильм с таким названием уже есть в базе.");
                }
                if (errorList.Count > 0)
                {
                    foreach (string error in errorList)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return false;
                }
                return true;
            }
        }

        [Authorize(Roles = "Администратор, Оператор")]
        public async Task<IActionResult> Delete(int id, int page)
        {
            if (await CheckErrors(id))
            {
                Movie item = await dataContext.Movies.Where(x => x.Id == id).Include(x => x.Genre).ThenInclude(x => x.Movie).Include(x => x.Country).ThenInclude(x => x.Movie).FirstAsync();
                List<Models.CinemaSessions.CinemaSession> sessions = await dataContext.CinemaSessions.Where(x => x.Movie.Id == item.Id).ToListAsync();
                List<MovieGenre> oldGenres = item.Genre;
                List<MovieCountry> oldCountries = item.Country;
                foreach (Models.CinemaSessions.CinemaSession session in sessions) dataContext.CinemaSessions.Remove(session);
                dataContext.Movies.Remove(item);
                await dataContext.SaveChangesAsync();
                foreach (MovieGenre oldGenre in oldGenres)
                {
                    if (oldGenre.Movie.Count == 0) dataContext.MovieGenres.Remove(oldGenre);
                }
                foreach (MovieCountry oldCountry in oldCountries)
                {
                    if (oldCountry.Movie.Count == 0) dataContext.MovieCountries.Remove(oldCountry);
                }
                await dataContext.SaveChangesAsync();
            }
            else LoadErrors();
            return await UpdatePage(page);


            async Task<bool> CheckErrors(int id)
            {
                if (!await dataContext.Movies.AnyAsync(x => x.Id == id))
                {
                    ModelState.AddModelError("", "Ошибка. Ошибка идентификации данных.");
                    return false;
                }
                return true;
            }
        }

        MoviesDataList GetDataList(List<Movie> listItems, int page)
        {
            int totalCount = listItems.Count();
            int totalPages = (totalCount != 0) ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            if (page < 1) page = 1;
            else if (page > totalPages) page = totalPages;
            List<Movie> listItemsPage = listItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            Pagination pagination = new Pagination()
            {
                PageNumber = page,
                TotalPages = totalPages,
                Action = Url.Action("NewPage", "Movies", new { page = "x" }),
                UpdateTagId = "dataList-box"
            };
            return new MoviesDataList()
            {
                Data = listItemsPage,
                Pagination = pagination,
                UpdateTagId = "updatedSectionsPage",
                AccessRoles = new List<string>()
                {
                    "Администратор",
                    "Оператор"
                }
            };
        }

        void LoadErrors()
        {
            if (!ModelState.IsValid)
            {
                List<string> errorsList = new List<string>();
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        errorsList.Add(error.ErrorMessage);
                    }
                }
                ViewBag.Errors = errorsList;
            }
            else ViewBag.Errors = new List<string>();
        }

        List<Movie> Filtration(List<Movie> listItems, MoviesFilter filter)
        {
            HttpContext.Session.Set("MoviesFilter", filter);
            List<Movie> filtrationResult = listItems.Where((x) => FilterTest(x, filter)).ToList();
            filtrationResult = Sorting(filtrationResult, filter.SortingCriterion);
            return filtrationResult;
        }

        List<Movie> Sorting(List<Movie> listItems, string sortingCriterion)
        {
            if (sortingCriterion == "duration")
            {
                listItems.Sort((x, y) => TimeSpan.Compare(x.Duration, y.Duration) > 0 ? 1 : TimeSpan.Compare(x.Duration, y.Duration) == 0 ? 0 : -1);
            }
            return listItems;
        }

        bool FilterTest(Movie item, MoviesFilter filter)
        {
            if (filter.MovieTitle != null && !item.Title.ToLower().Contains(filter.MovieTitle.ToLower())) return false;
            if (filter.DurationMin != null)
            {
                TimeSpan durationMin = TimeSpan.Parse(filter.DurationMin);
                if (TimeSpan.Compare(item.Duration, durationMin) < 0) return false;
            }
            if (filter.DurationMax != null)
            {
                TimeSpan durationMax = TimeSpan.Parse(filter.DurationMax);
                if (TimeSpan.Compare(item.Duration, durationMax) > 0) return false;
            }
            if (filter.AgeLimitMax > 0)
            {
                if (item.AgeLimit > filter.AgeLimitMax) return false;
            }
            if (filter.AgeLimitMin > 0)
            {
                if (item.AgeLimit < filter.AgeLimitMin) return false;
            }
            if (filter.Genres != null)
            {
                bool genreContains = false;
                foreach (var genreName in filter.Genres)
                {
                    if (item.Genre.Any(x => x.Name.ToLower() == genreName.ToLower()))
                    {
                        genreContains = true;
                        break;
                    }
                }
                if (!genreContains) return false;
            }
            if (filter.Countries != null)
            {
                bool countryContains = false;
                foreach (var countryName in filter.Countries)
                {
                    if (item.Country.Any(x => x.Name.ToLower() == countryName.ToLower()))
                    {
                        countryContains = true;
                        break;
                    }
                }
                if (!countryContains) return false;
            }
            if (filter.Formats != null)
            {
                if (!filter.Formats.Contains(item.Format)) return false;
            }
            if (filter.Directors != null)
            {
                if (!filter.Directors.Contains(item.Director)) return false;
            }
            return true;
        }
    }
}
