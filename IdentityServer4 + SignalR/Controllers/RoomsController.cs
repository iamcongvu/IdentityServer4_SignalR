using AutoMapper;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Controllers
{
    public class RoomsController : BaseController
    {
        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;

        public RoomsController(ManageAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomVm>>> Get()
        {
            var rooms = await _context.Rooms.ToListAsync();

            var roomsVms = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomVm>>(rooms);

            return Ok(roomsVms);
        }

        [HttpGet("{roomId}")]
        public async Task<ActionResult<Room>> Get(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                return NotFound();

            var roomsVm = _mapper.Map<Room, RoomVm>(room);
            return Ok(roomsVm);
        }
    }
}