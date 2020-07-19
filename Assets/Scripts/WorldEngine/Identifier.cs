using System.Xml.Serialization;

public class Identifier : Identifiable
{
    public Identifier()
    {
    }

    public Identifier(long date, long id) : base(date, id)
    {
    }

    public Identifier(Identifiable identifiable) : base(identifiable.InitDate, identifiable.InitId)
    {
    }

    public Identifier(string idString)
    {
        string[] parts = idString.Split(':');

        if (!long.TryParse(parts[0], out long date))
            throw new System.ArgumentException("Not a valid date part: " + parts[0]);

        if (!long.TryParse(parts[1], out long id))
            throw new System.ArgumentException("Not a valid id part: " + parts[1]);

        Init(date, id);
    }

    public override Identifier UniqueIdentifier => this;
}
