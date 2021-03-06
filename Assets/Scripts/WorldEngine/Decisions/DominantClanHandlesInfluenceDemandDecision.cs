using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DominantClanHandlesInfluenceDemandDecision : FactionDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinInfluencePercentChange = 0.05f;
	public const float BaseMaxInfluencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _acceptDemand = true;

	private Clan _dominantClan;
	private Clan _demandClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan demandClan, Clan dominantClan) {

		return 
			demandClan.CurrentLeader.Name.BoldText + ", leader of clan " + demandClan.Name.BoldText + ", has demanded greater influence over the " + tribe.Name.BoldText + 
			" tribe at the expense of clan " + dominantClan.Name.BoldText + ".\n\n";
	}

	public DominantClanHandlesInfluenceDemandDecision (Tribe tribe, Clan demandClan, Clan dominantClan, long eventId) : base (dominantClan, eventId) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_demandClan = demandClan;

		Description = GenerateDescriptionIntro (tribe, demandClan, dominantClan) +
			"Unfortunately, the situation is beyond control for the tribe leader, " + dominantClan.CurrentLeader.Name.BoldText + ", to be able to do anything other than accept " +
			"clan " + demandClan.Name.BoldText + "'s demand...";

		_cantPrevent = true;
	}

	public DominantClanHandlesInfluenceDemandDecision (Tribe tribe, Clan demandClan, Clan dominantClan, bool acceptDemand, long eventId) : base (dominantClan, eventId) {

		_tribe = tribe;

		_dominantClan = dominantClan;
		_demandClan = demandClan;

		Description = GenerateDescriptionIntro (tribe, demandClan, dominantClan) +
			"Should the leader of clan " + dominantClan.Name.BoldText + ", " + _dominantClan.CurrentLeader.Name.BoldText + ", accept the demans from clan " + demandClan.Name.BoldText + "?";

		_acceptDemand = acceptDemand;
	}

	private string GenerateRejectDemandEffectsMessage () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_dominantClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_DecreaseRelationship (_dominantClan, _demandClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange);
	}

	public static void LeaderRejectsDemand_notifyDemandClan (Clan demandClan, Clan dominantClan, Tribe tribe, long eventId) {

		World world = tribe.World;

		if (tribe.IsUnderPlayerFocus || demandClan.IsUnderPlayerGuidance) {

			Decision decision = new RejectedClanInfluenceDemandDecision (tribe, demandClan, dominantClan, eventId); // Notify player that tribe leader rejected demand

			if (demandClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (decision);

			} else {

				decision.ExecutePreferredOption ();
			}

		} else {

			RejectedClanInfluenceDemandDecision.DominantClanRejectedDemand (demandClan, dominantClan, tribe);
		}
	}

	public static void LeaderRejectsDemand (Clan demandClan, Clan dominantClan, Tribe tribe, long eventId) {

		int rngOffset = RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_DOMINANTCLAN_LEADER_REJECTS_DEMAND_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (dominantClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_DecreaseRelationship (dominantClan, demandClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange, rngOffset++);

		LeaderRejectsDemand_notifyDemandClan (demandClan, dominantClan, tribe, eventId);
	}

	private void RejectDemand () {

		LeaderRejectsDemand (_demandClan, _dominantClan, _tribe, _eventId);
	}

	private string GenerateAcceptDemandEffectsMessage () {

		string dominantClanInfluenceEffectString;
		string demandClanInfluenceEffectString;

		GenerateEffectsString_TransferInfluence (
			_dominantClan, _demandClan, _tribe, BaseMinInfluencePercentChange, BaseMaxInfluencePercentChange, out dominantClanInfluenceEffectString, out demandClanInfluenceEffectString);

		return 
			"\t• " + dominantClanInfluenceEffectString + "\n" +
			"\t• " + demandClanInfluenceEffectString + "\n" +
			"\t• " + GenerateEffectsString_DecreasePreference (_dominantClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_IncreaseRelationship (_dominantClan, _demandClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange);
	}

	public static void LeaderAcceptsDemand_notifyDemandClan (Clan demandClan, Clan dominantClan, Tribe tribe, long eventId) {

		World world = tribe.World;

		if (tribe.IsUnderPlayerFocus || demandClan.IsUnderPlayerGuidance) {

			Decision decision = new AcceptedClanInfluenceDemandDecision (tribe, demandClan, dominantClan, eventId); // Notify player that tribe leader acepted demand

			if (demandClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (decision);

			} else {

				decision.ExecutePreferredOption ();
			}

		} else {

			AcceptedClanInfluenceDemandDecision.DominantClanAcceptedDemand (demandClan, dominantClan, tribe);
		}
	}

	public static void LeaderAcceptsDemand (Clan demandClan, Clan dominantClan, Tribe tribe, long eventId) {

		int rngOffset = RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_DOMINANTCLAN_LEADER_ACCEPTS_DEMAND_MODIFY_ATTRIBUTE;

		Effect_TransferInfluence (dominantClan, demandClan, BaseMinInfluencePercentChange, BaseMaxInfluencePercentChange, rngOffset++);
		Effect_DecreasePreference (dominantClan, CulturalPreference.AuthorityPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_IncreaseRelationship (dominantClan, demandClan, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange, rngOffset++);

		demandClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		LeaderAcceptsDemand_notifyDemandClan (demandClan, dominantClan, tribe, eventId);
	}

	private void AcceptDemand () {

		LeaderAcceptsDemand (_demandClan, _dominantClan, _tribe, _eventId);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {

			return new Option[] {
				new Option ("Oh well...", "Effects:\n" + GenerateAcceptDemandEffectsMessage (), AcceptDemand),
			};
		}

		return new Option[] {
			new Option ("Accept demand...", "Effects:\n" + GenerateAcceptDemandEffectsMessage (), AcceptDemand),
			new Option ("Reject demand...", "Effects:\n" + GenerateRejectDemandEffectsMessage (), RejectDemand)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_acceptDemand)
			AcceptDemand ();
		else
			RejectDemand ();
	}
}
	