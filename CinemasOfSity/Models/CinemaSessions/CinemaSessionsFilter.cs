using System.Collections.Generic;

namespace CinemasOfSity.Models.CinemaSessions
{
    public class CinemaSessionsFilter
    {
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string SortingCriterion { get; set; }
        public string DateTimeMin { get; set; }
        public string DateTimeMax { get; set; }
        public double PriceMin { get; set; }
        public double PriceMax { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Formates { get; set; }
    }
}
