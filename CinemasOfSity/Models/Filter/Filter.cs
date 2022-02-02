using System.Collections.Generic;

namespace CinemasOfSity.Models.Filter
{
    public class Filter
    {
        public string Title { get; set; }
        public List<FilterRow> FilterSectionRows { get; set; }
        public string Action { get; set; }
        public string UpdateTagId { get; set; }
    }
}
