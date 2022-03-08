using CinemasOfSity.Models.Account;
using System.Collections.Generic;
using System.Linq;

namespace CinemasOfSity
{
    public static class CreateDefaultUsers
    {
        static List<Role> Roles = new List<Role>()
        {
            new Role()
            {
                Name="Администратор",
                Users = new List<User>()
                {
                    new User()
                    {
                        Login = "Administrator",
                        Password = "12345",
                        Address = "Адрес администратора",
                        Email = "Email администратора",
                        FIO = "Администратор Администратович",
                        Telephone ="Телефон администратора"
                    }
                }
            },
            new Role()
            {
                Name="Оператор",
                Users = new List<User>()
                {
                    new User()
                    {
                        Login = "Operator1",
                        Password = "12345",
                        Address = "Адрес 1-го оператора",
                        Email = "Email 1-го оператора",
                        FIO = "Первый оператор",
                        Telephone ="Телефон 1-го оператора"
                    },
                    new User()
                    {
                        Login = "Operator2",
                        Password = "12345",
                        Address = "Адрес 2-го оператора",
                        Email = "Email 2-го оператора",
                        FIO = "Второй оператор",
                        Telephone = "Телефон 2-го оператора"
                    },
                    new User()
                    {
                        Login = "Operator3",
                        Password = "12345",
                        Address = "Адрес 3-го оператора",
                        Email = "Email 3-го оператора",
                        FIO = "Третий оператор",
                        Telephone = "Телефон 3-го оператора"
                    }
                }
            }
        };

        public static void Initialize(Models.DataContext context)
        {
            foreach (Role role in Roles)
            {
                if (!context.Roles.Any(x=>x.Name == role.Name))
                {
                    context.Roles.Add(role);
                    context.SaveChanges();
                }
                foreach (User user in role.Users)
                {
                    if (!context.Users.Any(x => x.Login == user.Login))
                    {
                        context.Users.Add(user);
                    }
                }
                context.SaveChanges();
            }
        }

    }
}
