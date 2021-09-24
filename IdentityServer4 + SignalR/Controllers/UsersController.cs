using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.Helpers;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("Bearer")]
    public class UsersController : ControllerBase
    {
        public readonly UserManager<User> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public readonly ManageAppDbContext _context;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ManageAppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostUser(UserCreateRequest request)
        {
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                // Birthday = DateTime.Parse(request.Dob),
                UserName = request.UserName,
                //DisplayName = request.FirstName + " " + request.LastName,
                PhoneNumber = request.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { userId = user.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            }
            var userVm = new UserVm()
            {
                Id = user.Id,
                UserName = user.UserName,
                // Dob = user.Birthday,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };
            return Ok(userVm);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var user = _userManager.Users;
            var query = await user.Select(x => new UserVm()
            {
                Id = x.Id,
                UserName = x.UserName,
                // Dob = x.Birthday,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
            }).ToListAsync();

            return Ok(query);
        }
    }
}