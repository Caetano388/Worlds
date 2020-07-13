using System.Xml.Serialization;

public class Identifier : Identifiable
{
    public Identifier()
    {
    }

    public Identifier(long date, long id) : base(date, id)
    {
    }

    public override Identifier UniqueIdentifier => this;
}
