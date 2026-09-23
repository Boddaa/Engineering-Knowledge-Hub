namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Xunit;

public class UserTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    private readonly EmailAddress _validEmail = EmailAddress.Create("engineer@company.com");

    [Fact]
    public void Constructor_With_Valid_Parameters_Should_Initialize_User()
    {
        var user = new User(
            _userId,
            "john.doe",
            "John Doe",
            _validEmail,
            _now,
            isActive: true);

        Assert.Equal(_userId, user.Id);
        Assert.Equal("john.doe", user.Username);
        Assert.Equal("John Doe", user.DisplayName);
        Assert.Equal(_validEmail, user.Email);
        Assert.True(user.IsActive);
        Assert.Equal(_now, user.CreatedAt);
        Assert.Null(user.UpdatedAt);
        Assert.Empty(user.RoleIds);
    }

    [Fact]
    public void Constructor_With_Empty_Id_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new User(
            Guid.Empty,
            "john.doe",
            "John Doe",
            _validEmail,
            _now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_With_Invalid_Username_Should_Throw_DomainRuleViolationException(string? username)
    {
        Assert.Throws<DomainRuleViolationException>(() => new User(
            _userId,
            username!,
            "John Doe",
            _validEmail,
            _now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_With_Invalid_DisplayName_Should_Throw_DomainRuleViolationException(string? displayName)
    {
        Assert.Throws<DomainRuleViolationException>(() => new User(
            _userId,
            "john.doe",
            displayName!,
            _validEmail,
            _now));
    }

    [Fact]
    public void Constructor_With_Null_Email_Should_Throw_DomainRuleViolationException()
    {
        Assert.Throws<DomainRuleViolationException>(() => new User(
            _userId,
            "john.doe",
            "John Doe",
            null!,
            _now));
    }

    [Fact]
    public void Constructor_Should_Normalize_Username_To_Lowercase_And_Trim()
    {
        var user = new User(
            _userId,
            "  Ahmed.Mansour  ",
            "  Ahmed Mansour  ",
            _validEmail,
            _now);

        Assert.Equal("ahmed.mansour", user.Username);
        Assert.Equal("Ahmed Mansour", user.DisplayName);
    }

    [Fact]
    public void UpdateProfile_With_Valid_Data_Should_Update_Fields()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);
        var newEmail = EmailAddress.Create("john.new@company.com");
        var updatedAt = _now.AddDays(1);

        user.UpdateProfile("  John Updated  ", newEmail, updatedAt);

        Assert.Equal("John Updated", user.DisplayName);
        Assert.Equal(newEmail, user.Email);
        Assert.Equal(updatedAt, user.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateProfile_With_Invalid_DisplayName_Should_Throw_DomainRuleViolationException(string? displayName)
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);

        Assert.Throws<DomainRuleViolationException>(() =>
            user.UpdateProfile(displayName!, _validEmail, _now.AddHours(1)));
    }

    [Fact]
    public void UpdateProfile_With_Null_Email_Should_Throw_DomainRuleViolationException()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);

        Assert.Throws<DomainRuleViolationException>(() =>
            user.UpdateProfile("Valid Name", null!, _now.AddHours(1)));
    }

    [Fact]
    public void AssignRole_With_Valid_RoleId_Should_Add_Role()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);

        Assert.Contains(roleId, user.RoleIds);
        Assert.Single(user.RoleIds);
    }

    [Fact]
    public void AssignRole_With_Duplicate_RoleId_Should_Be_Idempotent()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);
        user.AssignRole(roleId);

        Assert.Contains(roleId, user.RoleIds);
        Assert.Single(user.RoleIds);
    }

    [Fact]
    public void AssignRole_With_Empty_RoleId_Should_Throw_DomainRuleViolationException()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);

        Assert.Throws<DomainRuleViolationException>(() => user.AssignRole(Guid.Empty));
    }

    [Fact]
    public void RemoveRole_With_Existing_RoleId_Should_Remove_Role()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        user.RemoveRole(roleId);

        Assert.DoesNotContain(roleId, user.RoleIds);
        Assert.Empty(user.RoleIds);
    }

    [Fact]
    public void RemoveRole_With_NonExisting_RoleId_Should_Not_Throw()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now);
        var roleId = Guid.NewGuid();

        user.RemoveRole(roleId);

        Assert.Empty(user.RoleIds);
    }

    [Fact]
    public void Deactivate_And_Activate_Should_Toggle_IsActive_State()
    {
        var user = new User(_userId, "john.doe", "John Doe", _validEmail, _now, isActive: true);
        Assert.True(user.IsActive);

        user.Deactivate();
        Assert.False(user.IsActive);

        user.Activate();
        Assert.True(user.IsActive);
    }
}
