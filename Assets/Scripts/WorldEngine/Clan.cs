﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public const string ClanType = "Clan";

	[XmlIgnore]
	public ClanSplitEvent ClanSplitEvent;

	[XmlIgnore]
	public TribeSplitEvent TribeSplitEvent;

	public Clan () {

	}

	public Clan (CellGroup group, Polity polity, float prominence) : base (ClanType, group, polity, prominence) {

		if (ClanSplitEvent.CanBeAssignedTo (this)) {

			ClanSplitEvent = new ClanSplitEvent (this, ClanSplitEvent.CalculateTriggerDate (this));
			TribeSplitEvent = new TribeSplitEvent (this, TribeSplitEvent.CalculateTriggerDate (this));

			World.InsertEventToHappen (ClanSplitEvent);
			World.InsertEventToHappen (TribeSplitEvent);
		}
	}

	protected override void UpdateInternal () {
		
	}

	public override void RelocateCore ()
	{
		List<CellGroup> validCoreGroups = new List<CellGroup> (CoreCell.Neighbors.Count);

		foreach (TerrainCell cell in CoreCell.Neighbors.Values) {
		
			CellGroup group = cell.Group;

			if ((group != null) && (CanBeCore (group))) {
			
				validCoreGroups.Add (group);
			}
		}

		if (validCoreGroups.Count <= 0) {
		
			throw new System.Exception ("No valid core group to target");
		}

		int groupIndex = CoreCell.GetNextLocalRandomInt (RngOffsets.CLAN_CHOOSE_CORE_GROUP + (int)Polity.Id, validCoreGroups.Count);

		SetCoreGroup (validCoreGroups [groupIndex]);
	}

	public bool CanBeCore (CellGroup group) {

		return group.GetPolityInfluenceValue (Polity) > 0;
	}

	protected override void SetCoreGroupInternal (CellGroup coreGroup)
	{
		if (IsDominant) {

			Polity.SetCoreGroup (coreGroup);
		}
	}

	public override void GenerateName () {

		int rngOffset = RngOffsets.CLAN_GENERATE_NAME + (int)Polity.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => CoreGroup.GetNextLocalRandomInt (rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => CoreGroup.GetNextLocalRandomFloat (rngOffset++);

		Language language = Polity.Culture.Language;
		Region region = CoreGroup.Cell.Region;

		string untranslatedName = "";
		Language.NounPhrase namePhrase = null;

		if (region.Elements.Count <= 0) {

			throw new System.Exception ("No elements to choose name from");
		}

		List<RegionElement> remainingElements = new List<RegionElement> (region.Elements);

		bool addMoreWords = true;

		bool isPrimaryWord = true;
		float extraWordChange = 0.2f;

		while (addMoreWords) {

			addMoreWords = false;

			int index = getRandomInt (remainingElements.Count);

			RegionElement element = remainingElements [index];

			remainingElements.RemoveAt (index);

			if (isPrimaryWord) {
			
				untranslatedName = element.Name;
				isPrimaryWord = false;

			} else {
			
				untranslatedName = "[nad]" + element.Name + " " + untranslatedName;
			}

			namePhrase = language.TranslateNounPhrase (untranslatedName, getRandomFloat);

			bool canAddMoreWords = remainingElements.Count > 0;

			if (canAddMoreWords) {
			
				addMoreWords = extraWordChange > getRandomFloat ();
			}

			if ((!canAddMoreWords) || (!addMoreWords)) {
				
				foreach (Faction faction in Polity.GetFactions ()) {

					if (namePhrase.Text == faction.Name.Text) {
						addMoreWords = true;
						break;
					}
				}
			}

			if (addMoreWords && !canAddMoreWords) {
			
				throw new System.Exception ("Ran out of words to add");
			}

			extraWordChange /= 2f;
		}

		Name = new Name (namePhrase, untranslatedName, language, World);
	}
}

public class ClanSplitEvent : FactionEvent {

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 1000;

	public const int MuAdministrativeLoadValue = 100;

	public const string EventSetFlag = "ClanSplitEvent_Set";

	public ClanSplitEvent () {

	}

	public ClanSplitEvent (Clan clan, int triggerDate) : base (clan, triggerDate, ClanSplitEventId) {

		clan.SetFlag (EventSetFlag);
	}

	private static float CalculateClanAdministrativeLoadFactor (Clan clan) {

		float socialOrganizationValue = 0;

		CellGroup clanGroup = clan.CoreGroup;

		CulturalKnowledge socialOrganizationKnowledge = clanGroup.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		if (socialOrganizationValue <= 0) {
		
			return float.MaxValue;
		}

		float administrativeLoad = clan.Polity.TotalAdministrativeCost * clan.Prominence;

		return administrativeLoad / socialOrganizationValue;
	}

	public static int CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		int targetDate = (int)(clan.World.CurrentDate + dateSpan);

		if (targetDate <= clan.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanBeAssignedTo (Clan clan) {

		if (clan.IsFlagSet (EventSetFlag))
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		float administrativeLoadFactor = CalculateClanAdministrativeLoadFactor (Faction as Clan);
		administrativeLoadFactor *= administrativeLoadFactor;

		if (administrativeLoadFactor < 0)
			return true;

		administrativeLoadFactor = administrativeLoadFactor / (0.001f + administrativeLoadFactor + MuAdministrativeLoadValue);

		float triggerValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (triggerValue > administrativeLoadFactor)
			return false;

		return true;
	}

	public override void Trigger () {

		CellGroup targetGroup = Polity.GetRandomGroup (RngOffsets.EVENT_TRIGGER + (int)Id, (group) => group.Population * group.GetPolityInfluenceValue (Polity));

		#if DEBUG
		if (targetGroup == null) {
			throw new System.Exception ("target group is null");
		}
		#endif

		float randomValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + 1 + (int)Id);
		float randomFactor = 0.25f + randomValue * 0.5f;

		float oldProminence = Faction.Prominence;

		Faction.Prominence = oldProminence * randomFactor;

		float newClanProminence = oldProminence * (1f - randomFactor);

		Polity.AddFaction (new Clan (targetGroup, Polity as Tribe, newClanProminence));

		World.AddPolityToUpdate (Polity);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			if (CanBeAssignedTo (clan)) {

				clan.ClanSplitEvent = this;

				Reset (CalculateTriggerDate (clan));

				World.InsertEventToHappen (this);
			}
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Clan clan = Faction as Clan;

		clan.ClanSplitEvent = this;
	}
}

public class TribeSplitEvent : FactionEvent {

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 1000;

	public const int MuAdministrativeLoadValue = 500;

	public const string EventSetFlag = "ClanSplitEvent_Set";

	public TribeSplitEvent () {

	}

	public TribeSplitEvent (Clan clan, int triggerDate) : base (clan, triggerDate, ClanSplitEventId) {

		clan.SetFlag (EventSetFlag);
	}

	private static float CalculateTribeAdministrativeLoadFactor (Tribe tribe) {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = tribe.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		if (socialOrganizationValue <= 0) {

			return float.MaxValue;
		}

		float administrativeLoad = tribe.TotalAdministrativeCost;

		return administrativeLoad / socialOrganizationValue;
	}

	public static int CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		int targetDate = (int)(clan.World.CurrentDate + dateSpan);

		if (targetDate <= clan.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanBeAssignedTo (Clan clan) {

		if (clan.IsFlagSet (EventSetFlag))
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Faction.IsDominant)
			return false;

		float administrativeLoadFactor = CalculateTribeAdministrativeLoadFactor (Polity as Tribe);
		administrativeLoadFactor = Mathf.Pow (administrativeLoadFactor, 2);

		if (administrativeLoadFactor < 0)
			return true;

		administrativeLoadFactor = Faction.Prominence * administrativeLoadFactor / (0.001f + administrativeLoadFactor + MuAdministrativeLoadValue);

		float triggerValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (triggerValue > administrativeLoadFactor)
			return false;

		return true;
	}

	public override void Trigger () {

		Tribe tribe = new Tribe (Faction as Clan);

		World.AddPolity (tribe);
		World.AddPolityToUpdate (tribe);

		World.AddGroupToUpdate (Faction.CoreGroup);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			if (CanBeAssignedTo (clan)) {

				clan.TribeSplitEvent = this;

				Reset (CalculateTriggerDate (clan));

				World.InsertEventToHappen (this);
			}
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Clan clan = Faction as Clan;

		clan.TribeSplitEvent = this;
	}
}
