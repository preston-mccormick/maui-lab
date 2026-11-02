using MauiLab.Layouts;

namespace MauiLabTests;

public class CompositeLayoutPageModelTests
{
    [Fact]
    public void OpensOnFlexAt320()
    {
        var model = new CompositeLayoutPageModel();

        Assert.Equal(FieldLayout.Flex, model.Layout);
        Assert.Equal(320, model.Width);
        Assert.False(model.ShowDescription);
        Assert.False(model.ShowValidation);
    }

    [Fact]
    public void SetLayout_RecordsUniform()
    {
        var usage = new ListLayoutUsage();
        var model = new CompositeLayoutPageModel(usage);

        model.SetLayout(FieldLayout.Uniform);

        Assert.Equal(("Switched layout", "Uniform"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void CommitWidth_RecordsTheChangeAndSkipsAnUnchangedValue()
    {
        var usage = new ListLayoutUsage();
        var model = new CompositeLayoutPageModel(usage);

        model.CommitWidth();
        model.Width = 200;
        model.CommitWidth();

        Assert.Equal(("Changed width", "320 to 200"), Assert.Single(usage.Actions));
    }
}
