using AutoMapper;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.Hubs;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public RoomsController(ManageAppDbContext context, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomVm>>> Get()
        {
            var rooms = await _context.Rooms.ToListAsync();

            var roomsVms = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomVm>>(rooms);

            return Ok(roomsVms);
        }

        [HttpGet("{roomId}")]
        public async Task<ActionResult<Room>> GetById(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                return NotFound();

            var roomsVm = _mapper.Map<Room, RoomVm>(room);
            return Ok(roomsVm);
        }

        [HttpPost]
        public async Task<ActionResult<Room>> Create(RoomVm roomViewModel)
        {
            if (_context.Rooms.Any(r => r.Name == roomViewModel.Name))
                return BadRequest("Invalid room name or room already exists");

            var user = _context.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var room = new Room()
            {
                Name = roomViewModel.Name,
                Admin = user
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("addChatRoom", new { id = room.Id, name = room.Name });

            return CreatedAtAction(nameof(GetById), new { roomId = room.Id }, new { id = room.Id, name = room.Name });
        }

        [HttpPut("{roomId}")]
        public async Task<IActionResult> Edit(int roomId, RoomVm roomVm)
        {
            if (_context.Rooms.Any(r => r.Name == roomVm.Name))
                return BadRequest("Invalid room name or room already exists");

            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == roomId && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            room.Name = roomVm.Name;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("updateChatRoom", new { id = room.Id, room.Name });

            return NoContent();
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> Delete(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == roomId && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
            await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted", string.Format("Room {0} has been deleted.\nYou are moved to the first available room!", room.Name));

            return NoContent();
        }
    }
}