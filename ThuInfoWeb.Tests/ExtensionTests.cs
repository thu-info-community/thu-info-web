using Xunit;

namespace ThuInfoWeb.Tests;

public class ExtensionTests
{
    [Fact]
    public void Sha256HashUsesTheLegacyLowercaseHexRepresentation()
    {
        Assert.Equal(
            "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
            "password".ToSHA256Hex());
    }

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("1.2", false)]
    [InlineData("1.2.3-beta", false)]
    public void VersionValidationRequiresThreeNumericParts(string version, bool expected)
    {
        Assert.Equal(expected, version.IsValidVersionNumber());
    }

    [Fact]
    public void VersionComparisonIsNumeric()
    {
        Assert.True("10.0.0".VersionGreaterThan("9.99.99"));
        Assert.False("1.2.3".VersionGreaterThan("1.2.3"));
    }
}
