using System.ComponentModel.DataAnnotations;

namespace MauiLab.Contacts;

public sealed class PresentNameAttribute : ValidationAttribute
{
    public PresentNameAttribute() => ErrorMessage = "Enter a name.";

    public override bool IsValid(object? value) =>
        value is string text && text.Trim().Length > 0;
}

public sealed class OptionalEmailAttribute : ValidationAttribute
{
    public OptionalEmailAttribute() => ErrorMessage = "Enter an email like name@example.com.";

    public override bool IsValid(object? value)
    {
        if (value is not string text || text.Length == 0)
            return true;

        var at = text.IndexOf('@');
        if (at <= 0 || at != text.LastIndexOf('@') || at == text.Length - 1)
            return false;

        var domain = text[(at + 1)..];
        var dot = domain.IndexOf('.');
        return dot > 0 && dot < domain.Length - 1 && !domain.Contains(' ');
    }
}

public sealed class OptionalPhoneAttribute : ValidationAttribute
{
    public OptionalPhoneAttribute() => ErrorMessage = "Enter a 10-digit phone number.";

    public override bool IsValid(object? value)
    {
        if (value is not string text || text.Length == 0)
            return true;

        return text.Length == 10 && text.All(char.IsDigit);
    }
}

public sealed class OptionalStateAttribute : ValidationAttribute
{
    public OptionalStateAttribute() => ErrorMessage = "Choose a state.";

    public override bool IsValid(object? value)
    {
        if (value is not string text || text.Length == 0)
            return true;

        return UsStates.TryCode(text, out var code) && code == text;
    }
}

public sealed class OptionalPostalCodeAttribute : ValidationAttribute
{
    public OptionalPostalCodeAttribute() => ErrorMessage = "Enter a 5-digit ZIP or ZIP+4.";

    public override bool IsValid(object? value)
    {
        if (value is not string text || text.Length == 0)
            return true;

        if (text.Length == 5)
            return text.All(char.IsDigit);

        if (text.Length == 10 && text[5] == '-')
            return text[..5].All(char.IsDigit) && text[6..].All(char.IsDigit);

        return false;
    }
}
