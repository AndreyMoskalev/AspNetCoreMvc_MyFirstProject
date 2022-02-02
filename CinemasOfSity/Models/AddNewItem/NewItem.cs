using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemasOfSity.Models.AddNewItem
{
    public class NewItem
    {
        public string Title { get; set; }
        public TagBuilder InputBuilder { get; set; }
    }
}
