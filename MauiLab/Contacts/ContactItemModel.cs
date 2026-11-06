using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiLab.Contacts;

public partial class ContactItemModel : ObservableObject, INotifyDataErrorInfo
{
    static readonly string[] Members =
    [
        nameof(FirstName), nameof(LastName), nameof(Street), nameof(City), nameof(State),
        nameof(PostalCode), nameof(Phone), nameof(Email), nameof(Notes)
    ];

    readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    bool _isEditing;

    public ContactItemModel(Contact contact, string directory = "")
    {
        Contact = contact;
        DirectoryPath = directory;
        foreach (var member in Members)
            Validate(member, notify: false);
    }

    public Contact Contact { get; }

    public string DirectoryPath { get; }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing == value)
                return;

            _isEditing = value;
            OnPropertyChanged();
        }
    }

    public string DisplayName
    {
        get
        {
            var name = $"{FirstName} {LastName}".Trim();
            return name.Length == 0 ? "New contact" : name;
        }
    }

    public string AddressText
    {
        get
        {
            var city = string.Join(" ", new[] { City, State, PostalCode }.Where(part => part.Length > 0));
            return string.Join(Environment.NewLine, new[] { Street, city }.Where(part => part.Length > 0));
        }
    }

    public bool HasNotes => !string.IsNullOrWhiteSpace(Contact.Notes);

    public string PhotoPath =>
        string.IsNullOrEmpty(Contact.PhotoFile) || DirectoryPath.Length == 0
            ? ""
            : Path.Combine(DirectoryPath, Contact.PhotoFile.Replace('/', Path.DirectorySeparatorChar));

    public bool CanSave => !HasErrors;

    public string FirstName
    {
        get => Contact.FirstName;
        set => Set(nameof(FirstName), value, text => Contact.FirstName = text);
    }

    public string LastName
    {
        get => Contact.LastName;
        set => Set(nameof(LastName), value, text => Contact.LastName = text);
    }

    public string Street
    {
        get => Contact.Street;
        set => Set(nameof(Street), value, text => Contact.Street = text);
    }

    public string City
    {
        get => Contact.City;
        set => Set(nameof(City), value, text => Contact.City = text);
    }

    public string State
    {
        get => Contact.State;
        set => Set(nameof(State), value, text => Contact.State = text);
    }

    public string PostalCode
    {
        get => Contact.PostalCode;
        set => Set(nameof(PostalCode), value, text => Contact.PostalCode = text);
    }

    public string Phone
    {
        get => Contact.Phone.Length == 10 ? Group(Contact.Phone) : Contact.Phone;
        set => Set(nameof(Phone), Digits(value), digits => Contact.Phone = digits);
    }

    public string Email
    {
        get => Contact.Email;
        set => Set(nameof(Email), value, text => Contact.Email = text);
    }

    public string Notes
    {
        get => Contact.Notes;
        set => Set(nameof(Notes), value, text => Contact.Notes = text);
    }

    public string? PhotoFile
    {
        get => Contact.PhotoFile;
        private set
        {
            if (Contact.PhotoFile == value)
                return;

            Contact.PhotoFile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PhotoPath));
        }
    }

    public void Refresh()
    {
        foreach (var member in Members)
        {
            OnPropertyChanged(member);
            Validate(member, notify: true);
        }

        OnPropertyChanged(nameof(PhotoFile));
        OnPropertyChanged(nameof(PhotoPath));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(AddressText));
        OnPropertyChanged(nameof(HasNotes));
        OnPropertyChanged(nameof(CanSave));
    }

    public void ChoosePhoto(string? relativePath)
    {
        if (relativePath == null)
            return;

        PhotoFile = relativePath;
    }

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName != null && _errors.TryGetValue(propertyName, out var errors))
            return errors;

        return Array.Empty<string>();
    }

    void Set(string name, string next, Action<string> assign)
    {
        assign(next);
        OnPropertyChanged(name);
        if (name is nameof(FirstName) or nameof(LastName))
            OnPropertyChanged(nameof(DisplayName));
        if (name is nameof(Street) or nameof(City) or nameof(State) or nameof(PostalCode))
            OnPropertyChanged(nameof(AddressText));
        if (name == nameof(Notes))
            OnPropertyChanged(nameof(HasNotes));
        Validate(name, notify: true);
    }

    void Validate(string name, bool notify)
    {
        var value = typeof(Contact).GetProperty(name)!.GetValue(Contact);
        var results = new List<ValidationResult>();
        Validator.TryValidateProperty(value, new ValidationContext(Contact) { MemberName = name }, results);
        var messages = results.Select(result => result.ErrorMessage ?? "Invalid").ToList();
        var had = _errors.ContainsKey(name);
        if (messages.Count == 0)
            _errors.Remove(name);
        else
            _errors[name] = messages;

        if (notify && (had || messages.Count > 0))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(name));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public static string Digits(string value) => new(value.Where(char.IsDigit).ToArray());

    public static string Group(string digits) => $"({digits[..3]}) {digits[3..6]}-{digits[6..]}";
}
