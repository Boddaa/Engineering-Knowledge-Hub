namespace EngineeringKnowledgeHub.Tests.Domain;

using EngineeringKnowledgeHub.Domain.Exceptions;
using EngineeringKnowledgeHub.Domain.ValueObjects;
using Xunit;

public class SlugTests
{
    [Theory]
    [InlineData("aspnet-core-middleware")]
    [InlineData("toll-collection-system-v1")]
    [InlineData("mongodb-performance-tuning")]
    [InlineData("anpr-camera-setup")]
    [InlineData("دليل-تثبيت-النظام")]
    [InlineData("إعداد-كاميرات-anpr-2026")]
    [InlineData("حل-مشكلة-البوابة-lane-3")]
    public void Create_With_Valid_Slug_Should_Succeed(string input)
    {
        var slug = Slug.Create(input);

        Assert.NotNull(slug);
        Assert.Equal(input, slug.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_With_Null_Or_Whitespace_Should_Throw_DomainRuleViolationException(string? input)
    {
        Assert.Throws<DomainRuleViolationException>(() => Slug.Create(input!));
    }

    [Theory]
    [InlineData("Invalid Slug With Spaces")]
    [InlineData("invalid_slug_with_underscores")]
    [InlineData("slug--with--double--hyphens")]
    [InlineData("-leading-hyphen")]
    [InlineData("trailing-hyphen-")]
    [InlineData("special$characters!")]
    [InlineData("دليل تثبيت مع مسافات")]
    [InlineData("-دليل-بداية-واصلة")]
    public void Create_With_Invalid_Format_Should_Throw_DomainRuleViolationException(string input)
    {
        Assert.Throws<DomainRuleViolationException>(() => Slug.Create(input));
    }

    [Fact]
    public void FromTitle_With_English_Title_Should_Generate_Normalized_Slug()
    {
        var title = "ASP.NET Core Middleware & Architecture Guide (2026)!";
        var slug = Slug.FromTitle(title);

        Assert.Equal("asp-net-core-middleware-architecture-guide-2026", slug.Value);
        Assert.Equal(slug, Slug.Create(slug.Value));
    }

    [Fact]
    public void FromTitle_With_Arabic_Title_Should_Generate_Normalized_Arabic_Slug()
    {
        var title = "دليل تثبيت وتشغيل بوابات الرسوم (الإصدار 2)";
        var slug = Slug.FromTitle(title);

        Assert.Equal("دليل-تثبيت-وتشغيل-بوابات-الرسوم-الإصدار-2", slug.Value);
        Assert.Equal(slug, Slug.Create(slug.Value));
    }

    [Fact]
    public void FromTitle_With_Mixed_Arabic_And_English_Should_Generate_Valid_Slug()
    {
        var title = "حل مشكلة ANPR Camera Timeout على Lane 3!";
        var slug = Slug.FromTitle(title);

        Assert.Equal("حل-مشكلة-anpr-camera-timeout-على-lane-3", slug.Value);
        Assert.Equal(slug, Slug.Create(slug.Value));
    }

    [Fact]
    public void FromTitle_With_Repeated_Punctuation_And_Whitespace_Should_Collapse_Hyphens()
    {
        var title = "  Database --- Connection ... Timeout ?? [Fix]  ";
        var slug = Slug.FromTitle(title);

        Assert.Equal("database-connection-timeout-fix", slug.Value);
    }

    [Fact]
    public void FromTitle_With_Numbers_Only_Should_Generate_Valid_Slug()
    {
        var title = "Error Code 404 & 500";
        var slug = Slug.FromTitle(title);

        Assert.Equal("error-code-404-500", slug.Value);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("!@#$%^&*()")]
    [InlineData("... --- ...")]
    public void FromTitle_With_Invalid_Or_Empty_Title_Should_Throw_DomainRuleViolationException(string title)
    {
        Assert.Throws<DomainRuleViolationException>(() => Slug.FromTitle(title));
    }

    [Fact]
    public void Slugs_With_Same_Value_Should_Be_Equal()
    {
        var slug1 = Slug.Create("csharp-best-practices");
        var slug2 = Slug.Create("csharp-best-practices");

        Assert.Equal(slug1, slug2);
        Assert.True(slug1 == slug2);
        Assert.Equal(slug1.GetHashCode(), slug2.GetHashCode());
    }
}
