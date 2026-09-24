namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;
using EngineeringKnowledgeHub.Domain.Exceptions;
using Xunit;

public class KnowledgeSourceTests
{
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Initialize_Source()
    {
        var id = Guid.NewGuid();
        var source = new KnowledgeSource(
            id,
            "Official Camera Vendor API Docs",
            SourceType.VendorDocumentation,
            _now,
            "https://docs.vendor.com/camera/v2",
            "Hardware protocol manual",
            TrustLevel.Official,
            true);

        Assert.Equal(id, source.Id);
        Assert.Equal("Official Camera Vendor API Docs", source.Name);
        Assert.Equal(SourceType.VendorDocumentation, source.SourceType);
        Assert.Equal("https://docs.vendor.com/camera/v2", source.Url);
        Assert.Equal("Hardware protocol manual", source.Description);
        Assert.Equal(TrustLevel.Official, source.TrustLevel);
        Assert.True(source.IsApproved);
        Assert.Equal(_now, source.CreatedAt);
        Assert.Null(source.UpdatedAt);
    }

    [Fact]
    public void Constructor_With_Empty_Id_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new KnowledgeSource(
            Guid.Empty,
            "Source Name",
            SourceType.Internal,
            _now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_With_Invalid_Name_Should_Throw_DomainRuleViolationException(string? name)
    {
        Assert.Throws<DomainRuleViolationException>(() => new KnowledgeSource(
            Guid.NewGuid(),
            name!,
            SourceType.Internal,
            _now));
    }

    [Fact]
    public void Constructor_With_Invalid_SourceType_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new KnowledgeSource(
            Guid.NewGuid(),
            "Source",
            (SourceType)999,
            _now));
    }

    [Fact]
    public void Constructor_With_Invalid_TrustLevel_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new KnowledgeSource(
            Guid.NewGuid(),
            "Source",
            SourceType.Internal,
            _now,
            trustLevel: (TrustLevel)999));
    }

    [Fact]
    public void Constructor_With_Invalid_Url_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new KnowledgeSource(
            Guid.NewGuid(),
            "Source",
            SourceType.External,
            _now,
            url: "not-a-valid-uri"));
    }

    [Fact]
    public void Approve_When_Untrusted_Should_Set_IsApproved_True_And_Promote_To_Verified()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Community Guide",
            SourceType.External,
            _now,
            trustLevel: TrustLevel.Untrusted,
            isApproved: false);

        var approvedAt = _now.AddHours(1);
        source.Approve(approvedAt);

        Assert.True(source.IsApproved);
        Assert.Equal(TrustLevel.Verified, source.TrustLevel);
        Assert.Equal(approvedAt, source.UpdatedAt);
    }

    [Fact]
    public void RevokeApproval_Should_Set_IsApproved_False()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Vendor Docs",
            SourceType.VendorDocumentation,
            _now,
            isApproved: true);

        var revokedAt = _now.AddHours(2);
        source.RevokeApproval(revokedAt);

        Assert.False(source.IsApproved);
        Assert.Equal(revokedAt, source.UpdatedAt);
    }

    [Fact]
    public void UpdateTrustLevel_With_Valid_Level_Should_Update_Trust_And_Timestamp()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Source",
            SourceType.Internal,
            _now,
            trustLevel: TrustLevel.Community);

        var updatedAt = _now.AddMinutes(30);
        source.UpdateTrustLevel(TrustLevel.Official, updatedAt);

        Assert.Equal(TrustLevel.Official, source.TrustLevel);
        Assert.Equal(updatedAt, source.UpdatedAt);
    }

    [Fact]
    public void UpdateTrustLevel_With_Invalid_Level_Should_Throw_DomainRuleViolationException()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Source",
            SourceType.Internal,
            _now);

        Assert.Throws<DomainRuleViolationException>(() =>
            source.UpdateTrustLevel((TrustLevel)888, _now.AddMinutes(5)));
    }

    [Fact]
    public void Update_With_Valid_Parameters_Should_Update_Fields()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Original Name",
            SourceType.External,
            _now,
            "https://original.com",
            "Original Desc");

        var updatedAt = _now.AddDays(1);
        source.Update("New Name", "https://updated.com", "New Desc", updatedAt);

        Assert.Equal("New Name", source.Name);
        Assert.Equal("https://updated.com", source.Url);
        Assert.Equal("New Desc", source.Description);
        Assert.Equal(updatedAt, source.UpdatedAt);
    }

    [Fact]
    public void Update_With_Invalid_Url_Should_Throw_DomainRuleViolationException()
    {
        var source = new KnowledgeSource(
            Guid.NewGuid(),
            "Name",
            SourceType.External,
            _now);

        Assert.Throws<DomainRuleViolationException>(() =>
            source.Update("New Name", "invalid-uri", "Desc", _now.AddHours(1)));
    }
}
