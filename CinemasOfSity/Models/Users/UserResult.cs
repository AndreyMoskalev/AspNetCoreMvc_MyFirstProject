using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemasOfSity.Models.Users
{
    public class UserResult : IValidatableObject
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            double maxIntValue = 2.1 * Math.Pow(10, 9);
            List<ValidationResult> errors = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Login)) errors.Add(new ValidationResult("Ошибка. Логин. Нет значения."));
            else if (Login.Length > 20) errors.Add(new ValidationResult("Ошибка. Логин. Превышено максимальное количество символов - 20."));

            if (string.IsNullOrWhiteSpace(Password)) errors.Add(new ValidationResult("Ошибка. Пароль. Нет значения."));
            else if (Password.Length > 30) errors.Add(new ValidationResult("Ошибка. Жанры фильма. Превышено максимальное количество символов - 30."));

            if (string.IsNullOrWhiteSpace(FIO)) errors.Add(new ValidationResult("Ошибка. ФИО. Нет значения."));
            else if (Password.Length > 500) errors.Add(new ValidationResult("Ошибка. ФИО. Превышено максимальное количество символов - 500."));

            if (string.IsNullOrWhiteSpace(Role)) errors.Add(new ValidationResult("Ошибка. Роль. Нет значения."));
            else if (Role.Length > 500) errors.Add(new ValidationResult("Ошибка. Роль. Превышено максимальное количество символов - 500."));

            if (Telephone.Length > 500) errors.Add(new ValidationResult("Ошибка. Телефон. Превышено максимальное количество символов - 500."));

            if (Email.Length > 500) errors.Add(new ValidationResult("Ошибка. Емейл. Превышено максимальное количество символов - 500."));

            if (Address.Length > 500) errors.Add(new ValidationResult("Ошибка. Адрес. Превышено максимальное количество символов - 500."));

            return errors;
        }
    }
}
