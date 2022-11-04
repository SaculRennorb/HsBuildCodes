using Gw2Sharp.WebApi.V2.Models;

using static Hardstuck.GuildWars2.BuildCodes.V2.Static;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public static class APILoader {

	/// <summary> Produces a list of token scopes that are missing. </summary>
	public static async Task<IEnumerable<TokenPermission>> ValidateScopes(Gw2Sharp.Gw2Client client)
	{
		var tokenInfo = await client.WebApi.V2.TokenInfo.GetAsync();

		var required = new[] { TokenPermission.Account, TokenPermission.Characters, TokenPermission.Builds };
		return required.Except(tokenInfo.Permissions.List.Select(e => e.Value));
	}

	/// <summary> This method assumes the scopes account, character and build are available. </summary>
	public static BuildCode LoadBuildCode(string authToken, string characterName, Kind targetGameMode, bool aquatic = false)
	{
		var connection = new Gw2Sharp.Connection(authToken);
		using var client = new Gw2Sharp.Gw2Client(connection);
		return LoadBuildCode(client, characterName, targetGameMode, aquatic);
	}

	/// <summary> This method assumes the scopes account, character and build are available, but does not explicitely test for them. </summary>
	public static BuildCode LoadBuildCode(Gw2Sharp.Gw2Client authorizedClient, string characterName, Kind targetGameMode, bool aquatic = false) {
		var code = new BuildCode();
		code.Version = CURRENT_VERSION;
		code.Kind    = targetGameMode;

		var playerData = authorizedClient.WebApi.V2.Characters.GetAsync(characterName).Result;
		
		code.Profession = Enum.Parse<Profession>(playerData.Profession);

		var activeBuild = playerData.BuildTabs![playerData.ActiveBuildTab!.Value - 1].Build;
		for(var i = 0; i < 3; i++) {
			var spec = activeBuild.Specializations[i];
			if(spec == null) continue;

			code.Specializations[i] = new() {
				SpecializationId = (SpecializationId)spec.Id!, //todo test
				Choices          = {
					Adept       = APICache.ResolvePosition(spec.Traits[0]), //todo translate
					Master      = APICache.ResolvePosition(spec.Traits[1]), //todo translate
					Grandmaster = APICache.ResolvePosition(spec.Traits[2]), //todo translate
				},
			};
		}

		var activeEquipment = playerData.EquipmentTabs![playerData.ActiveEquipmentTab!.Value - 1];
		if(targetGameMode != Kind.PvP)
		{
			int? runeId = null;

			void SetArmorData(int equipSlot, CharacterEquipmentItem item)
			{
				code.EquipmentAttributes[equipSlot] = ResolveStatId(item);
				code.Infusions          [equipSlot] = item.Infusions?[0];
				if(item.Upgrades != null) {
					if(runeId == null) runeId = item.Upgrades[0];
					else if(runeId != item.Upgrades[0]) runeId = 0;
				}
			}

			foreach(var item in activeEquipment.Equipment!) {
				switch(item.Slot.Value)
				{
					case ItemEquipmentSlotType.Helm       : if( aquatic) break; SetArmorData(0, item); break;
					case ItemEquipmentSlotType.HelmAquatic: if(!aquatic) break; SetArmorData(0, item); break;
					case ItemEquipmentSlotType.Shoulders  :                     SetArmorData(1, item); break;
					case ItemEquipmentSlotType.Coat       :                     SetArmorData(2, item); break;
					case ItemEquipmentSlotType.Gloves     :                     SetArmorData(3, item); break;
					case ItemEquipmentSlotType.Leggings   :                     SetArmorData(4, item); break;
					case ItemEquipmentSlotType.Boots      :                     SetArmorData(5, item); break;
						
					case ItemEquipmentSlotType.Backpack:
						code.EquipmentAttributes.BackItem = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.BackItem_1 = item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.BackItem_1 = item.Infusions[1];
						}
						break;

					case ItemEquipmentSlotType.Accessory1:
						code.EquipmentAttributes.Accessory1 = ResolveStatId(item);
						code.Infusions          .Accessory1 = item.Infusions?[0];
						break;

					case ItemEquipmentSlotType.Accessory2:
						code.EquipmentAttributes.Accessory2 = ResolveStatId(item);
						code.Infusions          .Accessory2 = item.Infusions?[0];
						break;

					case ItemEquipmentSlotType.Ring1:
						code.EquipmentAttributes.Ring1 = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.Ring1_1 = item.Infusions[0];
							if(item.Infusions.Count > 1) {
								code.Infusions.Ring1_2 = item.Infusions[1];
								if(item.Infusions.Count > 2)
									code.Infusions.Ring1_3 = item.Infusions[2];
							}
						}
						break;
						
					case ItemEquipmentSlotType.Ring2:
						code.EquipmentAttributes.Ring2 = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.Ring2_1 = item.Infusions[0];
							if(item.Infusions.Count > 1) {
								code.Infusions.Ring2_2 = item.Infusions[1];
								if(item.Infusions.Count > 2)
									code.Infusions.Ring2_3 = item.Infusions[2];
							}
						}
						break;
						
					case ItemEquipmentSlotType.WeaponA1:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet1MainHand = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet1_1 = item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet1_2 = item.Infusions[1];
						}
						code.WeaponSet1.MainHand = APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet1.Sigil1 = item.Upgrades[0];
							if(item.Upgrades.Count > 1)
								code.WeaponSet1.Sigil2 = item.Upgrades[1];
						}
						break;

					case ItemEquipmentSlotType.WeaponAquaticA:
						if(!aquatic) break;
						code.EquipmentAttributes.WeaponSet1MainHand = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet1_1 = item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet1_2 = item.Infusions[1];
						}
						code.WeaponSet1.MainHand = APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet1.Sigil1 = item.Upgrades[0];
							if(item.Upgrades.Count > 1)
								code.WeaponSet1.Sigil2 = item.Upgrades[1];
						}
						break;

					case ItemEquipmentSlotType.WeaponA2:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet1OffHand = ResolveStatId(item);
						code.Infusions.WeaponSet1_2 = item.Infusions?[0]; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						code.WeaponSet1.OffHand = APICache.ResolveWeaponType(item.Id);
						code.WeaponSet1.Sigil2 = item.Upgrades?[0]; //NOTE(Rennorb): this assues that buidls with twohanded main weapons dont contain an 'empty' weapon with no upgrades
						break;

					case ItemEquipmentSlotType.WeaponB1:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet2MainHand = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet2_1 = item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet2_2 = item.Infusions[1];
						}
						code.WeaponSet2.MainHand = APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet2.Sigil1 = item.Upgrades[0];
							if(IsTwoHanded(code.WeaponSet2.MainHand.Value) && item.Upgrades.Count > 1)
								code.WeaponSet2.Sigil2 = item.Upgrades[1];
						}
						break;

					case ItemEquipmentSlotType.WeaponAquaticB:
						if(!aquatic) break;
						code.EquipmentAttributes.WeaponSet2MainHand = ResolveStatId(item);
						if(item.Infusions != null) {
							code.Infusions.WeaponSet2_1 = item.Infusions[0];
							if(item.Infusions.Count > 1)
								code.Infusions.WeaponSet2_2 = item.Infusions[1];
						}
						code.WeaponSet2.MainHand = APICache.ResolveWeaponType(item.Id);
						if(item.Upgrades != null) {
							code.WeaponSet2.Sigil1 = item.Upgrades[0];
							if(item.Upgrades.Count > 1)
								code.WeaponSet2.Sigil2 = item.Upgrades[1];
						}
						break;

					case ItemEquipmentSlotType.WeaponB2:
						if(aquatic) break;
						code.EquipmentAttributes.WeaponSet2OffHand = ResolveStatId(item);
						code.Infusions.WeaponSet2_2 = item.Infusions?[0];
						code.WeaponSet2.OffHand = APICache.ResolveWeaponType(item.Id);
						code.WeaponSet2.Sigil2 = item.Upgrades?[0];
						break;

					case ItemEquipmentSlotType.Amulet:
						if(aquatic) break;
						code.EquipmentAttributes.Amulet = ResolveStatId(item);
						code.Infusions          .Amulet = item.Infusions?[0];
						break;
				}
			}

			if(runeId != 0) code.Rune = runeId;
		}
		else
		{
			var pvpEquip = activeEquipment.EquipmentPvp!;

			code.EquipmentAttributes.Helmet = (StatId)(pvpEquip.Amulet ?? 0);
			code.Rune = pvpEquip.Rune;
			code.WeaponSet1.Sigil1 = pvpEquip.Sigils[0];
			code.WeaponSet1.Sigil2 = pvpEquip.Sigils[1];
			code.WeaponSet2.Sigil1 = pvpEquip.Sigils[2];
			code.WeaponSet2.Sigil2 = pvpEquip.Sigils[3];
		}

		var apiSkills = aquatic ? activeBuild.AquaticSkills : activeBuild.Skills;
		code.SlotSkills.Heal     = (SkillId?)apiSkills.Heal;
		code.SlotSkills.Utility1 = (SkillId?)apiSkills.Utilities[0];
		code.SlotSkills.Utility2 = (SkillId?)apiSkills.Utilities[1];
		code.SlotSkills.Utility3 = (SkillId?)apiSkills.Utilities[2];
		code.SlotSkills.Elite    = (SkillId?)apiSkills.Elite;

		switch(code.Profession)
		{
			case Profession.Ranger:
				var rangerData = new RangerData();

				var petBlock = aquatic ? activeBuild.Pets!.Aquatic : activeBuild.Pets!.Terrestrial;
				rangerData.Pet1 = petBlock[0];
				rangerData.Pet2 = petBlock[1];

				code.ProfessionSpecific = rangerData;
				break;

			case Profession.Revenant:
				var revenantData = new RevenantData();

				var legends = aquatic ? activeBuild.AquaticLegends! : activeBuild.Legends!;
				var legend1 = ResolveLegend(in code.Specializations.Choice3, legends[0]);
				var legend2 = ResolveLegend(in code.Specializations.Choice3, legends[1]);
				if(legend1.HasValue) // One legend is always set.
				{
					revenantData.Legend1 = legend1.Value;
					revenantData.Legend2 = legend2;

					//NOTE(Rennorb): doesnt seem to be available via the api
					// activeBuild.Skills = 
				}
				else // Flip so the legend 1 has the data.
				{
					revenantData.Legend1 = legend2!.Value;
					revenantData.Legend2 = legend1;

					revenantData.AltUtilitySkill1 = code.SlotSkills.Utility1;
					revenantData.AltUtilitySkill2 = code.SlotSkills.Utility2;
					revenantData.AltUtilitySkill3 = code.SlotSkills.Utility3;

					// inactive skills dont seem to be available
					code.SlotSkills.Utility1 = null;
					code.SlotSkills.Utility2 = null;
					code.SlotSkills.Utility3 = null;
				}

				code.ProfessionSpecific = revenantData;
				break;
		}
		return code;
	}

	internal static StatId ResolveStatId(CharacterEquipmentItem item)
		=> item.Stats != null ? (StatId)item.Stats.Id : APICache.ResolveStatId(item.Id);
}
