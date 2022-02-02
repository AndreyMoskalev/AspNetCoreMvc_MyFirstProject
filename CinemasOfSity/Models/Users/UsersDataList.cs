using System.Collections.Generic;

namespace CinemasOfSity.Models.Users
{
    public class UsersDataList
    {
        public List<Account.User> Data { get; set; }
        public Pagination Pagination { get; set; }
        public string UpdateTagId { get; set; }
        public List<string> AccessRoles { get; set; }
    }
}
