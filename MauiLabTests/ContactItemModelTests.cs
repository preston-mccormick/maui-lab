using MauiLab.Contacts;
using Contact = MauiLab.Contacts.Contact;

namespace MauiLabTests;

public class ContactItemModelTests
{
    [Fact]
    public void AClearedFirstNameShowsAnError()
    {
        var item = new ContactItemModel(Valid());
        Assert.True(item.CanSave);

        item.FirstName = "  ";

        Assert.False(item.CanSave);
        Assert.NotEmpty(item.GetErrors(nameof(ContactItemModel.FirstName)));
    }

    [Fact]
    public void ARepairedEmailClearsItsError()
    {
        var item = new ContactItemModel(Valid());
        item.Email = "ada@example";
        Assert.NotEmpty(item.GetErrors(nameof(ContactItemModel.Email)));

        item.Email = "ada@example.com";

        Assert.Empty(item.GetErrors(nameof(ContactItemModel.Email)));
        Assert.True(item.CanSave);
    }

    [Fact]
    public void PhoneStoresDigitsAndGroupsTheDisplay()
    {
        var item = new ContactItemModel(Valid());

        item.Phone = "(272) 790-0888";

        Assert.Equal("2727900888", item.Contact.Phone);
        Assert.Equal("(272) 790-0888", item.Phone);
    }

    [Fact]
    public void DecliningAPhotoLeavesTheCurrentOne()
    {
        var item = new ContactItemModel(Valid());
        item.ChoosePhoto("picker/one.jpg");

        item.ChoosePhoto(null);

        Assert.Equal("picker/one.jpg", item.PhotoFile);
    }

    static Contact Valid() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        State = "MI",
        PostalCode = "63104",
        Phone = "2727900888",
        Email = "ada@example.com",
        PhotoFile = "portraits/shared.jpg"
    };
}
