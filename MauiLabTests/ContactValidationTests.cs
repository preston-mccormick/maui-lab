using System.ComponentModel.DataAnnotations;
using MauiLab.Contacts;
using Contact = MauiLab.Contacts.Contact;

namespace MauiLabTests;

public class ContactValidationTests
{
    [Fact]
    public void ABlankNameFails()
    {
        var contact = Valid();
        contact.FirstName = "   ";

        Assert.False(contact.IsValid());
        Assert.Contains("Enter a name.", Errors(contact, nameof(Contact.FirstName)));
    }

    [Fact]
    public void AnEmptyEmailPasses()
    {
        var contact = Valid();
        contact.Email = "";

        Assert.True(contact.IsValid());
    }

    [Theory]
    [InlineData("ada@example")]
    [InlineData("ada@example.com")]
    public void EmailMatchesTheDemoRule(string email)
    {
        var contact = Valid();
        contact.Email = email;

        Assert.Equal(email.Contains(".com"), contact.IsValid());
    }

    [Fact]
    public void ANineDigitPhoneFails()
    {
        var contact = Valid();
        contact.Phone = "272790088";

        Assert.False(contact.IsValid());
    }

    [Theory]
    [InlineData("1234", false)]
    [InlineData("12345", true)]
    [InlineData("12345-6789", true)]
    public void PostalCodeIsFiveOrNine(string postal, bool valid)
    {
        var contact = Valid();
        contact.PostalCode = postal;

        Assert.Equal(valid, contact.IsValid());
    }

    [Fact]
    public void ATooLongStreetFails()
    {
        var contact = Valid();
        contact.Street = new string('a', 101);

        Assert.False(contact.IsValid());
    }

    static Contact Valid() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Street = "12 Analytical Eng",
        City = "Billings",
        State = "MI",
        PostalCode = "63104",
        Phone = "2727900888",
        Email = "ada@example.com"
    };

    static IEnumerable<string> Errors(Contact contact, string member)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(contact, new ValidationContext(contact), results, validateAllProperties: true);
        return results.Where(result => result.MemberNames.Contains(member)).Select(result => result.ErrorMessage ?? "");
    }
}
