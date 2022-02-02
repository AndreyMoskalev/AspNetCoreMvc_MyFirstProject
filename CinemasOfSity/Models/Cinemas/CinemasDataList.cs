using System.Collections.Generic;

namespace CinemasOfSity.Models.Cinemas
{
    public class CinemasDataList
    {
        public List<Cinema> Data { get; set; }
        public Pagination Pagination { get; set; }
        public string UpdateTagId { get; set; }
        public List<string> AccessRoles { get; set; }
    }
}
