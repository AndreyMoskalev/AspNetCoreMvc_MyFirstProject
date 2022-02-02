using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemasOfSity.Models.Cinemas
{
    public class CinemaResult : IValidatableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string? Telephone { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public int? Capacity { get; set; }
        public int? NumberOfHalls { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            double maxIntValue = 2.1 * System.Math.Pow(10, 9);
            List<ValidationResult> errors = new List<ValidationResult>();
            if (Id < 0 || Id > maxIntValue) errors.Add(new ValidationResult("Ошибка идентификации данных."));

            if (string.IsNullOrWhiteSpace(Name)) errors.Add(new ValidationResult("Ошибка. Название кинотеатра. Нет значения."));
            else if (Name.Length > 500) errors.Add(new ValidationResult("Ошибка. Название кинотеатра. Превышено максимальное количество символов - 500."));

            if (string.IsNullOrWhiteSpace(Telephone)) Telephone = "";
            else if (Telephone.Length > 500) errors.Add(new ValidationResult("Ошибка. Номер телефона. Превышено максимальное количество символов - 500."));

            if (string.IsNullOrWhiteSpace(Email)) Email = "";
            else if (Email.Length > 500) errors.Add(new ValidationResult("Ошибка. Email. Превышено максимальное количество символов - 500."));

            if (string.IsNullOrWhiteSpace(Address)) errors.Add(new ValidationResult("Ошибка. Адрес кинотеатра. Нет значения."));
            else if (Address.Length > 500) errors.Add(new ValidationResult("Ошибка. Адрес кинотеатра. Превышено максимальное количество символов - 500."));

            if (Capacity < 0) errors.Add(new ValidationResult("Ошибка. Вместимость кинотеатра. Общее количество мест не может быть отрицательным."));
            else if(Capacity > maxIntValue) errors.Add(new ValidationResult("Ошибка. Вместимость кинотеатра. Вместимость кинотеатра задана некорректно."));

            if (NumberOfHalls < 0) errors.Add(new ValidationResult("Ошибка. Количество залов кинотеатра. Количество залов кинотеатра не может быть отрицательным."));
            else if (NumberOfHalls > maxIntValue) errors.Add(new ValidationResult("Ошибка. Количество залов кинотеатра. Количество залов кинотеатра задано некорректно."));

            if (string.IsNullOrWhiteSpace(Description)) Description = "";
            else if (Description.Length > 5000) errors.Add(new ValidationResult("Ошибка. Описание кинотеатра. Превышено максимальное количество символов - 5000."));

            return errors;
        }
    }
}
