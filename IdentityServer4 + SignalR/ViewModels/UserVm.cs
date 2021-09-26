using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.ViewModels
{
    public class UserVm
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }
    }
}