using System.ComponentModel.DataAnnotations;

namespace MauiLab.Contacts;

public sealed class Contact
{
    public int Id { get; set; }

    [PresentName]
    [MaxLength(50)]
    public string FirstName { get; set; } = "";

    [PresentName]
    [MaxLength(50)]
    public string LastName { get; set; } = "";

    [MaxLength(100)]
    public string Street { get; set; } = "";

    [MaxLength(50)]
    public string City { get; set; } = "";

    [OptionalState]
    public string State { get; set; } = "";

    [OptionalPostalCode]
    public string PostalCode { get; set; } = "";

    [OptionalPhone]
    public string Phone { get; set; } = "";

    [OptionalEmail]
    public string Email { get; set; } = "";

    [MaxLength(500)]
    public string Notes { get; set; } = "";

    public string? PhotoFile { get; set; }

    public ContactGroup Group { get; set; }

    public bool IsValid()
    {
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(this, new ValidationContext(this), results, validateAllProperties: true);
    }

    public Contact Copy() => new()
    {
        Id = Id,
        FirstName = FirstName,
        LastName = LastName,
        Street = Street,
        City = City,
        State = State,
        PostalCode = PostalCode,
        Phone = Phone,
        Email = Email,
        Notes = Notes,
        PhotoFile = PhotoFile,
        Group = Group
    };
}
