using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.ViewModels
{
    public class UploadVm
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}