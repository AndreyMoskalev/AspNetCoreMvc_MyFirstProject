using CinemasOfSity.Models;
using CinemasOfSity.Models.Users;
using CinemasOfSity.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace CinemasOfSity.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class UsersController : Controller
    {
        int pageSize = 2;
        DataContext dataContext;

        public UsersController(DataContext context)
        {
            dataContext = context;
        }

        async Task LoadPageData()
        {
            ViewBag.ControllerName = "Users";
            ViewBag.Roles = await dataContext.Roles.Where(x => x.Name != "Администратор").Select(x => x.Name).ToListAsync();
            ViewBag.UsersFIO = await dataContext.Users.Select(x => x.FIO).ToListAsync();
            ViewBag.UserLogins = await dataContext.Users.Select(x => x.Login).ToListAsync();
            if (User.Identity.Name != null)
            {
                User user = await dataContext.Users.Include(x => x.Role).Where(x => x.Login == User.Identity.Name).FirstAsync();
                ViewBag.UserRole = user.Role.Name;
                ViewBag.UserLogin = user.Login;
            }
        }

        public async Task<IActionResult> Index(bool partialView = false)
        {
            UsersView viewModel = new UsersView();
            List<User> listItems = await dataContext.Users.Include(x => x.Role).ToListAsync();
            listItems.Reverse();
            if (HttpContext.Session.Get<UsersFilter>("UsersFilter") != null)
            {
                viewModel.Filter = HttpContext.Session.Get<UsersFilter>("UsersFilter");
                listItems = Filtration(listItems, viewModel.Filter);
            }
            else viewModel.Filter = new UsersFilter();
            viewModel.DataList = GetDataList(listItems, 1);
            viewModel.AddNewItem = new Models.AddNewItem.AddNewItem()
            {
                Title = "Добавить пользователя",
                Action = Url.Action("Add", "Users"),
                UpdateTagId = "updatedSectionsPage",
                AccessRoles = new List<string>()
                    {
                        "Администратор"
                    }
            };
            await LoadPageData();
            if (partialView) return PartialView(viewModel);
            else return View(viewModel);
        }

        async Task<IActionResult> UpdatePage(int page = 1)
        {
            UpdatedSectionsPage viewModel = new UpdatedSectionsPage();
            List<User> listItems = await dataContext.Users.Include(x => x.Role).ToListAsync();
            listItems.Reverse();
            if (HttpContext.Session.Get<UsersFilter>("UsersFilter") != null)
            {
                viewModel.Filter = HttpContext.Session.Get<UsersFilter>("UsersFilter");
                listItems = Filtration(listItems, viewModel.Filter);
            }
            else viewModel.Filter = new UsersFilter();
            viewModel.DataList = GetDataList(listItems, page);
            await LoadPageData();
            return PartialView("UpdatedSectionsPage", viewModel);
        }

        public async Task<IActionResult> NewPage(int page = 1)
        {
            UsersDataList dataList = new UsersDataList();
            List<User> listItems = await dataContext.Users.Include(x => x.Role).ToListAsync();
            listItems.Reverse();
            if (HttpContext.Session.Get<UsersFilter>("UsersFilter") != null)
            {
                listItems = Filtration(listItems, HttpContext.Session.Get<UsersFilter>("UsersFilter"));
            }
            dataList = GetDataList(listItems, page);
            await LoadPageData();
            return PartialView("UsersDataList", dataList);
        }

        public async Task<IActionResult> Filter(UsersFilter filter)
        {
            UsersDataList dataList = new UsersDataList();
            List<User> listItems = await dataContext.Users.Include(x => x.Role).ToListAsync();
            listItems.Reverse();
            listItems = Filtration(listItems, filter);
            dataList = GetDataList(listItems, 1);
            await LoadPageData();
            return PartialView("UsersDataList", dataList);
        }

        public async Task<IActionResult> Add(UserResult addItem)
        {
            if (ModelState.IsValid && await CheckErrors(addItem))
            {
                User item = new User();
                item.Role = await dataContext.Roles.Where(x => x.Name == addItem.Role).FirstAsync();
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
                await dataContext.Users.AddAsync(item);
                await dataContext.SaveChangesAsync();
            }
            else LoadErrors();
            return await UpdatePage(1);


            async Task<bool> CheckErrors(UserResult addItem)
            {
                List<string> errorList = new List<string>();
                if (await dataContext.Users.AnyAsync(x => x.Login == addItem.Login))
                {
                    errorList.Add("Ошибка. Логин. Пользователь с таким логином уже существует.");
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

        public async Task<IActionResult> Update(UserResult updateItem, int page)
        {
            if (ModelState.IsValid && await CheckErrors(updateItem))
            {
                User item = await dataContext.Users.Where(x => x.Id == updateItem.Id).Include(x => x.Role).FirstAsync();
                Role role = await dataContext.Roles.Where(x => x.Name == updateItem.Role).FirstAsync();
                item.Role = role;
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
                await dataContext.SaveChangesAsync();
            }
            else LoadErrors();
            return await UpdatePage(page);


            async Task<bool> CheckErrors(UserResult updateItem)
            {
                List<string> errorList = new List<string>();
                if(!await dataContext.Users.AnyAsync(x => x.Id == updateItem.Id))
                {
                    errorList.Add("Ошибка идентификации данных.");
                }
                if (await dataContext.Users.AnyAsync(x => x.Login == updateItem.Login && updateItem.Id != x.Id))
                {
                    errorList.Add("Ошибка. Логин. Пользователь с таким логином уже существует.");
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

        public async Task<IActionResult> Delete(int id, int page)
        {
            if (await CheckErrors(id))
            {
                User item = await dataContext.Users.Where(x => x.Id == id).FirstAsync();
                dataContext.Users.Remove(item);
                await dataContext.SaveChangesAsync();
            }
            else LoadErrors();
            return await UpdatePage(page);


            async Task<bool> CheckErrors(int id)
            {
                if (!await dataContext.Users.AnyAsync(x => x.Id == id))
                {
                    ModelState.AddModelError("", "Ошибка идентификации данных.");
                    return false;
                }
                return true;
            }
        }

        UsersDataList GetDataList(List<User> listItems, int page)
        {
            int totalCount = listItems.Count();
            int totalPages = (totalCount != 0) ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            if (page < 1) page = 1;
            else if (page > totalPages) page = totalPages;
            List<User> listItemsPage = listItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            Pagination pagination = new Pagination()
            {
                PageNumber = page,
                TotalPages = totalPages,
                Action = Url.Action("NewPage", "Users", new { page = "x" }),
                UpdateTagId = "dataList-box"
            };
            return new UsersDataList()
            {
                Data = listItemsPage,
                Pagination = pagination,
                UpdateTagId = "updatedSectionsPage",
                AccessRoles = new List<string>()
                {
                    "Администратор"
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

        List<User> Filtration(List<User> listItems, UsersFilter filter)
        {
            HttpContext.Session.Set("UsersFilter", filter);
            List<User> filtrationResult = listItems.Where((x) => FilterTest(x, filter)).ToList();
            return filtrationResult;
        }

        bool FilterTest(User item, UsersFilter filter)
        {
            if (filter.FIO != null && !item.FIO.ToLower().Contains(filter.FIO.ToLower())) return false;
            if (filter.Login != null && !item.Login.ToLower().Contains(filter.Login.ToLower())) return false;
            return true;
        }
    }
}