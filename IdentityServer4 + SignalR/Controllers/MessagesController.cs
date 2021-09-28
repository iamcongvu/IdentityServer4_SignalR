using AutoMapper;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.Hubs;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Controllers
{
    public class MessagesController : BaseController
    {
        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(ManageAppDbContext context, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return NotFound();

            var messageViewModel = _mapper.Map<Message, MessageVm>(message);

            return Ok(messageViewModel);
        }

        [HttpGet("Room/{roomName}")]
        public IActionResult GetMessages(string roomName)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);
            if (room == null)
                return BadRequest();

            var messages = _context.Messages.Where(m => m.ToRoomId == room.Id)
                .Include(m => m.FromUser)
                .Include(m => m.ToRoom)
                .OrderByDescending(m => m.Timestamp)
                .Take(20)
                .AsEnumerable()
                .Reverse()
                .ToList();

            var messagesViewModel = _mapper.Map<IEnumerable<Message>, IEnumerable<MessageVm>>(messages);

            return Ok(messagesViewModel);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(MessageVm messageVm)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var room = _context.Rooms.FirstOrDefault(r => r.Name == messageVm.Room);
            if (room == null)
                return BadRequest();

            var msg = new Message()
            {
                Content = Regex.Replace(messageVm.Content, @"<.*?>", string.Empty),
                FromUser = user,
                ToRoom = room,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Broadcast the message
            var createdMessage = _mapper.Map<Message, MessageVm>(msg);
            await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", createdMessage);

            return CreatedAtAction(nameof(GetAsync), new { id = msg.Id }, createdMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.Messages
                .Include(u => u.FromUser)
                .Where(m => m.Id == id && m.FromUser.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (message == null)
                return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}