using System.Collections.Generic;

namespace CinemasOfSity.Models.DataList
{
    public class DataBox
    {
        public string IdDataBox { get; set; }
        public string IdItem { get; set; }
        public DataBoxButton ButtonAdditionalFirst { get; set; }
        public DataBoxButton ButtonAdditionalTwo { get; set; }
        public DataBoxButton ButtonToUpdate { get; set; }
        public DataBoxButton ButtonToDelete { get; set; }
        public List<List<TitleContentPair>> TitleContentRows { get; set; }
        public string UpdateTagId { get; set; }
        public int PageNumber { get; set; }
    }
}
