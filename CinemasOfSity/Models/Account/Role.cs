﻿using System.Collections.Generic;

namespace CinemasOfSity.Models.Account
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
    }
}
