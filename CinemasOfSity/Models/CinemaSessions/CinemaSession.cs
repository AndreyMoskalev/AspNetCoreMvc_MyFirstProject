using System;
using CinemasOfSity.Models.Cinemas;
using CinemasOfSity.Models.Movies;

namespace CinemasOfSity.Models.CinemaSessions
{
    public class CinemaSession
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public int NumberOfTickets { get; set; }
        public DateTime DateTime { get; set; }
        public Movie Movie { get; set; }
        public Cinema Cinema { get; set; }
    }
}