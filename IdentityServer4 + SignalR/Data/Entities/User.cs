using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Data.Entities
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; }

        public DateTime Birthday { get; set; }
    }
}