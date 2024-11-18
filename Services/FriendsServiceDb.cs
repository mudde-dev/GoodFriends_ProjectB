using System;
using DbContext;
using DbRepos;
using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;

namespace Services;

public class FriendsServiceDb : IFriendsService
{
    private readonly FriendsDbRepos _repo = null;
    private readonly ILogger<FriendsServiceDb> _logger = null;


    #region constructors
    public FriendsServiceDb(FriendsDbRepos repo)
    {
        _repo = repo;
    }
    public FriendsServiceDb(FriendsDbRepos repo, ILogger<FriendsServiceDb> logger):this(repo)
    {
        _logger = logger;
    }
    #endregion
    
    #region Simple 1:1 calls in this case, but as Services expands, this will no longer be the case
    public Task<GstUsrInfoAllDto> InfoAsync => _repo.InfoAsync();

    public Task<GstUsrInfoAllDto> SeedAsync(int nrOfItems) => _repo.SeedAsync(nrOfItems);
    public Task<GstUsrInfoAllDto> RemoveSeedAsync(bool seeded) => _repo.RemoveSeedAsync(seeded);

    public Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadFriendsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IFriend> ReadFriendAsync(Guid id, bool flat) => _repo.ReadFriendAsync(id, flat);
    public Task<IFriend> DeleteFriendAsync(Guid id) => _repo.DeleteFriendAsync(id);
    public Task<IFriend> UpdateFriendAsync(FriendCUdto item) => _repo.UpdateFriendAsync(item);
    public Task<IFriend> CreateFriendAsync(FriendCUdto item) => _repo.CreateFriendAsync(item);

    public Task<ResponsePageDto<IAddress>> ReadAddressesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadAddressesAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IAddress> ReadAddressAsync(Guid id, bool flat) => _repo.ReadAddressAsync(id, flat);
    public Task<IAddress> DeleteAddressAsync(Guid id) => _repo.DeleteAddressAsync(id);
    public Task<IAddress> UpdateAddressAsync(AddressCUdto item) => _repo.UpdateAddressAsync(item);
    public Task<IAddress> CreateAddressAsync(AddressCUdto item) => _repo.CreateAddressAsync(item);

    public Task<ResponsePageDto<IQuote>> ReadQuotesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadQuotesAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IQuote> ReadQuoteAsync(Guid id, bool flat) => _repo.ReadQuoteAsync(id, flat);
    public Task<IQuote> DeleteQuoteAsync(Guid id) => _repo.DeleteQuoteAsync(id);
    public Task<IQuote> UpdateQuoteAsync(QuoteCUdto item) => _repo.UpdateQuoteAsync(item);
    public Task<IQuote> CreateQuoteAsync(QuoteCUdto item) => _repo.CreateQuoteAsync(item);

    public Task<ResponsePageDto<IPet>> ReadPetsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadPetsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IPet> ReadPetAsync(Guid id, bool flat) => _repo.ReadPetAsync(id, flat);
    public Task<IPet> DeletePetAsync(Guid id) => _repo.DeletePetAsync(id);
    public Task<IPet> UpdatePetAsync(PetCUdto item) => _repo.UpdatePetAsync(item);
    public Task<IPet> CreatePetAsync(PetCUdto item) => _repo.CreatePetAsync(item);
    #endregion
}

