using CinemasOfSity.Models;
using CinemasOfSity.Models.Cinemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CinemasOfSity.Controllers
{
    public class CinemasController : Controller
    {
        int pageSize = 2;
        DataContext dataContext;

        public CinemasController(DataContext context)
        {
            dataContext = context;
        }

        async Task LoadPageData()
        {
            await Task.Run(() =>
            {
                ViewBag.ControllerName = "Cinemas";
                ViewBag.SortingCriteria = GetSortingCriteria();
                ViewBag.CinemaNames = dataContext.Cinemas.Select(x => x.Name).Distinct().ToList();
                if (User.Identity.Name != null)
                {
                    Models.Account.User user = dataContext.Users.Include(x => x.Role).Where(x => x.Login == User.Identity.Name).First();
                    ViewBag.UserRole = user.Role.Name;
                    ViewBag.UserLogin = user.Login;
                }
            });
        }

        IDictionary<string, string> GetSortingCriteria()
        {
            return new Dictionary<string, string>()
            {
                { "capacity", "Сортировать по вместимости" },
                { "", "Без сортировки" }
            };
        }

        public async Task<IActionResult> Index(bool partialView = false, CinemasFilter filter = null)
        {
            CinemasView viewModel = new CinemasView();
            List<Cinema> listItems = await dataContext.Cinemas.ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                if (filter != null)
                {
                    viewModel.Filter = filter;
                    listItems = Filtration(listItems, viewModel.Filter);
                }
                else if (HttpContext.Session.Get<CinemasFilter>("CinemasFilter") != null)
                {
                    viewModel.Filter = HttpContext.Session.Get<CinemasFilter>("CinemasFilter");
                    listItems = Filtration(listItems, viewModel.Filter);
                }
                else viewModel.Filter = new CinemasFilter();
                viewModel.DataList = GetDataList(listItems, 1);
                viewModel.AddNewItem = new Models.AddNewItem.AddNewItem()
                {
                    Title = "Добавить кинотеатр",
                    Action = Url.Action("Add", "Cinemas"),
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
            List<Cinema> listItems = await dataContext.Cinemas.ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                if (HttpContext.Session.Get<CinemasFilter>("CinemasFilter") != null)
                {
                    viewModel.Filter = HttpContext.Session.Get<CinemasFilter>("CinemasFilter");
                    listItems = Filtration(listItems, viewModel.Filter);
                }
                else viewModel.Filter = new CinemasFilter();
                viewModel.DataList = GetDataList(listItems, page);
            });
            await LoadPageData();
            return PartialView("UpdatedSectionsPage", viewModel);
        }

        public async Task<IActionResult> NewPage(int page = 1)
        {
            CinemasDataList dataList = new CinemasDataList();
            List<Cinema> listItems = await dataContext.Cinemas.ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                if (HttpContext.Session.Get<CinemasFilter>("CinemasFilter") != null)
                {
                    listItems = Filtration(listItems, HttpContext.Session.Get<CinemasFilter>("CinemasFilter"));
                }
                dataList = GetDataList(listItems, page);
            });
            await LoadPageData();
            return PartialView("CinemasDataList", dataList);
        }

        public async Task<IActionResult> Filter(CinemasFilter filter)
        {
            CinemasDataList dataList = new CinemasDataList();
            List<Cinema> listItems = await dataContext.Cinemas.ToListAsync();
            await Task.Run(() =>
            {
                listItems.Reverse();
                listItems = Filtration(listItems, filter);
                dataList = GetDataList(listItems, 1);
            });
            await LoadPageData();
            return PartialView("CinemasDataList", dataList);
        }

        [Authorize(Roles = "Администратор, Оператор")]
        public async Task<IActionResult> Add(CinemaResult addItem)
        {
            if (ModelState.IsValid && await CheckErrors(addItem))
            {
                Cinema item = new Cinema();
                await Task.Run(() =>
                {
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
                    dataContext.Cinemas.Add(item);
                    dataContext.SaveChanges();
                });
            }
            else LoadErrors();
            return await UpdatePage(1);

            async Task<bool> CheckErrors(CinemaResult addItem)
            {
                List<string> errorList = new List<string>();
                if (await dataContext.Cinemas.AnyAsync(x => x.Name.ToLower() == addItem.Name.ToLower()))
                {
                    errorList.Add("Ошибка. Названеи кинотеатра. Кинотеатр с таким названием уже есть в базе.");
                }
                if (errorList.Count > 0)
                {
                    foreach (string error in errorList)
                    {
                        ModelState.AddModelError(null, error);
                    }
                    return false;
                }
                return true;
            }
        }

        [Authorize(Roles = "Администратор, Оператор")]
        public async Task<IActionResult> Update(CinemaResult updateItem, int page)
        {
            if (ModelState.IsValid && await CheckErrors(updateItem))
            {
                Cinema item = await dataContext.Cinemas.Where(x => x.Id == updateItem.Id).FirstAsync();
                await Task.Run(() =>
                {
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

            async Task<bool> CheckErrors(CinemaResult updateItem)
            {
                List<string> errorList = new List<string>();
                if (!await dataContext.Cinemas.AnyAsync(x => x.Id == updateItem.Id))
                {
                    errorList.Add("Ошибка идентификации данных.");
                }
                if (await dataContext.Cinemas.AnyAsync(x => x.Name.ToLower() == updateItem.Name.ToLower() && x.Id != updateItem.Id))
                {
                    errorList.Add("Ошибка. Названеи кинотеатра. Кинотеатр с таким названием уже есть в базе.");
                }
                if (errorList.Count > 0)
                {
                    foreach (string error in errorList)
                    {
                        ModelState.AddModelError(null, error);
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
                Cinema item = await dataContext.Cinemas.Where(x => x.Id == id).FirstAsync();
                List<Models.CinemaSessions.CinemaSession> sessions = await dataContext.CinemaSessions.Where(x => x.Cinema.Id == item.Id).ToListAsync();
                await Task.Run(() =>
                {
                    foreach (Models.CinemaSessions.CinemaSession session in sessions) dataContext.CinemaSessions.Remove(session);
                    dataContext.Cinemas.Remove(item);
                    dataContext.SaveChanges();
                });
            }
            else LoadErrors();
            return await UpdatePage(page);


            async Task<bool> CheckErrors(int id)
            {
                if (!await dataContext.Cinemas.AnyAsync(x => x.Id == id))
                {
                    ModelState.AddModelError(null, "Ошибка идентификации данных.");
                    return false;
                }
                return true;
            }
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

        CinemasDataList GetDataList(List<Cinema> listItems, int page)
        {
            int totalCount = listItems.Count();
            int totalPages = (totalCount != 0) ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            if (page < 1) page = 1;
            else if (page > totalPages) page = totalPages;
            List<Cinema> listItemsPage = listItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            Pagination pagination = new Pagination()
            {
                PageNumber = page,
                TotalPages = totalPages,
                Action = Url.Action("NewPage", "Cinemas", new { page = "x" }),
                UpdateTagId = "dataList-box"
            };
            return new CinemasDataList()
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

        List<Cinema> Filtration(List<Cinema> listItems, CinemasFilter filter)
        {
            HttpContext.Session.Set("CinemasFilter", filter);
            List<Cinema> filtrationResult = listItems.Where((x) => FilterTest(x, filter)).ToList();
            filtrationResult = Sorting(filtrationResult, filter.SortingCriterion);
            return filtrationResult;
        }

        List<Cinema> Sorting(List<Cinema> listItems, string sortingCriterion)
        {
            if (sortingCriterion == "capacity")
            {
                listItems.Sort((x, y) => x.Capacity < y.Capacity ? 1 : x.Capacity == y.Capacity ? 1 : -1);
            }
            return listItems;
        }

        bool FilterTest(Cinema item, CinemasFilter filter)
        {
            if (filter.CinemaName != null && !item.Name.ToLower().Contains(filter.CinemaName.ToLower())) return false;
            return true;
        }
    }
}
