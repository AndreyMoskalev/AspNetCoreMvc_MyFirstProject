using System.Collections.Generic;

namespace CinemasOfSity.Models.AddNewItem
{
    public class AddNewItem
    {
        public string Title { get; set; }
        public string Action { get; set; }
        public string UpdateTagId { get; set; }
        public List<NewItemsRow> NewItemsRows { get; set; }
        public List<string> AccessRoles { get; set; }
    }
}