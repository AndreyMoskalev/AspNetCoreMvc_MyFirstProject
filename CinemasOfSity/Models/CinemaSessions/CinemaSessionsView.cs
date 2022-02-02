namespace CinemasOfSity.Models.CinemaSessions
{
    public class CinemaSessionsView
    {
        public CinemaSessionsFilter Filter { get; set; }
        public CinemaSessionsDataList DataList { get; set; }
        public AddNewItem.AddNewItem AddNewItem { get; set; }
    }
}
