namespace CinemasOfSity.Models.Cinemas
{
    public class CinemasView
    {
        public CinemasFilter Filter { get; set; }
        public CinemasDataList DataList { get; set; }
        public AddNewItem.AddNewItem AddNewItem { get; set; }
    }
}