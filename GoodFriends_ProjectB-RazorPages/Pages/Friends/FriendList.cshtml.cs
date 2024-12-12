using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models;
using Microsoft.EntityFrameworkCore;
using Services;
using AppStudies.Pages;
using Azure.Core;

namespace AppStudies.Pages
{
    //Demonstrate how to use the model to present a list of objects
    public class FriendlListModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<FriendlListModel> _logger = null;

        //public member becomes part of the Model in the Razor page
        public List<IFriend> Friends { get; set; } = new List<IFriend>();


        //Will execute on a Get request
        public async IActionResult OnGet()
        {
            //Just to show how to get current uri
            var uri = Request.Path;

            //Use the Service
            var resp = await _service.ReadFriendsAsync();
            Friends = resp.PageItems;
            return Page();
        }

        //Inject services just like in WebApi
        public FriendlListModel(IFriendsService service, ILogger<FriendlListModel> logger)
        {
            _logger = logger;
            _service = service;
        }
    }
}
