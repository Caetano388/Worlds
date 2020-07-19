﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;
using System.Linq;
using System.Xml.Schema;

public enum PolityType
{
    None,
    Tribe
}

public class PolityInfo : Identifiable
{
    [XmlAttribute("T")]
	public string Type;

    public Name Name;
    
    public Polity Polity;

    public long FormationDate => InitDate;

    private string _nameFormat;

    private PolityType _type;

    public PolityInfo()
    {
	
	}

	public PolityInfo(Polity polity, string type, long initDate, long initId) :
        base(initDate, initId)
    {
        Polity = polity;

        SetType(type);
    }

    public static PolityType GetPolityType(string typeStr)
    {
        switch (typeStr)
        {
            case Tribe.PolityTypeStr:
                return PolityType.Tribe;
            default:
                throw new System.Exception("PolityInfo: Unrecognized polity type: " + typeStr);
        }
    }

    private void SetType(string typeStr)
    {
        Type = typeStr;

        _type = GetPolityType(typeStr);

        switch (_type)
        {
            case PolityType.Tribe:
                _type = PolityType.Tribe;
                _nameFormat = Tribe.PolityNameFormat;
                break;
            default:
                throw new System.Exception("PolityInfo: Unhandled polity type: " + _type);
        }
    }

    public string GetNameAndTypeString()
    {
        return string.Format(_nameFormat, Name);
    }

    public string GetNameAndTypeStringBold()
    {
        return string.Format(_nameFormat, Name.BoldText);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        if (Polity != null)
            Polity.FinalizeLoad();
        
        SetType(Type);
    }

    public override void Synchronize()
    {
        if (Polity != null)
            Polity.Synchronize();
    }
}
