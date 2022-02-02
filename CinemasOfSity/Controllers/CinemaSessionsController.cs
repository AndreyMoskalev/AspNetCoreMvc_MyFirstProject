using System;
using CinemasOfSity.Models;
using CinemasOfSity.Models.CinemaSessions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

namespace CinemasOfSity.Controllers
{
    public class CinemaSessionsController : Controller
    {
        int pageSize = 3;
        DataContext dataContext;

        public CinemaSessionsController (DataContext context)
        {
            dataContext = context;
        }

        IDictionary<string, string> GetSortingCriteria()
        {
            return new Dictionary<string, string>()
            {
                { "price", "Сортировать по цене" },
                { "dateTime", "Сортировать по дате и времени" },
                { "", "Без сортировки" }
            };
        }

        async Task LoadPageData()
        {
            await Task.Run(() =>
            {
                ViewBag.ControllerName = "CinemaSessions";
                ViewBag.SortingCriteria = GetSortingCriteria();
                ViewBag.MovieTitles = dataContext.Movies.Select(x => x.Title).Distinct().ToList();
                ViewBag.CinemaNames = dataContext.Cinemas.Select(x => x.Name).Distinct().ToList();
                if (User.Identity.Name != null)
                {
                    Models.Account.User user = dataContext.Users.Include(x => x.Role).Where(x => x.Login == User.Identity.Name).First();
                    ViewBag.UserRole = user.Role.Name;
                    ViewBag.UserLogin = user.Login;
                }
            });
        }

        public async Task<IActionResult> Index(bool partialView = false, CinemaSessionsFilter filter = null)
        {
            CinemaSessionsView viewModel = new CinemaSessionsView();
            List<CinemaSession> listItems = await dataContext.CinemaSessions.Include(x => x.Movie).Include(x => x.Cinema).ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                if (filter != null)
                {
                    viewModel.Filter = filter;
                    listItems = Filtration(listItems, viewModel.Filter);
                }
                else if (HttpContext.Session.Get<CinemaSessionsFilter>("CinemaSessionsFilter") != null)
                {
                    viewModel.Filter = HttpContext.Session.Get<CinemaSessionsFilter>("CinemaSessionsFilter");
                    listItems = Filtration(listItems, viewModel.Filter);
                }
                else viewModel.Filter = new CinemaSessionsFilter();
                viewModel.DataList = GetDataList(listItems, 1);
                viewModel.AddNewItem = new Models.AddNewItem.AddNewItem()
                {
                    Title = "Добавить сеанс",
                    Action = Url.Action("Add", "CinemaSession"),
                    UpdateTagId = "updatedSectionsPage",
                    AccessRoles = new List<string>()
                    {
                        "Администратор",
                        "Оператор"
                    }
                };
                ModelState.Clear();
            });
            await LoadPageData();
            if (partialView) return PartialView(viewModel);
            else return View(viewModel);
        }

        async Task<IActionResult> UpdatePage(int page = 1)
        {
            UpdatedSectionsPage viewModel = new UpdatedSectionsPage();
            List<CinemaSession> listItems = await dataContext.CinemaSessions.Include(x => x.Movie).Include(x => x.Cinema).ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                if (HttpContext.Session.Get<CinemaSessionsFilter>("CinemaSessionsFilter") != null)
                {
                    viewModel.Filter = HttpContext.Session.Get<CinemaSessionsFilter>("CinemaSessionsFilter");
                    listItems = Filtration(listItems, viewModel.Filter);
                }
                else viewModel.Filter = new CinemaSessionsFilter();
                viewModel.DataList = GetDataList(listItems, page);
            });
            await LoadPageData();
            return PartialView("UpdatedSectionsPage", viewModel);
        }

        public async Task<IActionResult> NewPage(int page = 1)
        {
            CinemaSessionsDataList dataList = new CinemaSessionsDataList();
            List<CinemaSession> listItems = await dataContext.CinemaSessions.Include(x => x.Movie).Include(x => x.Cinema).ToListAsync();
            listItems.Reverse();
            if (HttpContext.Session.Get<CinemaSessionsFilter>("CinemaSessionsFilter") != null)
            {
                listItems = Filtration(listItems, HttpContext.Session.Get<CinemaSessionsFilter>("CinemaSessionsFilter"));
            }
            dataList = GetDataList(listItems, page);
            await LoadPageData();
            return PartialView("CinemaSessionsDataList", dataList);
        }

        public async Task<IActionResult> Filter(CinemaSessionsFilter filter)
        {
            CinemaSessionsDataList dataList = new CinemaSessionsDataList();
            List<CinemaSession> listItems = await dataContext.CinemaSessions.Include(x => x.Movie).Include(x => x.Cinema).ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                listItems = Filtration(listItems, filter);
                dataList = GetDataList(listItems, 1);
            });
            await LoadPageData();
            return PartialView("CinemaSessionsDataList", dataList);
        }

        [Authorize(Roles = "Администратор, Оператор")]
        public async Task<IActionResult> Add(CinemaSessionResult addItem)
        {
            if (ModelState.IsValid && await CheckErrors(addItem))
            {
                CinemaSession item = new CinemaSession();
                await Task.Run(() =>
                {
                    item.Cinema = dataContext.Cinemas.Where(x => x.Name.ToLower() == addItem.CinemaName.ToLower()).First();
                    item.Movie = dataContext.Movies.Where(x => x.Title.ToLower() == addItem.MovieTitle.ToLower()).First();
                    item.DateTime = DateTime.Parse(addItem.DateTime);
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
                    dataContext.CinemaSessions.Add(item);
                    dataContext.SaveChanges();
                });
            }
            else LoadErrors();
            return await UpdatePage(1);


            async Task<bool> CheckErrors(CinemaSessionResult addItem)
            {
                List<string> errorList = new List<string>();
                if (!await dataContext.Cinemas.AnyAsync(x => x.Name.ToLower() == addItem.CinemaName.ToLower()))
                {
                    errorList.Add("Ошибка. Кинотеатр. Заданного кинотеатра нет в базе.");
                }
                if (!await dataContext.Movies.AnyAsync(x => x.Title.ToLower() == addItem.MovieTitle.ToLower()))
                {
                    errorList.Add("Ошибка. Фильм. Заданного фильма нет в базе.");
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
        public async Task<IActionResult> Update(CinemaSessionResult updateItem, int page)
        {
            if (ModelState.IsValid && await CheckErrors(updateItem))
            {
                CinemaSession item = await dataContext.CinemaSessions.Where(x => x.Id == updateItem.Id).Include(x => x.Movie).Include(x => x.Cinema).FirstAsync();
                await Task.Run(() =>
                {
                    item.Cinema = dataContext.Cinemas.Where(x => x.Name.ToLower() == updateItem.CinemaName.ToLower()).First();
                    item.Movie = dataContext.Movies.Where(x => x.Title.ToLower() == updateItem.MovieTitle.ToLower()).First();
                    string dateTimeString = updateItem.Date + "T" + updateItem.Time;
                    item.DateTime = DateTime.Parse(dateTimeString);
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
                    dataContext.SaveChanges();
                });
            }
            else LoadErrors();
            return await UpdatePage(page);

            async Task<bool> CheckErrors(CinemaSessionResult updateItem)
            {
                List<string> errorList = new List<string>();
                if (!await dataContext.CinemaSessions.AnyAsync(x => x.Id == updateItem.Id))
                {
                    errorList.Add("Ошибка идентификации данных.");
                }
                if (!await dataContext.Cinemas.AnyAsync(x => x.Name.ToLower() == updateItem.CinemaName.ToLower()))
                {
                    errorList.Add("Ошибка. Кинотеатр. Заданного кинотеатра нет в базе.");
                }
                if (!await dataContext.Movies.AnyAsync(x => x.Title.ToLower() == updateItem.MovieTitle.ToLower()))
                {
                    errorList.Add("Ошибка. Фильм. Заданного фильма нет в базе.");
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
                CinemaSession item = await dataContext.CinemaSessions.Where(x => x.Id == id).FirstAsync();
                await Task.Run(() =>
                {
                    dataContext.CinemaSessions.Remove(item);
                    dataContext.SaveChanges();
                });
            }
            else LoadErrors();
            return await UpdatePage(page);


            async Task<bool> CheckErrors(int id)
            {
                if (!await dataContext.CinemaSessions.AnyAsync(x => x.Id == id))
                {
                    ModelState.AddModelError("", "Ошибка. Ошибка идентификации данных.");
                    return false;
                }
                return true;
            }
        }

        CinemaSessionsDataList GetDataList(List<CinemaSession> listItems, int page)
        {
            int totalCount = listItems.Count();
            int totalPages = (totalCount != 0) ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            if (page < 1) page = 1;
            else if (page > totalPages) page = totalPages;
            List<CinemaSession> listItemsPage = listItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            Pagination pagination = new Pagination()
            {
                PageNumber = page,
                TotalPages = totalPages,
                Action = Url.Action("NewPage", "CinemaSession", new { page = "x" }),
                UpdateTagId = "dataList-box"
            };
            return new CinemaSessionsDataList()
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

        List<CinemaSession> Filtration(List<CinemaSession> listItems, CinemaSessionsFilter filter)
        {
            HttpContext.Session.Set("CinemaSessionsFilter", filter);
            List<CinemaSession> filtrationResult = listItems.Where((x) => FilterTest(x, filter)).ToList();
            filtrationResult = Sorting(filtrationResult, filter.SortingCriterion);
            return filtrationResult;
        }

        List<CinemaSession> Sorting(List<CinemaSession> listItems, string sortingCriterion)
        {
            if (sortingCriterion == "price")
            {
                listItems.Sort((x, y) => x.Price > y.Price ? 1 : x.Price == y.Price ? 0 : -1);
            }
            else if (sortingCriterion == "dateTime")
            {
                listItems.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime) > 0 ? 1 : DateTime.Compare(x.DateTime, y.DateTime) == 0 ? 0 : -1);
            }
            return listItems;
        }

        bool FilterTest (CinemaSession item, CinemaSessionsFilter filter)
        {
            if (filter.CinemaName != null && !item.Cinema.Name.ToLower().Contains(filter.CinemaName.ToLower())) return false;
            if (filter.MovieTitle != null && !item.Movie.Title.ToLower().Contains(filter.MovieTitle.ToLower())) return false;
            if (filter.PriceMin >= 0 && filter.PriceMax > 0)
            {
                if (item.Price < filter.PriceMin || item.Price > filter.PriceMax) return false;
            }
            else if (filter.PriceMax > 0)
            {
                if (item.Price > filter.PriceMax) return false;
            }
            else if (filter.PriceMin > 0)
            {
                if (item.Price < filter.PriceMin) return false;
            }
            if (filter.DateTimeMin != null && filter.DateTimeMax != null)
            {
                DateTime dateTimeMin = DateTime.Parse(filter.DateTimeMin);
                DateTime dateTimeMax = DateTime.Parse(filter.DateTimeMax);
                if (DateTime.Compare(item.DateTime, dateTimeMin) < 0 || DateTime.Compare(item.DateTime, dateTimeMax) > 0) return false;
            }
            else if (filter.DateTimeMax != null)
            {
                DateTime dateTimeMax = DateTime.Parse(filter.DateTimeMax);
                if (DateTime.Compare(item.DateTime, dateTimeMax) > 0) return false;
            }
            else if (filter.DateTimeMin != null)
            {
                DateTime dateTimeMin = DateTime.Parse(filter.DateTimeMin);
                if (DateTime.Compare(item.DateTime, dateTimeMin) < 0) return false;
            }
            return true;
        }
    }
}
