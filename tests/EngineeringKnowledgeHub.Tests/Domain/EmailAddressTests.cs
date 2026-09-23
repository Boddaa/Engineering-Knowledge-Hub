namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Xunit;

public class EmailAddressTests
{
    [Theory]
    [InlineData("engineer@company.com")]
    [InlineData("john.doe@subdomain.domain.org")]
    [InlineData("Lead_Dev+test@domain.co.uk")]
    public void Create_With_Valid_Email_Should_Succeed(string input)
    {
        var email = EmailAddress.Create(input);

        Assert.NotNull(email);
        Assert.Equal(input.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_With_Null_Or_Empty_Should_Throw_DomainRuleViolationException(string? input)
    {
        Assert.Throws<DomainRuleViolationException>(() => EmailAddress.Create(input!));
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@.com")]
    [InlineData("username@domain")]
    public void Create_With_Invalid_Format_Should_Throw_DomainRuleViolationException(string input)
    {
        Assert.Throws<DomainRuleViolationException>(() => EmailAddress.Create(input));
    }

    [Fact]
    public void Emails_With_Different_Casing_Should_Be_Equal()
    {
        var email1 = EmailAddress.Create("Engineer@Company.COM");
        var email2 = EmailAddress.Create("engineer@company.com");

        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }
}
