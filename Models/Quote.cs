using Seido.Utilities.SeedGenerator;

namespace Models;

public class Quote : IQuote, ISeed<Quote>, IEquatable<Quote>
{
    public virtual Guid QuoteId { get; set; }
    public virtual string QuoteText { get; set; }
    public virtual string Author { get; set; }

    //One Quote can have many friends
    public virtual List<IFriend> Friends { get; set; } = null;

    public override string ToString() => $"{QuoteText} - {Author}";


    #region constructors
    public Quote() { }

    public Quote(SeededQuote goodQuote)
    {
        QuoteId = Guid.NewGuid();
        QuoteText = goodQuote.Quote;
        Author = goodQuote.Author;
        Seeded = true;
    }

    #endregion

    #region implementing IEquatable

    public bool Equals(Quote other) => (other != null) && ((QuoteText, Author) == (other.QuoteText, other.Author));
    public override bool Equals(object obj) => Equals(obj as Quote);
    public override int GetHashCode() => (QuoteText, Author).GetHashCode();

    #endregion

    #region randomly seed this instance
    public bool Seeded { get; set; } = false;

    public virtual Quote Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;
        QuoteId = Guid.NewGuid();

        var quote = seedGenerator.Quote;
        QuoteText = quote.Quote;
        Author = quote.Author;

        return this;
    }
    #endregion
}


