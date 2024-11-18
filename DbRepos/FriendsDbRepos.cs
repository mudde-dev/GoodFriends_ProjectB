using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;

using Seido.Utilities.SeedGenerator;
using Models;
using Models.DTO;
using DbModels;
using DbContext;

namespace DbRepos;

public class FriendsDbRepos
{
    const string _seedSource = "./friends-seeds.json";
    private ILogger<FriendsDbRepos> _logger;
    private readonly MainDbContext _dbContext;

    #region contructors
    public FriendsDbRepos(ILogger<FriendsDbRepos> logger, MainDbContext context)
    {
        _logger = logger;
        _dbContext = context;
    }
    #endregion

    #region Admin repo methods
    public async Task<GstUsrInfoAllDto> InfoAsync()
    {
        return await DbInfo();
    }

    private async Task<GstUsrInfoAllDto> DbInfo()
    {
        var info = new GstUsrInfoAllDto();
        info.Db = await _dbContext.InfoDbView.FirstAsync();
        info.Friends = await _dbContext.InfoFriendsView.ToListAsync();
        info.Pets = await _dbContext.InfoPetsView.ToListAsync();
        info.Quotes = await _dbContext.InfoQuotesView.ToListAsync();

        return info;
    }

    public async Task<GstUsrInfoAllDto> SeedAsync(int nrOfItems)
    {
        //First of all make sure the database is cleared from all seeded data
        await RemoveSeedAsync(true);

        //Create a seeder
        var fn = Path.GetFullPath(_seedSource);
        var seeder = new SeedGenerator(fn);

        //Seeding the  quotes table
        var quotes = seeder.AllQuotes.Select(q => new QuoteDbM(q)).ToList();

        #region Full seeding
        //Generate friends and addresses
        var friends = seeder.ItemsToList<FriendDbM>(nrOfItems);
        var addresses = seeder.UniqueItemsToList<AddressDbM>(nrOfItems);

        //Assign Address, Pets and Quotes to all the friends
        foreach (var friend in friends)
        {
            friend.AddressDbM = (seeder.Bool) ? seeder.FromList(addresses) : null;
            friend.PetsDbM =  seeder.ItemsToList<PetDbM>(seeder.Next(0, 4));
            friend.QuotesDbM = seeder.UniqueItemsPickedFromList(seeder.Next(0, 6), quotes);
        }

        //Note that all other tables are automatically set through FriendDbM Navigation properties
        _dbContext.Friends.AddRange(friends);
        #endregion

        await _dbContext.SaveChangesAsync();
        var info = await DbInfo();
        return info;
    }
    
    public async Task<GstUsrInfoAllDto> RemoveSeedAsync(bool seeded)
    {
        var parameters = new List<SqlParameter>();

        var retvalue = new SqlParameter("retval", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var seededArg = new SqlParameter("seeded", seeded);
        var nrF = new SqlParameter("nrF", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var nrA = new SqlParameter("nrA", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var nrP = new SqlParameter("nrP", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var nrQ = new SqlParameter("nrQ", SqlDbType.Int) { Direction = ParameterDirection.Output };

        parameters.Add(retvalue);
        parameters.Add(seededArg);
        parameters.Add(nrF);
        parameters.Add(nrA);
        parameters.Add(nrP);
        parameters.Add(nrQ);

        await Task.Run(() =>
            _dbContext.InfoDbView.FromSqlRaw($"EXEC @retval = supusr.spDeleteAll @seeded," +
                $"@nrF OUTPUT, @nrA OUTPUT, " +
                $"@nrP OUTPUT, @nrQ OUTPUT", parameters.ToArray()).AsEnumerable());

        return await DbInfo();
    }
    #endregion
    
    #region Friends repo methods
    public async Task<IFriend> ReadFriendAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Friends.AsNoTracking()
                .Include(i => i.AddressDbM)
                .Include(i => i.PetsDbM)
                .Include(i => i.QuotesDbM)
                .Where(i => i.FriendId == id);

            return await query.FirstOrDefaultAsync<IFriend>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Friends.AsNoTracking()
                .Where(i => i.FriendId == id);

            return await query.FirstOrDefaultAsync<IFriend>();
        }   
    }

    public async Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<FriendDbM> query;
        if (flat)
        {
            query = _dbContext.Friends.AsNoTracking();
        }
        else
        {
            query = _dbContext.Friends.AsNoTracking()
                .Include(i => i.AddressDbM)
                .Include(i => i.PetsDbM)
                .Include(i => i.QuotesDbM);
        }

        var ret = new ResponsePageDto<IFriend>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.FirstName.ToLower().Contains(filter) ||
                            i.LastName.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.FirstName.ToLower().Contains(filter) ||
                            i.LastName.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IFriend>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<IFriend> DeleteFriendAsync(Guid id)
    {
        //Find the instance with matching id
        var query1 = _dbContext.Friends
            .Where(i => i.FriendId == id);
        var item = await query1.FirstOrDefaultAsync<FriendDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Friends.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;   
    }

    public async Task<IFriend> UpdateFriendAsync(FriendCUdto itemDto)
    {
        //Find the instance with matching id and read the navigation properties.
        var query1 = _dbContext.Friends
            .Where(i => i.FriendId == itemDto.FriendId);
        var item = await query1
            .Include(i => i.AddressDbM)
            .Include(i => i.PetsDbM)
            .Include(i => i.QuotesDbM)
            .FirstOrDefaultAsync<FriendDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.FriendId} is not existing");

        //transfer any changes from DTO to database objects
        //Update individual properties
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_FriendCUdto_to_FriendDbM(itemDto, item);

        //write to database model
        _dbContext.Friends.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadFriendAsync(item.FriendId, false);    
    }

    public async Task<IFriend> CreateFriendAsync(FriendCUdto itemDto)
    {
        if (itemDto.FriendId != null)
            throw new ArgumentException($"{nameof(itemDto.FriendId)} must be null when creating a new object");

        //transfer any changes from DTO to database objects
        //Update individual properties Friend
        var item = new FriendDbM(itemDto);

        //Update navigation properties
        await navProp_FriendCUdto_to_FriendDbM(itemDto, item);

        //write to database model
        _dbContext.Friends.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadFriendAsync(item.FriendId, false);   
    }

    //from all Guid relationships in _itemDtoSrc finds the corresponding object in the database and assigns it to _itemDst 
    //as navigation properties. Error is thrown if no object is found corresponing to an id.
    private async Task navProp_FriendCUdto_to_FriendDbM(FriendCUdto itemDtoSrc, FriendDbM itemDst)
    {
        //update AddressDbM from itemDto.AddressId
        itemDst.AddressDbM = (itemDtoSrc.AddressId != null) ? await _dbContext.Addresses.FirstOrDefaultAsync(
            a => (a.AddressId == itemDtoSrc.AddressId)) : null;

        //update PetsDbM from itemDto.PetsId list
        List<PetDbM> pets = null;
        if (itemDtoSrc.PetsId != null)
        {
            pets = new List<PetDbM>();
            foreach (var id in itemDtoSrc.PetsId)
            {
                var p = await _dbContext.Pets.FirstOrDefaultAsync(i => i.PetId == id);
                if (p == null)
                    throw new ArgumentException($"Item id {id} not existing");

                pets.Add(p);
            }
        }
        itemDst.PetsDbM = pets;

        //update QuotesDbM from itemDto.QuotesId
        List<QuoteDbM> quotes = null;
        if (itemDtoSrc.QuotesId != null)
        {
            quotes = new List<QuoteDbM>();
            foreach (var id in itemDtoSrc.QuotesId)
            {
                var q = await _dbContext.Quotes.FirstOrDefaultAsync(i => i.QuoteId == id);
                if (q == null)
                    throw new ArgumentException($"Item id {id} not existing");

                quotes.Add(q);
            }
        }
        itemDst.QuotesDbM = quotes;
    }
    #endregion

    #region Addresses repo methods
    public async Task<IAddress> ReadAddressAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Addresses.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.QuotesDbM)
                .Where(i => i.AddressId == id);

            return await query.FirstOrDefaultAsync<IAddress>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Addresses.AsNoTracking()
                .Where(i => i.AddressId == id);

            return await query.FirstOrDefaultAsync<IAddress>();
        }  
    }

    public async Task<ResponsePageDto<IAddress>> ReadAddressesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<AddressDbM> query;
        if (flat)
        {
            query = _dbContext.Addresses.AsNoTracking();
        }
        else
        {
            query = _dbContext.Addresses.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.QuotesDbM);
        }

        var ret = new ResponsePageDto<IAddress>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.StreetAddress.ToLower().Contains(filter) ||
                            i.City.ToLower().Contains(filter) ||
                            i.Country.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.StreetAddress.ToLower().Contains(filter) ||
                            i.City.ToLower().Contains(filter) ||
                            i.Country.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IAddress>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<IAddress> DeleteAddressAsync(Guid id)
    {
        var query1 = _dbContext.Addresses
            .Where(i => i.AddressId == id);

        var item = await query1.FirstOrDefaultAsync<AddressDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Addresses.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;    
    }

    public async Task<IAddress> UpdateAddressAsync(AddressCUdto itemDto)
    {
        var query1 = _dbContext.Addresses
            .Where(i => i.AddressId == itemDto.AddressId);
        var item = await query1
                .Include(i => i.FriendsDbM)
                .FirstOrDefaultAsync<AddressDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.AddressId} is not existing");

        //I cannot have duplicates in the Addresses table, so check that
        var query2 = _dbContext.Addresses
            .Where(i => ((i.StreetAddress == itemDto.StreetAddress) &&
            (i.ZipCode == itemDto.ZipCode) &&
            (i.City == itemDto.City) &&
            (i.Country == itemDto.Country)));
        var existingItem = await query2.FirstOrDefaultAsync<AddressDbM>();
        if (existingItem != null && existingItem.AddressId != itemDto.AddressId)
            throw new ArgumentException($"Item already exist with id {existingItem.AddressId}");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_AddressCUdto_to_AddressDbM(itemDto, item);

        //write to database model
        _dbContext.Addresses.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadAddressAsync(item.AddressId, false);    
    }

    public async Task<IAddress> CreateAddressAsync(AddressCUdto itemDto)
    { 
        if (itemDto.AddressId != null)
            throw new ArgumentException($"{nameof(itemDto.AddressId)} must be null when creating a new object");

        //I cannot have duplicates in the Addresses table, so check that
        var query2 = _dbContext.Addresses
            .Where(i => ((i.StreetAddress == itemDto.StreetAddress) &&
                (i.ZipCode == itemDto.ZipCode) &&
                (i.City == itemDto.City) &&
                (i.Country == itemDto.Country)));
        var existingItem = await query2.FirstOrDefaultAsync<AddressDbM>();
        if (existingItem != null)
            throw new ArgumentException($"Item already exist with id {existingItem.AddressId}");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        var item = new AddressDbM(itemDto);

        //Update navigation properties
        await navProp_AddressCUdto_to_AddressDbM(itemDto, item);

        //write to database model
        _dbContext.Addresses.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadAddressAsync(item.AddressId, false);    
    }

    private async Task navProp_AddressCUdto_to_AddressDbM(AddressCUdto itemDtoSrc, AddressDbM itemDst)
    {
        //update FriendsDbM from itemDto.FriendId
        List<FriendDbM> friends = null;
        if (itemDtoSrc.FriendsId != null)
        {
            friends = new List<FriendDbM>();
            foreach (var id in itemDtoSrc.FriendsId)
            {
                var f = await _dbContext.Friends.FirstOrDefaultAsync(i => i.FriendId == id);
                if (f == null)
                    throw new ArgumentException($"Item id {id} not existing");

                friends.Add(f);
            }
        }
        itemDst.FriendsDbM = friends;
    }
    #endregion

    #region Quotes repo methods
    public async Task<IQuote> ReadQuoteAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Quotes.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.AddressDbM)
                .Where(i => i.QuoteId == id);

            return await query.FirstOrDefaultAsync<IQuote>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Quotes.AsNoTracking()
                .Where(i => i.QuoteId == id);

            return await query.FirstOrDefaultAsync<IQuote>();
        }
    }

    public async Task<ResponsePageDto<IQuote>> ReadQuotesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<QuoteDbM> query;
        if (flat)
        {
            query = _dbContext.Quotes.AsNoTracking();
        }
        else
        {
            query = _dbContext.Quotes.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.AddressDbM);

        }

        var ret = new ResponsePageDto<IQuote>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.QuoteText.ToLower().Contains(filter) ||
                            i.Author.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.QuoteText.ToLower().Contains(filter) ||
                            i.Author.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IQuote>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret; 
    }

    public async Task<IQuote> DeleteQuoteAsync(Guid id)
    {
        var query1 = _dbContext.Quotes
            .Where(i => i.QuoteId == id);

        var item = await query1.FirstOrDefaultAsync<QuoteDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Quotes.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;  
    }

    public async Task<IQuote> UpdateQuoteAsync(QuoteCUdto itemDto)
    {
        var query1 = _dbContext.Quotes
            .Where(i => i.QuoteId == itemDto.QuoteId);
        var item = await query1
                .Include(i => i.FriendsDbM)
                .FirstOrDefaultAsync<QuoteDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.QuoteId} is not existing");

        //Avoid duplicates in the Quotes table, so check that
        var query2 = _dbContext.Quotes
            .Where(i => ((i.Author == itemDto.Author) &&
            (i.QuoteText == itemDto.Quote)));
        var existingItem = await query2.FirstOrDefaultAsync<QuoteDbM>();
        if (existingItem != null && existingItem.QuoteId != itemDto.QuoteId)
            throw new ArgumentException($"Item already exist with id {existingItem.QuoteId}");


        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_QuoteCUdto_to_QuoteDbM(itemDto, item);

        //write to database model
        _dbContext.Quotes.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadQuoteAsync(item.QuoteId, false);    
 }

    public async Task<IQuote> CreateQuoteAsync(QuoteCUdto itemDto)
    {
        if (itemDto.QuoteId != null)
            throw new ArgumentException($"{nameof(itemDto.QuoteId)} must be null when creating a new object");

        //Avoid duplicates in the Quotes table, so check that
        var query2 = _dbContext.Quotes
            .Where(i => ((i.Author == itemDto.Author) &&
            (i.QuoteText == itemDto.Quote)));
        var existingItem = await query2.FirstOrDefaultAsync<QuoteDbM>();
        if (existingItem != null)
            throw new ArgumentException($"Item already exist with id {existingItem.QuoteId}");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        var item = new QuoteDbM(itemDto);

        //Update navigation properties
        await navProp_QuoteCUdto_to_QuoteDbM(itemDto, item);


        //write to database model
        _dbContext.Quotes.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadQuoteAsync(item.QuoteId, false);    
    }

    private async Task navProp_QuoteCUdto_to_QuoteDbM(QuoteCUdto itemDtoSrc, QuoteDbM itemDst)
    {
        //update FriendsDbM from itemDto.FriendId
        List<FriendDbM> friends = null;
        if (itemDtoSrc.FriendsId != null)
        {
            friends = new List<FriendDbM>();
            foreach (var id in itemDtoSrc.FriendsId)
            {
                var f = await _dbContext.Friends.FirstOrDefaultAsync(i => i.FriendId == id);
                if (f == null)
                    throw new ArgumentException($"Item id {id} not existing");

                friends.Add(f);
            }
        }
        itemDst.FriendsDbM = friends;
    }
    #endregion

    #region Pets repo methods
    public async Task<IPet> ReadPetAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Pets.AsNoTracking()
                .Include(i => i.FriendDbM)
                .ThenInclude(i => i.AddressDbM)
                .Include(i => i.FriendDbM)
                .ThenInclude(i => i.QuotesDbM)
                .Where(i => i.PetId == id);

            return await query.FirstOrDefaultAsync<IPet>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Pets.AsNoTracking()
                .Where(i => i.PetId == id);

            return await query.FirstOrDefaultAsync<IPet>();
        } 
    }

    public async Task<ResponsePageDto<IPet>> ReadPetsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<PetDbM> query;
        if (flat)
        {
            query = _dbContext.Pets.AsNoTracking();
        }
        else
        {
            query = _dbContext.Pets.AsNoTracking()
                .Include(i => i.FriendDbM)
                .ThenInclude(i => i.AddressDbM)
                .Include(i => i.FriendDbM)
                .ThenInclude(i => i.QuotesDbM);
        }

        var ret = new ResponsePageDto<IPet>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.Name.ToLower().Contains(filter) ||
                            i.strMood.ToLower().Contains(filter) ||
                            i.strKind.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.Name.ToLower().Contains(filter) ||
                            i.strMood.ToLower().Contains(filter) ||
                            i.strKind.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IPet>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<IPet> DeletePetAsync(Guid id)
    {
        var query1 = _dbContext.Pets
            .Where(i => i.PetId == id);

        var item = await query1.FirstOrDefaultAsync<PetDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Pets.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;
    }

    public async Task<IPet> UpdatePetAsync(PetCUdto itemDto)
    {
        var query1 = _dbContext.Pets
            .Where(i => i.PetId == itemDto.PetId);
        var item = await query1
                .Include(i => i.FriendDbM)
                .FirstOrDefaultAsync<PetDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.PetId} is not existing");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_PetCUdto_to_PetDbM(itemDto, item);

        //write to database model
        _dbContext.Pets.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadPetAsync(item.PetId, false);    
    }

    public async Task<IPet> CreatePetAsync(PetCUdto itemDto)
    {
        if (itemDto.PetId != null)
            throw new ArgumentException($"{nameof(itemDto.PetId)} must be null when creating a new object");

        //transfer any changes from DTO to database objects
        //Update individual properties
        var item = new PetDbM(itemDto);

        //Update navigation properties
        await navProp_PetCUdto_to_PetDbM(itemDto, item);

        //write to database model
        _dbContext.Pets.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadPetAsync(item.PetId, false);    
    }

    private async Task navProp_PetCUdto_to_PetDbM(PetCUdto itemDtoSrc, PetDbM itemDst)
    {
        //update owner, i.e. navigation property FriendDbM
        var owner = await _dbContext.Friends.FirstOrDefaultAsync(
            a => (a.FriendId == itemDtoSrc.FriendId));

        if (owner == null)
            throw new ArgumentException($"Item id {itemDtoSrc.FriendId} not existing");

        itemDst.FriendDbM = owner;
    }
    #endregion
}
