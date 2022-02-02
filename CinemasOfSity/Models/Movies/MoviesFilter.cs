using System.Collections.Generic;

namespace CinemasOfSity.Models.Movies
{
    public class MoviesFilter
    {
        public string MovieTitle { get; set; }
        public string SortingCriterion { get; set; }
        public string DurationMin { get; set; }
        public string DurationMax { get; set; }
        public int AgeLimitMin { get; set; }
        public int AgeLimitMax { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Countries { get; set; }
        public List<string> Formats { get; set; }
        public List<string> Directors { get; set; }
    }
}
