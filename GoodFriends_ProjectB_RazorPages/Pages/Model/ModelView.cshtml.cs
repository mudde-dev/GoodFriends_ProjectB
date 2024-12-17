using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models;
using Microsoft.EntityFrameworkCore;
using Services;
using GoodFriends_ProjectB_RazorPages.Pages;

namespace GoodFriends_ProjectB_RazorPages.Pages.Model
{
    //Demonstrate how to read Query parameters
    public class ModelViewModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<ModelViewModel> _logger = null;

        //public member becomes part of the Model in the Razor page
        public Quote Quote { get; set; }

        public IFriend Friend  { get; set; }

        public List<IFriend> Friends {get; set; } = new List<IFriend>();
        public string ErrorMessage { get; set; } = null;

        //Will execute on a Get request
        public async Task<IActionResult> OnGet()
        {
            try
            {
                //Read a QueryParameter
                Guid _id = Guid.Parse(Request.Query["id"]);

                //Use the Service
                Quote = (Quote)await _service.ReadQuoteAsync(_id, false);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return Page();
        }

        //Inject services just like in WebApi
        public ModelViewModel(IFriendsService service, ILogger<ModelViewModel> logger)
        {
            _logger = logger;
            _service = service;
        }
    }
}
