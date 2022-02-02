using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CinemasOfSity.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace CinemasOfSity.Controllers
{
    public class AccountController : Controller
    {

        Models.DataContext dataContext;

        public AccountController(Models.DataContext context)
        {
            dataContext = context;
        }

        [Authorize]
        public new async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();
            string javaScriptResult = "location.reload(true)";
            return PartialView("JavaScriptResult", javaScriptResult);
        }

        public async Task<IActionResult> Authorization(Authorization authorization)
        {
            if (ModelState.IsValid)
            {
                User? user = await dataContext.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Login == authorization.Login && x.Password == authorization.Password);
                if (user != null)
                {
                    await Authenticate(user);
                    string javaScriptResult = "location.reload(true)";
                    return PartialView("JavaScriptResult", javaScriptResult);
                }
                ModelState.AddModelError("", "Неверный логин и(или) пароль");
            }
            return PartialView("Account/Authorization", authorization);
        }

        async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}

