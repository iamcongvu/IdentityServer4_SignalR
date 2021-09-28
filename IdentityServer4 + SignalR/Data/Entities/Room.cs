using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Data.Entities
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public User Admin { get; set; } // admin tạo ra room này

        public ICollection<Message> Messages { get; set; }
    }
}