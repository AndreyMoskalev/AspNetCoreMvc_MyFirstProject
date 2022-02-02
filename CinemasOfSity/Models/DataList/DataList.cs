using System.Collections.Generic;

namespace CinemasOfSity.Models.DataList
{
    public class DataList
    {
        public Pagination Page { get; set; }
        public List<DataBox> DataBoxes { get; set; }
        public string UpdateTagId { get; set; }
        public List<string> AccessRoles { get; set; }
    }
}
