using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalDiscoveryInfo {
	
	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;

	public CulturalDiscoveryInfo () {
	}
	
	public CulturalDiscoveryInfo (string id, string name) {
		
		Id = id;
		
		Name = name;
	}
	
	public CulturalDiscoveryInfo (CulturalDiscoveryInfo baseDiscovery) {
		
		Id = baseDiscovery.Id;
		
		Name = baseDiscovery.Name;
	}
}

public abstract class CulturalDiscovery : CulturalDiscoveryInfo {
	
	public CulturalDiscovery (string id, string name) : base (id, name) {

	}
	
	public CulturalDiscovery GenerateCopy () {
		
		System.Type discoveryType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = discoveryType.GetConstructor (new System.Type[] {});
		
		return cInfo.Invoke (new object[] {}) as CulturalDiscovery;
	}

	public abstract bool CanBeHeld (CellGroup group);
}

public class BoatMakingDiscovery : CulturalDiscovery {

	public const string BoatMakingDiscoveryId = "BoatMakingDiscovery";
	public const string BoatMakingDiscoveryName = "Boat Making";
	
	public BoatMakingDiscovery () : base (BoatMakingDiscoveryId, BoatMakingDiscoveryName) {

	}

	public override bool CanBeHeld (CellGroup group)
	{
		CulturalKnowledge knowledge = group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);

		if (knowledge == null)
			return false;

		if (knowledge.Value < 0)
			return false;

		return true;
	}
}

public class SailingDiscovery : CulturalDiscovery {

	public const float MinShipBuildingKnowledgeValue = 3;
	public const float OptimalShipBuildingKnowledgeValue = 10;
	
	public const string SailingDiscoveryId = "SailingDiscovery";
	public const string SailingDiscoveryName = "Sailing";
	
	public SailingDiscovery () : base (SailingDiscoveryId, SailingDiscoveryName) {
		
	}
	
	public override bool CanBeHeld (CellGroup group)
	{
		CulturalKnowledge knowledge = group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);
		
		if (knowledge == null)
			return false;
		
		if (knowledge.Value < MinShipBuildingKnowledgeValue)
			return false;
		
		return true;
	}
}

public class PlantCultivationDiscovery : CulturalDiscovery {

	public const string PlantCultivationDiscoveryId = "PlantCultivationDiscovery";
	public const string PlantCultivationDiscoveryName = "Plant Cultivation";

	public PlantCultivationDiscovery () : base (PlantCultivationDiscoveryId, PlantCultivationDiscoveryName) {

	}

	public override bool CanBeHeld (CellGroup group)
	{
//		CulturalKnowledge knowledge = group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);
//
//		if (knowledge == null)
//			return false;
//
//		if (knowledge.Value < 0)
//			return false;

		return true;
	}
}
