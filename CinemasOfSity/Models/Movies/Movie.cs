using System.Collections.Generic;
using System;

namespace CinemasOfSity.Models.Movies
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Format { get; set; }
        public string Director { get; set; }
        public string Description { get; set; }
        public int AgeLimit { get; set; }
        public TimeSpan Duration { get; set; }
        public List<MovieGenre> Genre { get; set; }
        public List<MovieCountry> Country { get; set; }
    }
}
