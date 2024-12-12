using System;
using Models;
using Models.DTO;

namespace Services;

public interface IFriendsService
{    
    //Full set of async methods
    public Task<GstUsrInfoAllDto> InfoAsync { get; }
    public Task<GstUsrInfoAllDto> SeedAsync(int nrOfItems);
    public Task<GstUsrInfoAllDto> RemoveSeedAsync(bool seeded);

    public Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IFriend> ReadFriendAsync(Guid id, bool flat);
    public Task<IFriend> DeleteFriendAsync(Guid id);
    public Task<IFriend> UpdateFriendAsync(FriendCUdto item);
    public Task<IFriend> CreateFriendAsync(FriendCUdto item);

    public Task<ResponsePageDto<IAddress>> ReadAddressesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IAddress> ReadAddressAsync(Guid id, bool flat);
    public Task<IAddress> DeleteAddressAsync(Guid id);
    public Task<IAddress> UpdateAddressAsync(AddressCUdto item);
    public Task<IAddress> CreateAddressAsync(AddressCUdto item);

    public Task<ResponsePageDto<IQuote>> ReadQuotesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IQuote> ReadQuoteAsync(Guid id, bool flat);
    public Task<IQuote> DeleteQuoteAsync(Guid id);
    public Task<IQuote> UpdateQuoteAsync(QuoteCUdto item);
    public Task<IQuote> CreateQuoteAsync(QuoteCUdto item);

    public Task<ResponsePageDto<IPet>> ReadPetsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IPet> ReadPetAsync(Guid id, bool flat);
    public Task<IPet> DeletePetAsync(Guid id);
    public Task<IPet> UpdatePetAsync(PetCUdto item);
    public Task<IPet> CreatePetAsync(PetCUdto item);
}


