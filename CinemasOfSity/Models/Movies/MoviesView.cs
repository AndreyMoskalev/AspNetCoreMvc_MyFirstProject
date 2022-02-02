namespace CinemasOfSity.Models.Movies
{
    public class MoviesView
    {
        public MoviesFilter Filter { get; set; }
        public MoviesDataList DataList { get; set; }
        public AddNewItem.AddNewItem AddNewItem { get; set; }
    }
}
