namespace CinemasOfSity.Models
{
    public class Pagination
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string UpdateTagId { get; set; }
        public string Action { get; set; }

        public bool HasPreviousPage
        {
            get
            {
                return PageNumber > 1;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return PageNumber < TotalPages;
            }
        }
    }
}
