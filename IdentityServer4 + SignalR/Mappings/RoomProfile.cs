using AutoMapper;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Mappings
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomVm>();
            CreateMap<RoomVm, Room>();
        }
    }
}