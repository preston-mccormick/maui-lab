namespace MauiLab.Contacts;

public static class UsStates
{
    public static IReadOnlyList<(string Name, string Code)> All { get; } =
    [
        ("Alabama", "AL"), ("Alaska", "AK"), ("Arizona", "AZ"), ("Arkansas", "AR"),
        ("California", "CA"), ("Colorado", "CO"), ("Connecticut", "CT"), ("Delaware", "DE"),
        ("District of Columbia", "DC"), ("Florida", "FL"), ("Georgia", "GA"), ("Hawaii", "HI"),
        ("Idaho", "ID"), ("Illinois", "IL"), ("Indiana", "IN"), ("Iowa", "IA"),
        ("Kansas", "KS"), ("Kentucky", "KY"), ("Louisiana", "LA"), ("Maine", "ME"),
        ("Maryland", "MD"), ("Massachusetts", "MA"), ("Michigan", "MI"), ("Minnesota", "MN"),
        ("Mississippi", "MS"), ("Missouri", "MO"), ("Montana", "MT"), ("Nebraska", "NE"),
        ("Nevada", "NV"), ("New Hampshire", "NH"), ("New Jersey", "NJ"), ("New Mexico", "NM"),
        ("New York", "NY"), ("North Carolina", "NC"), ("North Dakota", "ND"), ("Ohio", "OH"),
        ("Oklahoma", "OK"), ("Oregon", "OR"), ("Pennsylvania", "PA"), ("Rhode Island", "RI"),
        ("South Carolina", "SC"), ("South Dakota", "SD"), ("Tennessee", "TN"), ("Texas", "TX"),
        ("Utah", "UT"), ("Vermont", "VT"), ("Virginia", "VA"), ("Washington", "WA"),
        ("West Virginia", "WV"), ("Wisconsin", "WI"), ("Wyoming", "WY")
    ];

    public static bool TryCode(string? nameOrCode, out string code)
    {
        code = "";
        if (string.IsNullOrWhiteSpace(nameOrCode))
            return false;

        var text = nameOrCode.Trim();
        foreach (var (name, state) in All)
        {
            if (string.Equals(text, state, StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, name, StringComparison.OrdinalIgnoreCase))
            {
                code = state;
                return true;
            }
        }

        return false;
    }
}
