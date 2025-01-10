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
    public class InputModelSimpleModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<InputModelSimpleModel>? _logger = null;

        [BindProperty]
        public FamousFriendIM FriendIM { get; set; }

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
                    FriendIM =  new FamousFriendIM(await _service.ReadFriendAsync(_id, false));
                    PageHeader = "Edit details of a friend";
                }
                else
                {
                    //Create an empty InputModel
                    FriendIM = new FamousFriendIM();
                    FriendIM.StatusIM = StatusIM.Inserted;
                    PageHeader = "Create a new friend";
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostUndo()
        {
            //Use the Service and populate the InputModel
            FriendIM = new FamousFriendIM(await _service.ReadFriendAsync(FriendIM.FriendId, false));          
            PageHeader = "Edit details of a friend";
            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        { 

              if (FriendIM.StatusIM == StatusIM.Inserted)
            {
                //It is an create
                var model = FriendIM.UpdateModel(new csFriend());
                model = await _service.CreateFriendAsync(new FriendCUdto(model));

                FriendIM = new FamousFriendIM(model);
            }
            else
            {
                //It is an update
                //Get orginal
                var model = await _service.ReadFriendAsync(FriendIM.FriendId, true);

                //update the changes and save
                model = FriendIM.UpdateModel(model);
                model = await  _service.UpdateFriendAsync(new FriendCUdto(model));
                
                FriendIM = new FamousFriendIM(model);
            } 
 
            //CHANGE THIS PATHWAY!

            //return Redirect($"~/Studies/Model/Search?search={FriendIM.FriendId}");


            PageHeader = "Edit details of a quote";
            return Page();
        }

 


        //Inject services just like in WebApi
        public InputModelSimpleModel(IFriendsService service, ILogger<InputModelSimpleModel> logger)
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

        public class FamousFriendIM
        {
            //Status of InputModel
            public StatusIM StatusIM { get; set; }

            //Properties from Model which is to be edited in the <form>
            public Guid FriendId { get; init; } = Guid.NewGuid();

            [BindProperty]
            [Required(ErrorMessage = "You type provide a FirstName")]
            public string FirstName { get; set; }

            [BindProperty]
            [Required(ErrorMessage = "You type provide a LastName")]
            public string LastName { get; set; }

            [BindProperty]
            [Required(ErrorMessage = "You type provide a FirstName")]
            public string EditFirstName { get; set; }
          

            [BindProperty]
            [Required(ErrorMessage = "You type provide a LastName")]
            public string EditLastName { get; set; }
          
            [BindProperty]
            [Required(ErrorMessage = "You type provide a Quote")]
            public List<IQuote> Quotes { get; set; }

            [BindProperty]
            [Required(ErrorMessage = "You type provide a Pet")]
            public List<IPet> Pets { get; set; }
          

            #region constructors and model update
            public FamousFriendIM(/*Task<IFriend> task*/) { StatusIM = StatusIM.Unchanged; }

            //Copy constructor
            public FamousFriendIM(FamousFriendIM original)
            {
                StatusIM = original.StatusIM;

                FriendId = original.FriendId;
                FirstName = original.FirstName;
                LastName = original.LastName;
                Quotes = original.Quotes;
                Pets = original.Pets;
                
                EditFirstName = original.FirstName;
                EditLastName = original.LastName;
                
            }

            //Model => InputModel constructor
            public FamousFriendIM(IFriend original)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = original.FriendId;
                FirstName = original.FirstName;
                LastName = original.LastName;
                Quotes = original.Quotes;
                Pets = original.Pets;

                EditFirstName = original.FirstName;
                EditLastName = original.LastName;
            
            }

         

            //InputModel => Model
            public IFriend UpdateModel(IFriend model)
            {
                model.FriendId = FriendId;
                model.FirstName = FirstName;
                model.LastName = LastName;
                
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