using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemasOfSity.Models.Movies
{
    public class MovieResult : IValidatableObject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genres { get; set; }
        public string Format { get; set; }
        public string Countries { get; set; }
        public string Director { get; set; }
        public int AgeLimit { get; set; }
        public string? Duration { get; set; }
        public string? Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            double maxIntValue = 2.1 * Math.Pow(10, 9);
            List<ValidationResult> errors = new List<ValidationResult>();

            if (Id < 0 || Id > maxIntValue) errors.Add(new ValidationResult("Ошибка. Ошибка идентификации данных."));

            if (string.IsNullOrWhiteSpace(Title)) errors.Add(new ValidationResult("Ошибка. Название фильма. Нет значения."));
            else if (Title.Length > 500) errors.Add(new ValidationResult("Ошибка. Название фильма. Превышено максимальное количество символов - 500."));
            
            if (string.IsNullOrWhiteSpace(Genres)) errors.Add(new ValidationResult("Ошибка. Жанры фильма. Нет значения."));
            else if (Genres.Length > 500) errors.Add(new ValidationResult("Ошибка. Жанры фильма. Превышено максимальное количество символов - 500."));

            if (string.IsNullOrWhiteSpace(Format)) errors.Add(new ValidationResult("Ошибка. Формат фильма. Нет значения."));
            else if (Format.Length > 500) errors.Add(new ValidationResult("Ошибка. Формат фильма. Превышено максимальное количество символов - 500."));
            
            if (string.IsNullOrWhiteSpace(Countries)) errors.Add(new ValidationResult("Ошибка. Страны фильма. Нет значения."));
            else if (Countries.Length > 500) errors.Add(new ValidationResult("Ошибка. Страны производства фильма. Превышено максимальное количество символов - 500."));
            
            if (string.IsNullOrWhiteSpace(Director)) errors.Add(new ValidationResult("Ошибка. Режиссер фильма. Нет значения."));
            else if (Director.Length > 500) errors.Add(new ValidationResult("Ошибка. Режиссер фильма. Превышено максимальное количество символов - 500."));

            if (AgeLimit < 0 || AgeLimit > 18) errors.Add(new ValidationResult("Ошибка. Возрастное ограничение. Значение должно быть в интервале от 0 до 18."));
            
            if (string.IsNullOrWhiteSpace(Duration)) Duration = null;
            else if (!TimeSpan.TryParse(Duration, out TimeSpan duration)) errors.Add(new ValidationResult("Продолжительность фильма задана некорректно."));

            if (string.IsNullOrWhiteSpace(Description)) Description = null;
            else if (Description.Length > 5000) errors.Add(new ValidationResult("Ошибка. Описание фильма. Превышено максимальное количество символов - 5000."));

            return errors;
        }
    }
}
