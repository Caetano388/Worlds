using System.Xml.Serialization;

public abstract class Identifiable
{
    // This property is to be used exclusively for XML serialization.
    // It needs to be public. But consider it 'read-only protected' and don't access directly
    [XmlAttribute("D")]
    public long InitDate;

    // This property is to be used exclusively for XML serialization.
    // It needs to be public. But consider it private and don't access it ever
    [XmlAttribute("Id")]
    public long InitId; 

    [XmlIgnore]
    public Identifier _uniqueIdentifier = null;

    //NOTE: max long: 9,223,372,036,854,775,807

    public Identifiable()
    {
    }

    public Identifiable(long date, long id)
    {
        Init(date, id);
    }

    public Identifiable(Identifiable identifiable)
    {
        Init(identifiable);
    }

    protected void Init(Identifiable identifiable)
    {
        InitDate = identifiable.InitDate;
        InitId = identifiable.InitId;
    }

    protected void Init(long date, long id)
    {
        InitDate = date;
        InitId = id;
    }

    public override string ToString()
    {
        return InitDate.ToString("D19") + ":" + InitId.ToString("D19");
    }

    public override bool Equals(object obj)
    {
        return obj is Identifiable ident &&
               InitDate == ident.InitDate &&
               InitId == ident.InitId;
    }

    public bool Equals(string idString)
    {
        string[] parts = idString.Split(':');

        if (!long.TryParse(parts[0], out long date)) return false;

        if (!long.TryParse(parts[1], out long id)) return false;

        return (InitDate == date) && (InitId == id);
    }

    public override int GetHashCode()
    {
        int hashCode = 1805739105;
        hashCode = hashCode * -1521134295 + InitDate.GetHashCode();
        hashCode = hashCode * -1521134295 + InitId.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(Identifiable left, Identifiable right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Identifiable left, Identifiable right)
    {
        return !(left == right);
    }

    public virtual Identifier UniqueIdentifier
    {
        get {
            if (_uniqueIdentifier == null)
            {
                _uniqueIdentifier = new Identifier(InitDate, InitId);
            }

            return _uniqueIdentifier;
        }
    }
}
