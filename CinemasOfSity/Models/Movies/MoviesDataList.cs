using System.Collections.Generic;

namespace CinemasOfSity.Models.Movies
{
    public class MoviesDataList
    {
        public List<Movie> Data { get; set; }
        public Pagination Pagination { get; set; }
        public string UpdateTagId { get; set; }
        public List<string> AccessRoles { get; set; }
    }
}
