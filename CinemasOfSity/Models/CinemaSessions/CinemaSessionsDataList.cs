using System.Collections.Generic;

namespace CinemasOfSity.Models.CinemaSessions
{
    public class CinemaSessionsDataList
    {
        public List<CinemaSession> Data { get; set; }
        public Pagination Pagination { get; set; }
        public string UpdateTagId { get; set; }
        public List<string> AccessRoles { get; set; }
    }
}
