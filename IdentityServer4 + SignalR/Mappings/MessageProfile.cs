using AutoMapper;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.Helpers;
using IdentityServer4SignalR.ViewModels;

namespace IdentityServer4SignalR.Mappings
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageVm>()
                .ForMember(dst => dst.From, opt => opt.MapFrom(x => x.FromUser.FullName))
                .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.ToRoom.Name))
                .ForMember(dst => dst.Avatar, opt => opt.MapFrom(x => x.FromUser.Avartar))
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)))
                .ForMember(dst => dst.Timestamp, opt => opt.MapFrom(x => x.Timestamp));

            CreateMap<MessageVm, Message>();
        }
    }
}