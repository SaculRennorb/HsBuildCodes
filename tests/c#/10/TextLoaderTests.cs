using Hardstuck.GuildWars2.BuildCodes.V2.Util;
using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Text;

public class FunctionTests {
	[Fact]
	public void DecodeValueFixed()
	{
		Assert.Equal( 0, TextLoader.Decode('A'));
		Assert.Equal(26, TextLoader.Decode('a'));
		Assert.Equal(63, TextLoader.Decode('-'));
	}

	[Fact]
	public void SuccessiveDecodeAndEatValueFixed()
	{
		var text = "Aa-".AsSpan();
		Assert.Equal( 0, TextLoader.DecodeNextChar(ref text));
		Assert.Equal(26, TextLoader.DecodeNextChar(ref text));
		Assert.Equal(63, TextLoader.DecodeNextChar(ref text));
		Assert.Equal(0, text.Length);
	}

	[Fact]
	public void SuccessiveDecodeAndEatValueValirable()
	{
		var text = "Aa-".AsSpan();
		Assert.Equal( 0, TextLoader.Decode(ref text, 1));
		Assert.Equal(26, TextLoader.Decode(ref text, 1));
		Assert.Equal(63, TextLoader.Decode(ref text, 1));
		Assert.Equal(0, text.Length);
	}

	[Fact]
	public void DecodeAndEatValueEarlyTerm()
	{
		var text = "A~".AsSpan();
		Assert.Equal(0, TextLoader.Decode(ref text, 3));
		Assert.Equal(0, text.Length);
	}
}

public class BasicCodesTests {
	[Fact]
	public void ShouldThrowVersion()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode("Xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		});
	}

	[Fact]
	public void ShouldThrowTooShort()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode("Btoo-short");
		});
	}

	[Fact]
	public void ShouldThrowInvalidCharacters()
	{
		Assert.ThrowsAny<Exception>(() => {
			var code = TextLoader.LoadBuildCode("B���������������������������������������������������������������������");
		});
	}

	[Fact]
	public void MinimalPvP()
	{
		var code = TextLoader.LoadBuildCode("BpA___~______B~");
		Assert.Equal(2                  , code.Version);
		Assert.Equal(Kind.PvP           , code.Kind);
		Assert.Equal(Profession.Guardian, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Null(code.Specializations[i]);
		Assert.False(code.Weapons.Set1.IsSet);
		Assert.False(code.Weapons.Set2.IsSet);
		for(int i = 0; i < 5; i++)
			Assert.Null(code.SlotSkills[i]);
		Assert.Null(code.Rune);
		for(int i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 14) Assert.Equal(default, code.EquipmentAttributes[i]);
			else Assert.Equal(1, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			Assert.Null(code.Infusions[i]);
		Assert.Null(code.Food);
		Assert.Null(code.Utility);
		Assert.Equal(IProfessionArbitrary.NONE.Instance, code.ArbitraryData.ProfessionSpecific);
		Assert.Equal(IArbitrary          .NONE.Instance, code.ArbitraryData.Arbitrary);
	}

	[Fact]
	public void MinimalPvE()
	{
		var code = TextLoader.LoadBuildCode("BoA___~______B~N~__");
		Assert.Equal(2                  , code.Version);
		Assert.Equal(Kind.PvE           , code.Kind);
		Assert.Equal(Profession.Guardian, code.Profession);
		for(int i = 0; i < 3; i++)
			Assert.Null(code.Specializations[i]);
		Assert.False(code.Weapons.Set1.IsSet);
		Assert.False(code.Weapons.Set2.IsSet);
		for(int i = 0; i < 5; i++)
			Assert.Null(code.SlotSkills[i]);
		Assert.Null(code.Rune);
		for(int i = 0; i < Static.ALL_EQUIPMENT_COUNT; i++) {
			if(11 <= i && i <= 14) Assert.Equal(default, code.EquipmentAttributes[i]);
			else Assert.Equal(1, code.EquipmentAttributes[i]);
		}
		for(int i = 0; i < Static.ALL_INFUSION_COUNT; i++)
			Assert.Null(code.Infusions[i]);
		Assert.Null(code.Food);
		Assert.Null(code.Utility);
		Assert.Equal(IProfessionArbitrary.NONE.Instance, code.ArbitraryData.ProfessionSpecific);
		Assert.Equal(IArbitrary          .NONE.Instance, code.ArbitraryData.Arbitrary);
	}

	[Fact]
	public void MinimalRanger()
	{
		var code = TextLoader.LoadBuildCode("BoD___~______A~N~__~");
		Assert.IsType<RangerData>(code.ArbitraryData.ProfessionSpecific);
		var data = (RangerData)code.ArbitraryData.ProfessionSpecific;
		Assert.Null(data.Pet1);
		Assert.Null(data.Pet2);
	}

	[Fact]
	public void MinimalRevenant()
	{
		var code = TextLoader.LoadBuildCode("BoI___~______A~N~____");
		Assert.IsType<RevenantData>(code.ArbitraryData.ProfessionSpecific);
		var data = (RevenantData)code.ArbitraryData.ProfessionSpecific;
		Assert.Null(data.Legend1);
		Assert.Null(data.Legend2);
		Assert.Null(data.AltUtilitySkill1);
		Assert.Null(data.AltUtilitySkill2);
		Assert.Null(data.AltUtilitySkill3);
	}
}

public class OfficialChatLinks {
	[Fact]
	public void LoadOfficialLink()
	{
		ProfessionSkillPallettes.Reload(Profession.Necromancer, true);

		var code = TextLoader.LoadOfficialBuildCode("[&DQg1KTIlIjbBEgAAgQB1AUABgQB1AUABlQCVAAAAAAAAAAAAAAAAAAAAAAA=]");
		Assert.Equal(Profession.Necromancer, code.Profession);

		Assert.Equal(SpecializationId.Spite, code.Specializations[0]!.Value.SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.MIDDLE,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[0]!.Value.Choices);

		Assert.Equal(SpecializationId.Soul_Reaping, code.Specializations[1]!.Value.SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.TOP,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.MIDDLE,
		}, code.Specializations[1]!.Value.Choices);

		Assert.Equal(SpecializationId.Reaper, code.Specializations[2]!.Value.SpecializationId);
		Assert.Equal(new TraitLineChoices() {
			Adept = TraitLineChoice.MIDDLE,
			Master = TraitLineChoice.TOP,
			Grandmaster = TraitLineChoice.BOTTOM,
		}, code.Specializations[2]!.Value.Choices);

		Assert.Equal(SkillId.Your_Soul_Is_Mine, code.SlotSkills[0]);
		Assert.Equal(SkillId.Well_of_Suffering1, code.SlotSkills[1]);
		Assert.Equal(SkillId.Well_of_Darkness1, code.SlotSkills[2]);
		Assert.Equal(SkillId.Signet_of_Spite, code.SlotSkills[3]);
		Assert.Equal(SkillId.Summon_Flesh_Golem, code.SlotSkills[4]);
	}

	[Fact]
	public void WriteOfficialLink()
	{
		var code = new BuildCode {
			Profession = Profession.Necromancer,
			Specializations = {
				Choice1 = new Specialization() {
					SpecializationId = SpecializationId.Spite,
					Choices          = {
						Adept        = TraitLineChoice.TOP,
						Master       = TraitLineChoice.MIDDLE,
						Grandmaster  = TraitLineChoice.MIDDLE,
					},
				},
				Choice2 = new Specialization() {
					SpecializationId = SpecializationId.Soul_Reaping,
					Choices          = {
						Adept        = TraitLineChoice.TOP,
						Master       = TraitLineChoice.TOP,
						Grandmaster  = TraitLineChoice.MIDDLE,
					},
				},
				Choice3 = new Specialization() {
					SpecializationId = SpecializationId.Reaper,
					Choices          = {
						Adept        = TraitLineChoice.MIDDLE,
						Master       = TraitLineChoice.TOP,
						Grandmaster  = TraitLineChoice.BOTTOM,
					},
				},
			},
			SlotSkills = {
				Heal = SkillId.Your_Soul_Is_Mine,
				Utility1 = SkillId.Well_of_Suffering1,
				Utility2 = SkillId.Well_of_Darkness1,
				Utility3 = SkillId.Signet_of_Spite,
				Elite   = SkillId.Summon_Flesh_Golem,
			}
		};

		ProfessionSkillPallettes.Reload(Profession.Necromancer, true);

		var reference = "[&DQg1KTIlIjbBEgAAgQAAAEABAAB1AQAAlQAAAAAAAAAAAAAAAAAAAAAAAAA=]";
		var result = TextLoader.WriteOfficialBuildCode(code);
		Assert.Equal(reference, result);
	}
}
