using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4SignalR.Data;
using IdentityServer4SignalR.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4SignalR.Services
{
    public class IdentityProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<User> _claimsFactory;
        private readonly UserManager<User> _userManager;
        private readonly ManageAppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityProfileService(IUserClaimsPrincipalFactory<User> claimsFactory,
            UserManager<User> userManager,
            ManageAppDbContext context,
           RoleManager<IdentityRole> roleManager)
        {
            _claimsFactory = claimsFactory;
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
            {
                throw new ArgumentException("");
            }

            var principal = await _claimsFactory.CreateAsync(user);//lấy ra danh sách claim trong db AspNetUserClaims
            var claims = principal.Claims.ToList();
            var roles = await _userManager.GetRolesAsync(user);

            //Add more claims like this
            claims.Add(new Claim(ClaimTypes.Name, user.UserName)); // gán vào claim // profile: name
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id)); // profile: sub

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}