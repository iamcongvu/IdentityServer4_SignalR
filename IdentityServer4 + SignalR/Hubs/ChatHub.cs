using AutoMapper;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public readonly static List<UserVm> _Connections = new List<UserVm>(); // danh sách những người kết nối
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>(); // tạo kết nối connectionId tới Hub khi client request -> signalR

        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;

        public ChatHub(ManageAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // send message cho user được chỉ định, chỉ người đó mới thấy tin nhắn
        public async Task SendPrivate(string receiverName, string message) // (tên người nhân, tin cần gửi)
        {
            if (_ConnectionsMap.TryGetValue(receiverName, out string userId)) // thử tìm xem người nhận có đăng nhập ko
            {
                // Who is the sender;
                var sender = _Connections.Where(u => u.UserName == IdentityName).First();

                if (!string.IsNullOrEmpty(message.Trim()))
                {
                    // Build the message
                    var messageViewModel = new MessageVm()
                    {
                        Content = Regex.Replace(message, @"<.*?>", string.Empty), // match bất kỳ kí tự nào
                        From = sender.FullName,
                        Avatar = sender.Avatar,
                        Room = "",
                        Timestamp = DateTime.Now.ToLongTimeString()
                    };

                    // Send the message
                    await Clients.Client(userId).SendAsync("newMessage", messageViewModel); // đây là cách gửi riêng cho 1 client nào đó
                    await Clients.Caller.SendAsync("newMessage", messageViewModel); // gửi tin này lên luôn cho chính "người gửi đi" để 2 người cx thấy
                }
            }
        }

        private string IdentityName
        {
            get { return Context.User.Identity.Name; }
        }

        public async Task Leave(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task Join(string roomName)
        {
            try
            {
                var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
                if (user != null && user.CurrentRoom != roomName)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(user.CurrentRoom))
                        await Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                    // Join to new chat room
                    await Leave(user.CurrentRoom);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName); // với mỗi group sẽ có 1 connectionId để kết nối tới
                    user.CurrentRoom = roomName;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(roomName).SendAsync("addUser", user);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
            }
        }

        // lần đầu login vào thì sẽ gọi hàm này
        public override Task OnConnectedAsync()
        {
            try
            {
                var user = _context.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                var userViewModel = _mapper.Map<User, UserVm>(user);
                userViewModel.Device = GetDevice();
                userViewModel.CurrentRoom = ""; // lúc này chưa tham gia room nào

                if (!_Connections.Any(u => u.UserName == IdentityName))
                {
                    _Connections.Add(userViewModel);
                    _ConnectionsMap.Add(IdentityName, Context.ConnectionId);
                }

                Clients.Caller.SendAsync("getProfileInfo", user.FullName, user.Avatar);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            return base.OnConnectedAsync();
        }

        // khi thoát hay tắt trình duyệt sẽ ở trạng thái ko hoạt động, hàm này sẽ được gọi và thông báo cho các clients khác biết mình ko hoạt động
        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = _Connections.Where(u => u.UserName == IdentityName).First();
                _Connections.Remove(user);

                // Tell other users to remove you from their list
                Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                // Remove mapping
                _ConnectionsMap.Remove(user.UserName);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnectedAsync(exception);
        }

        // lấy ra danh sách nhứng người đang hoạt động
        public IEnumerable<UserVm> GetUsers(string roomName)
        {
            return _Connections.Where(u => u.CurrentRoom == roomName).ToList();
        }

        private string GetDevice()
        {
            var device = Context.GetHttpContext().Request.Headers["Device"].ToString();
            if (!string.IsNullOrEmpty(device) && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }
    }
}