using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class HandleMergeTribesOfferDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.05f;
	public const float BaseMaxPreferencePercentChange = 0.15f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	private bool _acceptOffer = true;

	private Tribe _targetTribe;
	private Tribe _sourceTribe;

	private static string GenerateDescriptionIntro (Tribe sourceTribe, Tribe targetTribe) {

		return 
			sourceTribe.CurrentLeader.Name.BoldText + ", leader of " + sourceTribe.GetNameAndTypeStringBold () + ", is proposing " + 
			targetTribe.GetNameAndTypeStringBold () + " merge into " + sourceTribe.CurrentLeader.PossessiveNoun + " tribe.\n\n" +
			"By accepting this proposal, " + targetTribe.GetNameAndTypeStringBold () + " will disappear.\n\n";
	}

	public HandleMergeTribesOfferDecision (Tribe sourceTribe, Tribe targetTribe, bool acceptOffer, long eventId) : base (sourceTribe, eventId) {

		_sourceTribe = sourceTribe;
		_targetTribe = targetTribe;

		Description = GenerateDescriptionIntro (sourceTribe, targetTribe) +
			"Should the leader of " + _targetTribe.GetNameAndTypeStringBold () + ", " + _targetTribe.CurrentLeader.Name.BoldText + ", allow " +
			_targetTribe.CurrentLeader.PossessiveNoun + " tribe to merge into " + sourceTribe.GetNameAndTypeStringBold () + "?";

		_acceptOffer = acceptOffer;
	}

	private string GenerateRejectOfferEffectsMessage () {

		return 
			"\t• " + GenerateEffectsString_IncreasePreference (_targetTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_DecreaseRelationship (_targetTribe, _sourceTribe, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange);
	}

	public static void LeaderRejectsOffer_notifySourceTribe (Tribe sourceTribe, Tribe targetTribe, long eventId) {

		World world = targetTribe.World;

		Clan sourceDominantClan = sourceTribe.DominantFaction as Clan;

		if (sourceTribe.IsUnderPlayerFocus || sourceDominantClan.IsUnderPlayerGuidance) {

			Decision decision = new RejectedMergeTribesOfferDecision (sourceTribe, targetTribe, eventId); // Notify player that tribe leader rejected offer

			if (sourceDominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (decision);

			} else {

				decision.ExecutePreferredOption ();
			}

		} else {

			RejectedMergeTribesOfferDecision.TargetTribeRejectedOffer (sourceTribe, targetTribe);
		}
	}

	public static void LeaderRejectsOffer (Tribe sourceTribe, Tribe targetTribe, long eventId) {

		int rngOffset = RngOffsets.MERGE_TRIBES_EVENT_TARGETTRIBE_LEADER_REJECTS_OFFER_MODIFY_ATTRIBUTE;

		Effect_IncreasePreference (targetTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_DecreaseRelationship (targetTribe, sourceTribe, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange, rngOffset++);

		LeaderRejectsOffer_notifySourceTribe (sourceTribe, targetTribe, eventId);
	}

	private void RejectOffer () {

		LeaderRejectsOffer (_sourceTribe, _targetTribe, _eventId);
	}

	private string GenerateAcceptOfferEffectsMessage () {

		return 
			"\t• " + GenerateEffectsString_DecreasePreference (_targetTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange) + "\n" +
			"\t• " + GenerateEffectsString_IncreaseRelationship (_targetTribe, _sourceTribe, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange) + "\n" +
			"\t• " + _targetTribe.GetNameAndTypeStringBold () + " will merge into " + _sourceTribe.GetNameAndTypeStringBold ();
	}

	public static void LeaderAcceptsOffer_notifySourceTribe (Tribe sourceTribe, Tribe targetTribe, long eventId) {

		World world = sourceTribe.World;

		Clan sourceDominantClan = sourceTribe.DominantFaction as Clan;

		if (sourceTribe.IsUnderPlayerFocus || sourceDominantClan.IsUnderPlayerGuidance) {

			Decision decision = new AcceptedMergeTribesOfferDecision (sourceTribe, targetTribe, eventId); // Notify player that tribe leader acepted offer

			if (sourceDominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (decision);

			} else {

				decision.ExecutePreferredOption ();
			}

		} else {

			AcceptedMergeTribesOfferDecision.TargetTribeAcceptedOffer (sourceTribe, targetTribe);
		}
	}

	public static void LeaderAcceptsOffer (Tribe sourceTribe, Tribe targetTribe, long eventId) {

//#if DEBUG
//        Debug.Log(string.Format("targetTribe ({0}) has accepted offer from sourceTribe ({1}) ", targetTribe.Id, sourceTribe.Id));
//        Debug.Log(string.Format("targetTribe ({0}) - coregroup location: {1}", targetTribe.Id, sourceTribe.CoreGroup.Position));
//        //Debug.Break();
//#endif

        int rngOffset = RngOffsets.MERGE_TRIBES_EVENT_TARGETTRIBE_LEADER_ACCEPTS_OFFER_MODIFY_ATTRIBUTE;

		Effect_DecreasePreference (targetTribe, CulturalPreference.IsolationPreferenceId, BaseMinPreferencePercentChange, BaseMaxPreferencePercentChange, rngOffset++);
		Effect_IncreaseRelationship (targetTribe, sourceTribe, BaseMinRelationshipPercentChange, BaseMaxRelationshipPercentChange, rngOffset++);

		sourceTribe.MergePolity (targetTribe);

		sourceTribe.DominantFaction.SetToUpdate ();
//		targetTribe.DominantFaction.SetToUpdate ();

		LeaderAcceptsOffer_notifySourceTribe (sourceTribe, targetTribe, eventId);
	}

	private void AcceptOffer () {

		LeaderAcceptsOffer (_sourceTribe, _targetTribe, _eventId);
	}

	public override Option[] GetOptions () {

		return new Option[] {
			new Option ("Accept offer...", "Effects:\n" + GenerateAcceptOfferEffectsMessage (), AcceptOffer),
			new Option ("Reject offer...", "Effects:\n" + GenerateRejectOfferEffectsMessage (), RejectOffer)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_acceptOffer)
			AcceptOffer ();
		else
			RejectOffer ();
	}
}
	