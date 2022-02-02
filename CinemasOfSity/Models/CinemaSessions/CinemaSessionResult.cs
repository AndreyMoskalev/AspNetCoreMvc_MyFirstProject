using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemasOfSity.Models.CinemaSessions
{
    public class CinemaSessionResult : IValidatableObject
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public int NumberOfTickets { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string DateTime { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            double maxIntValue = 2.1 * Math.Pow(10, 9);
            List<ValidationResult> errors = new List<ValidationResult>();

            if (Id < 0 || Id > maxIntValue) errors.Add(new ValidationResult("Ошибка идентификации данных."));

            if (Price < 0) errors.Add(new ValidationResult("Ошибка. Цена билета. Цена билета должна быть больше или равна 0."));
            else if (Price > maxIntValue) errors.Add(new ValidationResult("Ошибка. Цена билета. Цена билета задана некорректно."));

            if (NumberOfTickets < 1) errors.Add(new ValidationResult("Ошибка. Количество билетов. Количество билетов должно быть больше или равно 1."));
            else if(NumberOfTickets > maxIntValue) errors.Add(new ValidationResult("Ошибка. Количество билетов. Количество билетов задано некорректно."));

            if (string.IsNullOrWhiteSpace(MovieTitle)) errors.Add(new ValidationResult("Ошибка. Название фильма. Нет значения."));

            if (string.IsNullOrWhiteSpace(CinemaName)) errors.Add(new ValidationResult("Ошибка. Название кинотеатра. Нет значения."));

            if (Date == null && Time == null)
            {
                if (string.IsNullOrWhiteSpace(DateTime)) errors.Add(new ValidationResult("Ошибка. Дата и время начала сеанса. Нет значения."));
                else if (!System.DateTime.TryParse(DateTime, out DateTime dateTime)) errors.Add(new ValidationResult("Ошибка. Дата и время начала сеанса. Дата и время сеанса заданы некорректно."));
            }
            else if (DateTime == null)
            {
                if (string.IsNullOrWhiteSpace(Date)) errors.Add(new ValidationResult("Ошибка. Дата начала сеанса. Нет значения."));
                else if (!System.DateTime.TryParse(Date, out DateTime date)) errors.Add(new ValidationResult("Ошибка. Дата начала сеанса. Дата начала сеанса задана некорректно."));

                if (string.IsNullOrWhiteSpace(Time)) errors.Add(new ValidationResult("Ошибка. Время начала сеанса. Нет значения."));
                else if (!TimeSpan.TryParse(Time, out TimeSpan time)) errors.Add(new ValidationResult("Ошибка. Время начала сеанса. Время начала сеанса задано некорректно."));
            }

            return errors;
        }
    }
}
