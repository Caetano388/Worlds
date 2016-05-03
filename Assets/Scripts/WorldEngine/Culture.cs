using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Culture : Synchronizable {

	[XmlIgnore]
	public World World;

	[XmlArrayItem(Type = typeof(CellCulturalActivity))]
	public List<CulturalActivity> Activities = new List<CulturalActivity> ();

	[XmlArrayItem(Type = typeof(BiomeSurvivalSkill)),
		XmlArrayItem(Type = typeof(SeafaringSkill))]
	public List<CulturalSkill> Skills = new List<CulturalSkill> ();
	
	[XmlArrayItem(Type = typeof(ShipbuildingKnowledge)),
		XmlArrayItem(Type = typeof(AgricultureKnowledge)),
		XmlArrayItem(Type = typeof(SocialOrganizationKnowledge))]
	public List<CulturalKnowledge> Knowledges = new List<CulturalKnowledge> ();
	
	[XmlArrayItem(Type = typeof(PolityCulturalDiscovery)),
		XmlArrayItem(Type = typeof(BoatMakingDiscovery)),
		XmlArrayItem(Type = typeof(SailingDiscovery)),
		XmlArrayItem(Type = typeof(TribalismDiscovery)),
		XmlArrayItem(Type = typeof(PlantCultivationDiscovery))]
	public List<CulturalDiscovery> Discoveries = new List<CulturalDiscovery> ();

	private Dictionary<string, CulturalActivity> _activities = new Dictionary<string, CulturalActivity> ();
	private Dictionary<string, CulturalSkill> _skills = new Dictionary<string, CulturalSkill> ();
	private Dictionary<string, CulturalKnowledge> _knowledges = new Dictionary<string, CulturalKnowledge> ();
	private Dictionary<string, CulturalDiscovery> _discoveries = new Dictionary<string, CulturalDiscovery> ();
	
	public Culture () {
	}

	public Culture (World world) {

		World = world;
	}

	protected void AddActivity (CulturalActivity activity) {

		if (_activities.ContainsKey (activity.Id))
			return;

		World.AddExistingCulturalActivityInfo (activity);

		Activities.Add (activity);
		_activities.Add (activity.Id, activity);
	}

	protected void RemoveActivity (CulturalActivity activity) {

		if (!_activities.ContainsKey (activity.Id))
			return;

		Activities.Remove (activity);
		_activities.Remove (activity.Id);
	}

	public void RemoveActivity (string activityId) {

		CulturalActivity activity = GetActivity (activityId);

		if (activity == null)
			return;

		RemoveActivity (activity);
	}
	
	protected void AddSkill (CulturalSkill skill) {

		if (_skills.ContainsKey (skill.Id))
			return;
		
		World.AddExistingCulturalSkillInfo (skill);

		Skills.Add (skill);
		_skills.Add (skill.Id, skill);
	}

	protected void RemoveSkill (CulturalSkill skill) {

		if (!_skills.ContainsKey (skill.Id))
			return;

		Skills.Remove (skill);
		_skills.Remove (skill.Id);
	}
	
	protected void AddKnowledge (CulturalKnowledge knowledge) {
		
		if (_knowledges.ContainsKey (knowledge.Id))
			return;
		
		World.AddExistingCulturalKnowledgeInfo (knowledge);

		Knowledges.Add (knowledge);
		_knowledges.Add (knowledge.Id, knowledge);
	}

	protected void RemoveKnowledge (CulturalKnowledge knowledge) {

		if (!_knowledges.ContainsKey (knowledge.Id))
			return;

		Knowledges.Remove (knowledge);
		_knowledges.Remove (knowledge.Id);
	}
	
	protected void AddDiscovery (CulturalDiscovery discovery) {
		
		if (_discoveries.ContainsKey (discovery.Id))
			return;
		
		World.AddExistingCulturalDiscoveryInfo (discovery);

		Discoveries.Add (discovery);
		_discoveries.Add (discovery.Id, discovery);
	}

	protected void RemoveDiscovery (CulturalDiscovery discovery) {

		if (!_discoveries.ContainsKey (discovery.Id))
			return;

		Discoveries.Remove (discovery);
		_discoveries.Remove (discovery.Id);
	}

	public CulturalActivity GetActivity (string id) {

		CulturalActivity activity = null;

		if (!_activities.TryGetValue (id, out activity))
			return null;

		return activity;
	}
	
	public CulturalSkill GetSkill (string id) {

		CulturalSkill skill = null;

		if (!_skills.TryGetValue (id, out skill))
			return null;
		
		return skill;
	}
	
	public CulturalKnowledge GetKnowledge (string id) {
		
		CulturalKnowledge knowledge = null;
		
		if (!_knowledges.TryGetValue (id, out knowledge))
			return null;
		
		return knowledge;
	}
	
	public CulturalDiscovery GetDiscovery (string id) {
		
		CulturalDiscovery discovery = null;
		
		if (!_discoveries.TryGetValue (id, out discovery))
			return null;
		
		return discovery;
	}

	public virtual void Synchronize () {
	}

	public virtual void FinalizeLoad () {

		Activities.ForEach (a => _activities.Add (a.Id, a));
		Skills.ForEach (s => _skills.Add (s.Id, s));
		Knowledges.ForEach (k => _knowledges.Add (k.Id, k));
		Discoveries.ForEach (d => _discoveries.Add (d.Id, d));
	}
}

public class PolityCulture : Culture {

	[XmlIgnore]
	public Polity Polity;

	public PolityCulture () {
	
	}

	public PolityCulture (Polity polity) : base (polity.World) {

		Polity = polity;

		#if DEBUG
		if (World.SelectedCell != null && 
			World.SelectedCell.Group != null) {

			if (World.SelectedCell.Group.GetPolityInfluenceValue (Polity) > 0) {

				Debug.Log ("Debug Selected");
			}
		}
		#endif

		CellGroup coreGroup = Polity.CoreGroup;
		CellCulture coreCulture = coreGroup.Culture;

		coreCulture.Activities.ForEach (a => AddActivity (new CulturalActivity (a)));
		coreCulture.Skills.ForEach (s => AddSkill (new CulturalSkill (s)));

		coreCulture.Knowledges.ForEach (k => {
			CulturalKnowledge knowledge = new CulturalKnowledge (k);
			AddKnowledge (knowledge);

			#if DEBUG
			if (float.IsNaN(knowledge.Value)) {

				Debug.Break ();
			}
			#endif
		});

		coreCulture.Discoveries.ForEach (d => {
			PolityCulturalDiscovery discovery = new PolityCulturalDiscovery (d);
			AddDiscovery (discovery);
			discovery.PresenceCount++;
		});
	}

	public void Update () {

		ResetAttributeValues ();

		AddGroupCultures ();

		NormalizeAttributeValues ();
	}

	private void ResetAttributeValues () {

		foreach (CulturalActivity activity in Activities) {

			activity.Value = 0;
			activity.Contribution = 0;
		}

		foreach (CulturalSkill skill in Skills) {

			skill.Value = 0;
		}

		foreach (CulturalKnowledge knowledge in Knowledges) {

			knowledge.Value = 0;
		}

		foreach (PolityCulturalDiscovery discovery in Discoveries) {

			discovery.PresenceCount = 0;
		}
	}

	private void NormalizeAttributeValues () {

		if (Polity.TotalGroupInfluenceValue <= 0)
			return;

		float totalGroupInfluenceValue = Polity.TotalGroupInfluenceValue;

		foreach (CulturalActivity activity in Activities) {

			activity.Value /= totalGroupInfluenceValue;
			activity.Contribution /= totalGroupInfluenceValue;
		}

		foreach (CulturalSkill skill in Skills) {

			skill.Value /= totalGroupInfluenceValue;
		}

		foreach (CulturalKnowledge knowledge in Knowledges) {

			knowledge.Value /= totalGroupInfluenceValue;
		}
	}

	private void AddGroupCultures () {
	
		foreach (CellGroup group in Polity.InfluencedGroups) {
		
			AddGroupCulture (group);
		}
	}

	private void AddGroupCulture (CellGroup group) {

		#if DEBUG
		if (World.SelectedCell != null && 
			World.SelectedCell.Group != null) {

			if (World.SelectedCell.Group.GetPolityInfluenceValue (Polity) > 0) {

				Debug.Log ("Debug Selected");
			}
		}
		#endif

		float influenceValue = group.GetPolityInfluenceValue (Polity);

		if (influenceValue <= 0) {

			throw new System.Exception ("Polity has zero or less influence value in group: " + influenceValue);
		}

		foreach (CulturalActivity groupActivity in group.Culture.Activities) {
		
			CulturalActivity activity = GetActivity (groupActivity.Id);

			if (activity == null) {
			
				activity = new CulturalActivity (groupActivity);
				activity.Value *= influenceValue;
				activity.Contribution *= influenceValue;

				AddActivity (activity);

			} else {
			
				activity.Value += groupActivity.Value * influenceValue;
				activity.Contribution += groupActivity.Contribution * influenceValue;
			}
		}

		foreach (CulturalSkill groupSkill in group.Culture.Skills) {

			CulturalSkill skill = GetSkill (groupSkill.Id);

			if (skill == null) {

				skill = new CulturalSkill (groupSkill);
				skill.Value *= influenceValue;

				AddSkill (skill);

			} else {

				skill.Value += groupSkill.Value * influenceValue;
			}
		}

		foreach (CulturalKnowledge groupKnowledge in group.Culture.Knowledges) {

			CulturalKnowledge knowledge = GetKnowledge (groupKnowledge.Id);

			if (knowledge == null) {

				knowledge = new CulturalKnowledge (groupKnowledge);
				knowledge.Value *= influenceValue;

				AddKnowledge (knowledge);

			} else {
				
				knowledge.Value += groupKnowledge.Value * influenceValue;
			}
		}

		foreach (CulturalDiscovery groupDiscovery in group.Culture.Discoveries) {

			PolityCulturalDiscovery discovery = GetDiscovery (groupDiscovery.Id) as PolityCulturalDiscovery;

			if (discovery == null) {

				discovery = new PolityCulturalDiscovery (groupDiscovery);

				AddDiscovery (discovery);
			}

			discovery.PresenceCount++;
		}
	}
}

public class CellCulture : Culture {
	
	public const float MinKnowledgeValue = 1f;
//	public const float BaseKnowledgeTransferFactor = 0.1f;

	[XmlIgnore]
	public CellGroup Group;

	[XmlIgnore]
	public Dictionary<string, CellCulturalActivity> ActivitiesToPerform = new Dictionary<string, CellCulturalActivity> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalSkill> SkillsToLearn = new Dictionary<string, CellCulturalSkill> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalKnowledge> KnowledgesToLearn = new Dictionary<string, CellCulturalKnowledge> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalDiscovery> DiscoveriesToFind = new Dictionary<string, CellCulturalDiscovery> ();

	private List<CellCulturalActivity> _activitiesToLose = new List<CellCulturalActivity> ();
	private List<CellCulturalSkill> _skillsToLose = new List<CellCulturalSkill> ();
	private List<CellCulturalKnowledge> _knowledgesToLose = new List<CellCulturalKnowledge> ();
	private List<CellCulturalDiscovery> _discoveriesToLose = new List<CellCulturalDiscovery> ();

	public CellCulture () {
	}

	public CellCulture (CellGroup group) : base (group.World) {

		Group = group;
	}

	public CellCulture (CellGroup group, CellCulture sourceCulture) : base (group.World) {

		Group = group;

		sourceCulture.Activities.ForEach (a => AddActivity (((CellCulturalActivity)a).GenerateCopy (group)));
		sourceCulture.Skills.ForEach (s => AddSkill (((CellCulturalSkill)s).GenerateCopy (group)));
		sourceCulture.Discoveries.ForEach (d => AddDiscovery (((CellCulturalDiscovery)d).GenerateCopy ()));
		sourceCulture.Knowledges.ForEach (k => AddKnowledge (((CellCulturalKnowledge)k).GenerateCopy (group)));
	}

	public void AddActivityToPerform (CellCulturalActivity activity) {

		if (ActivitiesToPerform.ContainsKey (activity.Id))
			return;

		ActivitiesToPerform.Add (activity.Id, activity);
	}
	
	public void AddSkillToLearn (CellCulturalSkill skill) {

		if (SkillsToLearn.ContainsKey (skill.Id))
			return;

		SkillsToLearn.Add (skill.Id, skill);
	}
	
	public void AddKnowledgeToLearn (CellCulturalKnowledge knowledge) {
		
		if (KnowledgesToLearn.ContainsKey (knowledge.Id))
			return;

		KnowledgesToLearn.Add (knowledge.Id, knowledge);
	}
	
	public void AddDiscoveryToFind (CellCulturalDiscovery discovery) {
		
		if (DiscoveriesToFind.ContainsKey (discovery.Id))
			return;
		
		DiscoveriesToFind.Add (discovery.Id, discovery);
	}
	
	public void MergeCulture (CellCulture sourceCulture, float percentage) {

		foreach (CellCulturalActivity a in sourceCulture.Activities) {

			CellCulturalActivity activity = GetActivity (a.Id) as CellCulturalActivity;

			if (activity == null) {
				activity = a.GenerateCopy (Group);
				activity.ModifyValue (percentage);

				AddActivityToPerform (activity);
			} else {
				activity.Merge (a, percentage);
			}
		}

		foreach (CellCulturalSkill s in sourceCulture.Skills) {
			
			CellCulturalSkill skill = GetSkill (s.Id) as CellCulturalSkill;
			
			if (skill == null) {
				skill = s.GenerateCopy (Group);
				skill.ModifyValue (percentage);
				
				AddSkillToLearn (skill);
			} else {
				skill.Merge (s, percentage);
			}
		}

		foreach (CellCulturalKnowledge k in sourceCulture.Knowledges) {
			
			CellCulturalKnowledge knowledge = GetKnowledge (k.Id) as CellCulturalKnowledge;
			
			if (knowledge == null) {
				knowledge = k.GenerateCopy (Group);
				knowledge.ModifyValue (percentage);
				
				AddKnowledgeToLearn (knowledge);
			} else {
				knowledge.Merge (k, percentage);
			}
		}

		foreach (CellCulturalDiscovery d in sourceCulture.Discoveries) {

			CellCulturalDiscovery discovery = GetDiscovery (d.Id) as CellCulturalDiscovery;
			
			if (discovery == null) {
				discovery = d.GenerateCopy ();
				
				AddDiscoveryToFind (discovery);
			}
		}
	}

	public void Update (int timeSpan) {

		float totalValue = 0;

		foreach (CellCulturalActivity activity in Activities) {

			activity.Update (timeSpan);
			totalValue += activity.Value;
		}

		foreach (CellCulturalActivity activity in Activities) {

			if (totalValue > 0) {
				activity.Contribution = activity.Value / totalValue;
			} else {
				activity.Contribution = 1f / Activities.Count;
			}
		}

		foreach (CellCulturalSkill skill in Skills) {
		
			skill.Update (timeSpan);
		}

		CulturalKnowledge[] knowledges = Knowledges.ToArray ();

		foreach (CellCulturalKnowledge knowledge in knowledges) {
			
			knowledge.Update (timeSpan);

			if (knowledge.WillBeLost ()) {

				_knowledgesToLose.Add (knowledge);
			}
		}

		CulturalDiscovery[] discoveries = Discoveries.ToArray ();
		
		foreach (CellCulturalDiscovery discovery in discoveries) {
			
			if (discovery.CanBeHeld (Group))
				continue;

			_discoveriesToLose.Add (discovery);
		}
	}

	public void PostUpdate () {

		bool discoveriesLost = false;

		_activitiesToLose.ForEach (a => RemoveActivity (a));
		_skillsToLose.ForEach (s => RemoveSkill (s));
		_knowledgesToLose.ForEach (k => {
			RemoveKnowledge (k);
			k.LossConsequences ();
		});
		_discoveriesToLose.ForEach (d => {
			RemoveDiscovery (d);
			d.LossConsequences (Group);
			discoveriesLost = true;
		});

		if (discoveriesLost) {
			foreach (CellCulturalKnowledge knowledge in Knowledges) {
				(knowledge as CellCulturalKnowledge).RecalculateAsymptote ();
			}
		}

		_activitiesToLose.Clear ();
		_skillsToLose.Clear ();
		_knowledgesToLose.Clear ();
		_discoveriesToLose.Clear ();

		foreach (CellCulturalActivity activity in ActivitiesToPerform.Values) {

			AddActivity (activity);
		}
		
		foreach (CellCulturalSkill skill in SkillsToLearn.Values) {
			
			AddSkill (skill);
		}
		
		foreach (CellCulturalKnowledge knowledge in KnowledgesToLearn.Values) {
			
			AddKnowledge (knowledge);

			knowledge.RecalculateAsymptote ();
		}
		
		foreach (CellCulturalDiscovery discovery in DiscoveriesToFind.Values) {
			
			if (!discovery.CanBeHeld (Group)) continue;
			
			AddDiscovery (discovery);

			foreach (CellCulturalKnowledge knowledge in Knowledges) {
				knowledge.CalculateAsymptote (discovery);
			}
		}

		ActivitiesToPerform.Clear ();
		SkillsToLearn.Clear ();
		KnowledgesToLearn.Clear ();
		DiscoveriesToFind.Clear ();
	}
	
//	public static float CalculateKnowledgeTransferValue (CellGroup sourceGroup, CellGroup targetGroup) {
//		
//		float maxTransferValue = 0;
//		
//		if (sourceGroup == null)
//			return maxTransferValue;
//		
//		if (targetGroup == null)
//			return maxTransferValue;
//		
//		if (!sourceGroup.StillPresent)
//			return maxTransferValue;
//		
//		if (!targetGroup.StillPresent)
//			return maxTransferValue;
//		
//		foreach (CulturalKnowledge sourceKnowledge in sourceGroup.Culture.Knowledges) {
//			
//			if (sourceKnowledge.Value <= MinKnowledgeValue) continue;
//			
//			CulturalKnowledge targetKnowledge = targetGroup.Culture.GetKnowledge (sourceKnowledge.Id);
//			
//			if (targetKnowledge == null) {
//				maxTransferValue = 1;
//			} else {
//				maxTransferValue = Mathf.Max (maxTransferValue, 1 - (targetKnowledge.Value / sourceKnowledge.Value));
//			}
//		}
//		
//		return maxTransferValue;
//	}
	
//	public void AbsorbKnowledgeFrom (CulturalKnowledge sourceKnowledge, float sourceFactor) {
//		
//		if (sourceKnowledge.Value < MinKnowledgeValue) return;
//		
//		CulturalKnowledge localKnowledge = GetKnowledge (sourceKnowledge.Id);
//		
//		if (localKnowledge == null) {
//			
//			localKnowledge = sourceKnowledge.GenerateCopy (Group, 0);
//			
//			AddKnowledgeToLearn (localKnowledge);
//		}
//		
//		if (localKnowledge.Value >= sourceKnowledge.Value) return;
//
//		float specificKnowledgeFactor = localKnowledge.CalculateTransferFactor ();
//		
//		localKnowledge.IncreaseValue (sourceKnowledge.Value, BaseKnowledgeTransferFactor * specificKnowledgeFactor * sourceFactor);
//	}
//	
//	public void AbsorbDiscoveryFrom (CulturalDiscovery sourceDiscovery) {
//		
//		CulturalDiscovery localDiscovery = GetDiscovery (sourceDiscovery.Id);
//		
//		if (localDiscovery == null) {
//			
//			localDiscovery = sourceDiscovery.GenerateCopy ();
//			
//			AddDiscoveryToFind (localDiscovery);
//		}
//	}
	
	public float MinimumSkillAdaptationLevel () {
		
		float minAdaptationLevel = 1f;
		
		foreach (CellCulturalSkill skill in Skills) {
			
			float level = skill.AdaptationLevel;

			if (level < minAdaptationLevel) {
				minAdaptationLevel = level;
			}
		}
		
		return minAdaptationLevel;
	}
	
	public float MinimumKnowledgeProgressLevel () {
		
		float minProgressLevel = 1f;
		
		foreach (CellCulturalKnowledge knowledge in Knowledges) {
			
			float level = knowledge.CalculateExpectedProgressLevel ();
			
			if (level < minProgressLevel) {
				minProgressLevel = level;
			}
		}
		
		return minProgressLevel;
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		foreach (CellCulturalActivity a in Activities) {
			a.Group = Group;
		}

		foreach (CellCulturalSkill s in Skills) {
			
			s.Group = Group;
			s.FinalizeLoad ();
		}
		foreach (CellCulturalKnowledge k in Knowledges) {
			
			k.Group = Group;
			k.FinalizeLoad ();
		}
	}
}
