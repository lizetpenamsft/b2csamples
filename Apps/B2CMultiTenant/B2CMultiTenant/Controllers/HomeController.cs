﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using B2CMultiTenant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using B2CMultiTenant.Extensions;

namespace B2CMultiTenant.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public HomeController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }
        TokenService _tokenService;
        public IActionResult Index()
        {
            if (User.FindFirst(c => c.Type == "isNewMember")?.Value == "true")
            {
                var tenantName = User.FindFirst(c => c.Type == "appTenantName").Value;
                ViewBag.ReentryUrl = $"{Request.Host}/home/membersignin?tenant={tenantName}";
            }
            return View(User.Claims);
        }
        public IActionResult MemberSignIn()
        {
            var p = new AuthenticationProperties() { RedirectUri = "/Home/Index" };
            if (Request.Query.ContainsKey("tenant"))
                p.Parameters.Add("tenant", Request.Query["tenant"]);
            return Challenge(p,
                new string[] { "mtsusi" });
        }
        public IActionResult NewTenant()
        {
            return Challenge(
                new AuthenticationProperties() { RedirectUri = "/Home/Index" },
                new string[] { "mtsusint" });
        }
        [Authorize]
        public IActionResult SignOut()
        {
            var loginPath = User.Claims.First(c => c.Type == "http://schemas.microsoft.com/claims/authnclassreference").Value;
            var policy = loginPath.Split('_')[2];
            return SignOut(
                new AuthenticationProperties() { RedirectUri = "/Home/Index" },
                new string[] { CookieAuthenticationDefaults.AuthenticationScheme, policy });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
