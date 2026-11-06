using MauiLab.Contacts;

namespace MauiLabTests;

public class ContactImportTests
{
    [Fact]
    public void FixtureNormalizesTheSample()
    {
        var json = File.ReadAllText(SamplePath());
        var contacts = ContactImporter.Import(json);

        Assert.Equal(2, contacts.Count);
        var ada = contacts[0];
        Assert.Equal("Ada", ada.FirstName);
        Assert.Equal("12 Analytical Eng", ada.Street);
        Assert.Equal("MI", ada.State);
        Assert.Equal("63104", ada.PostalCode);
        Assert.Equal("2727900888", ada.Phone);
        Assert.Equal("portraits/women-75.jpg", ada.PhotoFile);
        Assert.Equal(ContactGroup.Friend, ada.Group);
        Assert.True(ada.IsValid());

        var grace = contacts[1];
        Assert.Equal("VA", grace.State);
        Assert.Equal("01234", grace.PostalCode);
        Assert.Equal("7035550199", grace.Phone);
        Assert.True(grace.IsValid());
    }

    [Fact]
    public void AThousandIndexesSplitIntoTheNestedGroups()
    {
        var groups = Enumerable.Range(0, 1000).Select(ContactImporter.GroupForIndex).ToList();

        Assert.Equal(10, groups.Count(group => group == ContactGroup.Friend));
        Assert.Equal(90, groups.Count(group => group == ContactGroup.Family));
        Assert.Equal(900, groups.Count(group => group == ContactGroup.Other));
    }

    [Fact]
    public void AnUnrepairableRowFailsTheImport()
    {
        const string json = """
            {
              "results": [
                {
                  "name": { "first": "Ada", "last": "Lovelace" },
                  "location": {
                    "street": { "number": 1, "name": "Main" },
                    "city": "Billings",
                    "state": "Michigan",
                    "postcode": "63104"
                  },
                  "email": "ada@example.com",
                  "phone": "12345",
                  "picture": { "medium": "" }
                }
              ]
            }
            """;

        var error = Assert.Throws<ContactImportException>(() => ContactImporter.Import(json));
        Assert.Contains("not a valid contact", error.Message);
    }

    static string SamplePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "MauiLabTests", "Contacts", "random-user-sample.json");
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("random-user-sample.json");
    }
}
