using System.Xml.Serialization;

public abstract class Identifiable
{
    [XmlAttribute("D")]
    public long InitDate;

    [XmlAttribute]
    public long Id;

    [XmlIgnore]
    public Identifier _uniqueIdentifier = null;

    //NOTE: max long: 9,223,372,036,854,775,807

    public Identifiable()
    {
    }

    public Identifiable(long date, long id)
    {
        InitDate = date;
        Id = id;
    }

    public string IdToString()
    {
        return InitDate.ToString("D19") + ":" + Id.ToString("D19");
    }

    public override bool Equals(object obj)
    {
        return obj is Identifiable ident &&
               InitDate == ident.InitDate &&
               Id == ident.Id;
    }

    public override int GetHashCode()
    {
        int hashCode = 1805739105;
        hashCode = hashCode * -1521134295 + InitDate.GetHashCode();
        hashCode = hashCode * -1521134295 + Id.GetHashCode();
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
                _uniqueIdentifier = new Identifier(InitDate, Id);
            }

            return _uniqueIdentifier;
        }
    }
}
