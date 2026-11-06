using System.Globalization;
using System.Text.Json;

namespace MauiLab.Contacts;

public sealed class ContactImportException : Exception
{
    public ContactImportException(string message)
        : base(message)
    {
    }
}

public static class ContactImporter
{
    public static ContactGroup GroupForIndex(int index) => index switch
    {
        < 10 => ContactGroup.Friend,
        < 100 => ContactGroup.Family,
        _ => ContactGroup.Other
    };

    public static IReadOnlyList<Contact> Import(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var results))
            throw new ContactImportException("The download has no results.");

        var contacts = new List<Contact>();
        var index = 0;
        foreach (var person in results.EnumerateArray())
        {
            var contact = Read(person, index);
            if (!contact.IsValid())
                throw new ContactImportException($"Person {index + 1} is not a valid contact.");

            contacts.Add(contact);
            index++;
        }

        return contacts;
    }

    static Contact Read(JsonElement person, int index)
    {
        var name = person.GetProperty("name");
        var location = person.GetProperty("location");
        var street = location.GetProperty("street");
        var number = street.GetProperty("number").GetInt32().ToString(CultureInfo.InvariantCulture);
        var streetName = street.GetProperty("name").GetString() ?? "";
        return new Contact
        {
            FirstName = (name.GetProperty("first").GetString() ?? "").Trim(),
            LastName = (name.GetProperty("last").GetString() ?? "").Trim(),
            Street = $"{number} {streetName}".Trim(),
            City = (location.GetProperty("city").GetString() ?? "").Trim(),
            State = StateCode(location.GetProperty("state").GetString()),
            PostalCode = Postal(location.GetProperty("postcode")),
            Phone = Phone(person.GetProperty("phone").GetString()),
            Email = (person.GetProperty("email").GetString() ?? "").Trim(),
            Notes = "",
            PhotoFile = PortraitFile(person),
            Group = GroupForIndex(index)
        };
    }

    static string StateCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return UsStates.TryCode(value, out var code) ? code : value.Trim();
    }

    static string Postal(JsonElement value)
    {
        var text = value.ValueKind switch
        {
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.String => value.GetString() ?? "",
            _ => ""
        };
        var digits = new string(text.Where(char.IsDigit).ToArray());
        if (digits.Length == 4)
            digits = "0" + digits;
        if (digits.Length == 9)
            return $"{digits[..5]}-{digits[5..]}";

        return digits;
    }

    static string Phone(string? value)
    {
        var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits[0] == '1')
            digits = digits[1..];

        return digits;
    }

    static string? PortraitFile(JsonElement person)
    {
        if (!person.TryGetProperty("picture", out var picture))
            return null;
        if (!picture.TryGetProperty("medium", out var medium))
            return null;

        var url = medium.GetString();
        if (string.IsNullOrWhiteSpace(url))
            return null;

        return "portraits/" + PortraitName(url);
    }

    public static string PortraitName(string url)
    {
        var path = url;
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            path = uri.AbsolutePath;

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var tail = parts.Length >= 2 ? string.Join('-', parts[^2..]) : parts[^1];
        foreach (var invalid in Path.GetInvalidFileNameChars())
            tail = tail.Replace(invalid, '-');

        return tail;
    }
}
