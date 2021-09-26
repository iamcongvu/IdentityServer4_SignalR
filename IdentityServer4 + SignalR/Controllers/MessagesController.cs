using AutoMapper;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Controllers
{
    public class MessagesController : BaseController
    {
        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;

        public MessagesController(ManageAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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