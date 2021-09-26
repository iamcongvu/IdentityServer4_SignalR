using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.ViewModels
{
    public class RoomVm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [RegularExpression(@"^\w+( \w+)*$", ErrorMessage = "Characters allowed: letters, numbers and one space between word.")]
        public string Name { get; set; }
    }
}