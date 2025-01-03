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
    public class AddressViewModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<AddressViewModel> _logger = null;

        //public member becomes part of the Model in the Razor page

        public IAddress Address {get; set;}

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
                Address = await  _service.ReadAddressAsync(_id, flat)


            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return Page();
        }

        //Inject services just like in WebApi
        public AddressViewModel(IFriendsService service, ILogger<AddressViewModel> logger)
        {
            _logger = logger;
            _service = service;
        }
    }
}
