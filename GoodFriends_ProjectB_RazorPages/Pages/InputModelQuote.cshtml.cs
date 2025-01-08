using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AppStudies.SeidoHelpers;
using Models;
using Microsoft.EntityFrameworkCore;
using Services;
using GoodFriends_ProjectB_RazorPages.Pages.Pages;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.DTO;


namespace GoodFriends_ProjectB_RazorPages.Pages.Pages
{
    //Demonstrate how to read Query parameters
    public class InputModelQuote : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<InputModelQuote>? _logger = null;

        [BindProperty]
        public FamousQuoteIM QuoteIM { get; set; }

        public string PageHeader { get; set; }

        //public member becomes part of the Model in the Razor page
        public string? ErrorMessage { get; set; } = null;

        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        //For Server Side Validation set by IsValid()
        public bool HasValidationErrors { get; set; }
        public IEnumerable<string> ValidationErrorMsgs { get; set; }
        public IEnumerable<KeyValuePair<string, ModelStateEntry>> InvalidKeys { get; set; }


        //Will execute on a Get request
        public async Task<IActionResult> OnGet()
        {
            try
            {
                if (Guid.TryParse(Request.Query["id"], out Guid _id))
                {
                    //Use the Service and populate the InputModel
                    QuoteIM =  new FamousQuoteIM(await _service.ReadQuoteAsync(_id, false));
                    PageHeader = "Edit details of a quote";
                }
                else
                {
                    //Create an empty InputModel
                    QuoteIM = new FamousQuoteIM();
                    QuoteIM.StatusIM = StatusIM.Inserted;
                    PageHeader = "Create a new quote";
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostUndo(Guid id)
        {
            //Use the Service and populate the InputModel
            QuoteIM = new FamousQuoteIM(await _service.ReadQuoteAsync(id, false));          
            PageHeader = "Edit details of a quote";
            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        { 
            /*
            //PageHeader is stored in TempData which has to be set after a Post
            PageHeader = (FriendIM.StatusIM == StatusIM.Inserted) ?
                "Create a new friend" : "Edit details of a friend";

            //I use IsValid() instead of ModelState.IsValid in order to extract
            //error information
            if (!IsValid())
            {
                //The page is not valid
                return Page();
            }

            if (FriendIM.StatusIM == StatusIM.Inserted)
            {
                //It is an create
                var model = FriendIM.UpdateModel(new csFriend());
                model = await  _service.CreateFriendAsync(new FriendCUdto(model));
                FriendIM = new FamousFriendIM(model);
            }
            else
            {
                //It is an update
                //Get orginal
                var model = await _service.ReadFriendAsync(FriendIM.FriendId, true);

                //update the changes and save
                model = FriendIM.UpdateModel(model);
                model = await _service.UpdateFriendAsync(new FriendCUdto(model));
                FriendIM = new FamousFriendIM(model);
            } */

              if (QuoteIM.StatusIM == StatusIM.Inserted)
            {
                //It is an create
                var model = QuoteIM.UpdateModel(new Quote());
                model = await _service.CreateQuoteAsync(new QuoteCUdto(model));

                QuoteIM = new FamousQuoteIM(model);
            }
            else
            {
                //It is an update
                //Get orginal
                var model = await _service.ReadQuoteAsync(QuoteIM.QuoteId, true);

                //update the changes and save
                model = QuoteIM.UpdateModel(model);
                model = await  _service.UpdateQuoteAsync(new QuoteCUdto(model));
                
                QuoteIM = new FamousQuoteIM(model);
            } 
 
            //CHANGE THIS PATHWAY!

            //return Redirect($"~/Studies/Model/Search?search={FriendIM.FriendId}");


            PageHeader = "Edit details of a quote";
            return Page();
        }

       

        //CAN´T ACCESS ENUMERABLE METHODS OR GENERIC BECAUSE  = **THIS IS FOR THE EDIT/DELETE LIST OF ADDRESSES - USE LATER!

     /*      public IActionResult OnPostDelete(Guid friendId)
        {
            //Set the Quote as deleted, it will not be rendered
            FriendIM.First(q => q.FriendId == friendId).StatusIM = StatusIM.Deleted;
            return Page();
        }

        public IActionResult OnPostEdit(Guid quoteId)
        {
            int idx = FriendIM.FindIndex(q => q.FriendId == friendId);
            string[] keys = { $"QuotesIM[{idx}].EditQuote",
                            $"QuotesIM[{idx}].EditAuthor"};
            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Quote as Modified, it will later be updated in the database
            var q = FriendIM.First(q => q.QuoteId == quoteId);
            q.StatusIM = StatusIM.Modified;

            //Implement the changes
            q.Author = q.EditAuthor;
            q.Quote = q.EditQuote;
            return Page();
        }   */


        //Inject services just like in WebApi
        public InputModelQuote(IFriendsService service, ILogger<InputModelQuote> logger)
        {
            _logger = logger;
            _service = service;
        }

        #region Input Model
        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        //These classes are in center of ModelBinding and Validation
        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

        public class FamousQuoteIM
        {
            //Status of InputModel
            public StatusIM StatusIM { get; set; }

            //Properties from Model which is to be edited in the <form>
            public Guid QuoteId { get; init; } = Guid.NewGuid();

            [BindProperty]
            [Required(ErrorMessage = "You type provide a quote")]
            public string Quote { get; set; }

          

            [BindProperty]
            [Required(ErrorMessage = "You must provide an quote")]
            public string EditQuote { get; set; }



            #region constructors and model update
            public FamousQuoteIM(Task<IQuote> task) { StatusIM = StatusIM.Unchanged; }

            //Copy constructor
            public FamousQuoteIM(FamousQuoteIM original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                Quote = original.Quote;        
              
                EditQuote = original.EditQuote;
             
            }

            //Model => InputModel constructor
            public FamousQuoteIM(IQuote original)
            {
                StatusIM = StatusIM.Unchanged;
                QuoteId = original.QuoteId;
                Quote = original.QuoteText;
               
                Quote = EditQuote = original.QuoteText;
            }

            public FamousQuoteIM()
            {
            }



            //InputModel => Model
            public IQuote UpdateModel(IQuote model)
            {
                model.QuoteId = QuoteId;
                model.QuoteText = Quote;
                return model;
            }
            #endregion

        }
        #endregion

        #region Server Side Validation
        private bool IsValid(string[] validateOnlyKeys = null)
        {
            InvalidKeys = ModelState
               .Where(s => s.Value.ValidationState == ModelValidationState.Invalid);

            if (validateOnlyKeys != null)
            {
                InvalidKeys = InvalidKeys.Where(s => validateOnlyKeys.Any(vk => vk == s.Key));
            }

            ValidationErrorMsgs = InvalidKeys.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage);
            HasValidationErrors = InvalidKeys.Any();

            return !HasValidationErrors;
        }
        #endregion
    }
}