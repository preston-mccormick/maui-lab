using MauiLab.Layouts;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace MauiLabTests;

public class GridLayoutPageModelTests
{
    [Fact]
    public void StartsAsThreeByThreeWithSeedColors()
    {
        var model = new GridLayoutPageModel();

        Assert.Equal(48, model.ColumnSpacing);
        Assert.Equal(48, model.RowSpacing);

        Assert.Equal(
            [
                new GridLength(1, GridUnitType.Star),
                new GridLength(4, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star),
            ],
            model.Columns.Select(column => column.ToGridLength()));
        Assert.Equal(
            [
                new GridLength(1, GridUnitType.Star),
                new GridLength(4, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star),
            ],
            model.Rows.Select(row => row.ToGridLength()));

        Assert.Equal([Colors.Red, Colors.Green, Colors.Blue], model.Columns.Select(column => column.Color));
        Assert.Equal([Colors.Magenta, Colors.Yellow, Colors.Cyan], model.Rows.Select(row => row.Color));
    }

    [Fact]
    public void AddThenRemoveKeepsSurvivingColors()
    {
        var model = new GridLayoutPageModel();
        var firstColumn = model.Columns[0].Color;
        var middleColumn = model.Columns[1].Color;
        var lastColumn = model.Columns[2].Color;
        var firstRow = model.Rows[0].Color;
        var middleRow = model.Rows[1].Color;
        var lastRow = model.Rows[2].Color;

        model.AddColumnCommand.Execute(null);
        model.AddRowCommand.Execute(null);
        var addedColumn = model.Columns[3].Color;
        var addedRow = model.Rows[3].Color;

        Assert.NotEqual(firstColumn, addedColumn);
        Assert.NotEqual(middleColumn, addedColumn);
        Assert.NotEqual(lastColumn, addedColumn);
        Assert.NotEqual(firstRow, addedRow);
        Assert.NotEqual(middleRow, addedRow);
        Assert.NotEqual(lastRow, addedRow);

        model.RemoveColumnCommand.Execute(model.Columns[1]);
        model.RemoveRowCommand.Execute(model.Rows[1]);

        Assert.Equal([firstColumn, lastColumn, addedColumn], model.Columns.Select(column => column.Color));
        Assert.Equal([firstRow, lastRow, addedRow], model.Rows.Select(row => row.Color));

        while (model.Columns.Count > 1)
            model.RemoveColumnCommand.Execute(model.Columns[0]);

        var remaining = model.Columns[0].Color;
        Assert.False(model.RemoveColumnCommand.CanExecute(model.Columns[0]));
        model.RemoveColumnCommand.Execute(model.Columns[0]);
        Assert.Equal(remaining, Assert.Single(model.Columns).Color);
    }

    [Fact]
    public void GridLengthMapsUnitAndValue()
    {
        var auto = new GridTrack(GridUnitType.Auto, 12, Colors.Red);
        var star = new GridTrack(GridUnitType.Star, 4, Colors.Green);
        var absolute = new GridTrack(GridUnitType.Absolute, 120, Colors.Blue);

        Assert.Equal(GridLength.Auto, auto.ToGridLength());
        Assert.Equal(new GridLength(4, GridUnitType.Star), star.ToGridLength());
        Assert.Equal(new GridLength(120, GridUnitType.Absolute), absolute.ToGridLength());
        Assert.False(auto.IsValueVisible);
        Assert.True(star.IsValueVisible);
    }

    [Fact]
    public void PlaceChild_RecordsTheCell()
    {
        var usage = new ListLayoutUsage();
        var model = new GridLayoutPageModel(usage);
        model.AddChildCommand.Execute(null);
        var child = model.Children[0];

        model.Place(child, 1, 2, 1, 2);

        Assert.Equal(1, child.Row);
        Assert.Equal(2, child.Column);
        Assert.Equal(2, child.ColumnSpan);
        Assert.Equal(("Placed child", "row 1, column 2, span 1 by 2"), usage.Actions[^1]);
    }

    [Fact]
    public void RemoveLastColumn_RecordsNothing()
    {
        var usage = new ListLayoutUsage();
        var model = new GridLayoutPageModel(usage);

        while (model.Columns.Count > 1)
            model.RemoveColumnCommand.Execute(model.Columns[0]);

        var before = usage.Actions.Count;
        Assert.False(model.RemoveColumnCommand.CanExecute(model.Columns[0]));
        model.RemoveColumnCommand.Execute(model.Columns[0]);

        Assert.Single(model.Columns);
        Assert.Equal(before, usage.Actions.Count);
    }

    [Fact]
    public void NonNumericSpacing_BecomesZero()
    {
        var model = new GridLayoutPageModel();

        model.ColumnSpacing = double.NaN;
        model.RowSpacing = -4;

        Assert.Equal(0, model.ColumnSpacing);
        Assert.Equal(0, model.RowSpacing);
        Assert.Equal(GridLength.Auto, new GridTrack(GridUnitType.Auto, double.NaN, Colors.Red).ToGridLength());
    }
}
