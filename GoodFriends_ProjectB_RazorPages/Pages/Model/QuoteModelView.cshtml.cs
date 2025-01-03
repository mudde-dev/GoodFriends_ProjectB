using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models;
using Microsoft.EntityFrameworkCore;
using Services;

namespace GoodFriends_ProjectB_RazorPages.Pages.Model
{
    //Demonstrate how to read Query parameters
    public class QuoteViewModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<QuoteViewModel> _logger = null;

        //public member becomes part of the Model in the Razor page

        public IQuote Quote {get; set;}

        public string ErrorMessage { get; set; } = null;

        //Will execute on a Get request
        public async Task<IActionResult> OnGet(string id, bool flat)
        {
            try
            {
                Guid _id = Guid.Parse(id);
                /*    //Read a QueryParameter
                   Guid _id = Guid.Parse(Request.Query["id"]); */

                //Use the Service
 
                Quote = await _service.ReadQuoteAsync(_id, flat);


            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return Page();
        }

        //Inject services just like in WebApi
        public QuoteViewModel(IFriendsService service, ILogger<QuoteViewModel> logger)
        {
            _logger = logger;
            _service = service;
        }
    }
}
