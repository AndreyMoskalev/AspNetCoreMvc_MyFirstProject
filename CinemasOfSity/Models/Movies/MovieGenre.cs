using System.Collections.Generic;

namespace CinemasOfSity.Models.Movies
{
    public class MovieGenre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Movie> Movie { get; set; }
    }
}
