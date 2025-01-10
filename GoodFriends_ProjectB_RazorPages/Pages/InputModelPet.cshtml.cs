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
    public class InputModelPet : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<InputModelPet>? _logger = null;

        [BindProperty]
        public FamousPetIM PetIM { get; set; }

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
                    PetIM =  new FamousPetIM(await _service.ReadPetAsync(_id, false));
                    PageHeader = "Edit details of a pet";
                }
                else
                {
                    //Create an empty InputModel
                    PetIM = new FamousPetIM();
                    PetIM.StatusIM = StatusIM.Inserted;
                    PageHeader = "Create a new pet";
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
            PetIM = new FamousPetIM(await _service.ReadPetAsync(PetIM.PetId, false));          
            PageHeader = "Edit details of a pet";
            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        {

              if (PetIM.StatusIM == StatusIM.Inserted)
            {
                //It is an create
                var model = PetIM.UpdateModel(new Pet());
                model = await _service.CreatePetAsync(new PetCUdto(model));

                PetIM = new FamousPetIM(model);
            }
            else
            {
                //It is an update
                //Get orginal
                var model = await _service.ReadPetAsync(PetIM.PetId, true);

                //update the changes and save
                model = PetIM.UpdateModel(model);
                model = await  _service.UpdatePetAsync(new PetCUdto(model));
                
                PetIM = new FamousPetIM(model);
            } 
 
            //CHANGE THIS PATHWAY!

            //return Redirect($"~/Studies/Model/Search?search={FriendIM.FriendId}");


            PageHeader = "Edit details of a quote";
            return Page();
        }




        //Inject services just like in WebApi
        public InputModelPet(IFriendsService service, ILogger<InputModelPet> logger)
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

        public class FamousPetIM
        {
            //Status of InputModel
            public StatusIM StatusIM { get; set; }

            //Properties from Model which is to be edited in the <form>
            public Guid PetId { get; init; } = Guid.NewGuid();
            public AnimalMood Mood {get; set;}
            public AnimalKind Kind { get; set; }
            public string Name {get; set;}

        
            [BindProperty]
            [Required(ErrorMessage = "You must provide a Pet")]
            public List<IPet> Pets { get; set; }
            
           
            [BindProperty]
            [Required(ErrorMessage = "You must provide an Pet")]
            public string EditPets { get; set; }



            #region constructors and model update
            public FamousPetIM(Task<IPet> task) { StatusIM = StatusIM.Unchanged; }

            //Copy constructor
            public FamousPetIM(FamousPetIM original)
            {
                StatusIM = original.StatusIM;

                PetId = original.PetId;
                Pets = original.Pets;
                Mood = original.Mood;
                Kind = original.Kind;
                Name = original.Name;

                EditPets = original.EditPets;
                
      
                //EditPets = original.EditPets;
            }

            //Model => InputModel constructor
            public FamousPetIM(IPet original) :this()
            {
                StatusIM = StatusIM.Unchanged;
                PetId = original.PetId;
                Mood = original.Mood;
                Kind = (AnimalKind)original.Kind;
                Name = original.Name;            
             
            }

            public FamousPetIM()
            {
            }

            //InputModel => Model
            public IPet UpdateModel(IPet model)
            {
                model.PetId = PetId;
                model.Mood = Mood;
                model.Kind = (Models.AnimalKind)Kind;
                model.Name = Name;
       
            
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