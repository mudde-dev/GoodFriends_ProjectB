using System;

namespace Models;

public interface IQuote
{
    public Guid QuoteId { get; set; }

    public string QuoteText { get; set; }
    public string Author { get; set; }

    public List<IFriend> Friends { get; set; }
}


