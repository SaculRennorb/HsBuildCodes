using Xunit;

namespace Hardstuck.GuildWars2.BuildCodes.V2.Tests.Version;

public class VersionTests {
	public static IEnumerable<object[]> DataProvider() {
		var invalidCodes = new[] {
			"x____________________________",
		};
		
		var v1Codes = new[] {
			"v0_-------------------------",
		};
		
		var v2Codes = new[] {
			"c_____________",
			"C_____________",
		};

		return invalidCodes.Select(c => new object[]{ c, -1 })
			.Concat(v1Codes.Select(c => new object[] { c, 1 }))
			.Concat(v2Codes.Select(c => new object[] { c, 2 }));
	}

	[Theory] [MemberData(nameof(DataProvider))]
	public void VersionDetectionTest(string code, int expectedVersion) {
		Assert.Equal(expectedVersion, Static.DetermineCodeVersion(code));
	}
}