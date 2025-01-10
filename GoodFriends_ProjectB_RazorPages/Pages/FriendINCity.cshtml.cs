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
using Azure.Core;
using Seido.Utilities.SeedGenerator;
using Models.DTO;

namespace GoodFriends_ProjectB_RazorPages.Pages
{
    //Demonstrate how to use the model to present a list of objects
    public class FriendINCity : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<FriendINCity> _logger = null;
        private readonly SeedGenerator _seedGenerator;


        //public member becomes part of the Model in the Razor page


        [BindProperty]
        public bool UseSeeds { get; set; } = true;
        
        public List<IFriend> Friends { get; set; } = new List<IFriend>();

        public List<IAddress> City { get; set; } = new List<IAddress>();


       // public string City {get; set;}
        




        public int NrOfFriends { get; set; }

        //Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 5;
 
        [BindProperty]
        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;
        public int PresentPages { get; set; } = 0;

        //ModelBinding for the form
        [BindProperty]
        public string SearchFilter { get; set; } = null;

        //will execute on a Get request
        public async Task<IActionResult> OnGet()
        {
            
            
            var city = await _service.ReadAddressesAsync(UseSeeds, false, "Sweden", ThisPageNr, PageSize);
            
            Friends = city.PageItems.SelectMany(f => f.Friends).ToList<IFriend>();

          //  City = friends;
          // Friends = city.PageItems;

    

      

            return Page();
        }

    /*     private void UpdatePagination(int nrOfItems)
        {
            //Pagination
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }

        public async Task<IActionResult> OnPostSearch()
        {
            //Use the Service
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friends = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            //Page is rendered as the postback is part of the form tag
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteGroup(Guid groupId)
        {
            await _service.DeleteFriendAsync(groupId);

            //Use the Service
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friends = resp.PageItems;
            NrOfFriends= resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

          public async Task<IActionResult> OnPostDelete(Guid id)
        {
          await  _service.DeleteQuoteAsync(id);        
          
            //Pagination
            //UpdatePagination();

            //Use the Service
            var resp = await _service.ReadQuotesAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Quotes = resp.PageItems;

            //Quotes =  await _service.ReadQuoteAsync(id, false);
           // SelectedQuote = _service.ReadQuote(id);

            //Page is rendered as the postback is part of the form tag
            return Page();
        }            

        public async Task<IActionResult> OnPostDeletePet(Guid id)
        {
          await _service.DeletePetAsync(id);        
          
            //Pagination
            //UpdatePagination();

            var resp2 = await _service.ReadPetsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Pets = resp2.PageItems;
            //Quotes =  await _service.ReadQuoteAsync(id, false);
           // SelectedQuote = _service.ReadQuote(id);

            //Page is rendered as the postback is part of the form tag
            return Page();
        } */
        //Inject services just like in WebApi
        public FriendINCity(IFriendsService service, ILogger<FriendINCity> logger)
        {
            _logger = logger;
            _service = service;
            _seedGenerator = new SeedGenerator();
        }
    }
}
