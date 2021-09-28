using AutoMapper;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using IdentityServer4SignalR.Hubs;
using IdentityServer4SignalR.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Controllers
{
    public class UploadsController : BaseController
    {
        private readonly int FileSizeLimit; // gioi han kich thuoc file
        private readonly string[] AllowedExtensions; // loc dinh dang file
        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment; // giúp truy xuât được vào folder wwwroot
        private readonly IHubContext<ChatHub> _hubContext;

        public UploadsController(ManageAppDbContext context,
            IMapper mapper,
            IWebHostEnvironment environment,
            IConfiguration configruation,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            _hubContext = hubContext;

            FileSizeLimit = configruation.GetSection("FileUpload").GetValue<int>("FileSizeLimit");
            AllowedExtensions = configruation.GetSection("FileUpload").GetValue<string>("AllowedExtensions").Split(",");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] UploadVm uploadVm)
        {
            if (ModelState.IsValid)
            {
                if (!Validate(uploadVm.File))
                {
                    return BadRequest("Validation failed!");
                }

                var fileName = DateTime.Now.ToString("yyyymmddMMss") + "_" + Path.GetFileName(uploadVm.File.FileName);
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads");
                var filePath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadVm.File.CopyToAsync(fileStream);
                }

                var user = _context.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                var room = _context.Rooms.Where(r => r.Id == uploadVm.RoomId).FirstOrDefault();
                if (user == null || room == null)
                    return NotFound();

                string htmlImage = string.Format(
                    "<a href=\"/uploads/{0}\" target=\"_blank\">" +
                    "<img src=\"/uploads/{0}\" class=\"post-image\">" +
                    "</a>", fileName);

                var message = new Message()
                {
                    Content = Regex.Replace(htmlImage, @"(?i)<(?!img|a|/a|/img).*?>", string.Empty),
                    Timestamp = DateTime.Now,
                    FromUser = user,
                    ToRoom = room
                };

                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();

                // Send image-message to group
                var messageVm = _mapper.Map<Message, MessageVm>(message);
                await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", messageVm);

                return Ok();
            }

            return BadRequest();
        }

        private bool Validate(IFormFile file)
        {
            if (file.Length > FileSizeLimit)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            // AllowedExtensions: lọc định dạng ảnh img, jpg, png
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Any(s => s.Contains(extension)))
                return false;

            return true;
        }
    }
}