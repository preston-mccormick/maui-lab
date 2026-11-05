# Tasks

## 1. Timeline

- [x] 1.1 Extend `UsageTimelineTests` with a List View visit that records "Deleted swatch" / "AliceBlue" and "Viewed Swipe" with no detail. The row sits one level under "Navigated to List View page", the title is amber, and the viewed row has an empty detail. No timeline mapping change: `ActionTaken` is already one amber row. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter UsageTimelineTests` passes.

## 2. Shared model

- [x] 2.1 Add `SwatchItem` and `CollectionPageModel` in `MauiLab/Collections`. `VariedHeight` is 48 under 8 characters, 96 under 14, and 144 otherwise. The model opens on the named-color catalog, name ascending, Card at 180 by 120, font 14, margin 0, and timings off. `SetCatalog` and `SetSort` rebuild items from the catalog and drop pins. A swatch tap sets the background and records nothing. An unchanged font commit records nothing. Add `CollectionPageModelTests` for that open state, an XKCD swap of 949 items that clears a pin, hue ascending, a quiet tap, and a timings-on sort that sets a notice naming the sort. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter CollectionPageModelTests` passes.

## 3. Collection View

- [x] 3.1 Add `CollectionViewPageModel` for layout, orientation, span, both gaps, sizing, grouping, and scroll anchor. It opens as a vertical grid, span 2, measure first, gaps 8, ungrouped, keep offset. Grouping builds first-letter groups in sorted order. Add `CollectionViewPageModelTests` for that open state, "Changed sizing" / "Measure all", and "Changed horizontal spacing" / "8 to 16". Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter CollectionViewPageModelTests` passes.
- [x] 3.2 Add `SwatchView` and `CollectionViewPage` on `LayoutPlayground` with Data, Collection, and Item. Each setting is its own grid with a `SettingSeparator` between them. Numeric settings are `NumberLever`s so the slider spans the next row and the entry stays right aligned. The canvas is a stock `CollectionView` (linear or grid layout, no native inset). The timing notice floats at the bottom of the canvas. Info controls are "Sizing" and "Span" beside those settings. Add a Collections `FlyoutItem` after Layouts, route `collections`, with one inner `Tab` so the pages are top tabs, and this page as its only tab. The page title is Collection View. Verify Colors is still top-level, Flex Layout is still a Layouts tab, and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 4. Flex Items

- [x] 4.1 Add `FlexItemsPageModel`. It opens as a row with wrap on and padding 0, and it publishes the 12-letter catalog 40 items per turn until 3099 are present. Add `FlexItemsPageModelTests` for the open wrap, "Changed wrap" / "NoWrap", "Changed padding" / "0 to 16", and a finished long catalog of 3099. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter FlexItemsPageModelTests` passes.
- [x] 4.2 Add `FlexItemsPage` on `LayoutPlayground`: a `ScrollView` around a `FlexLayout` bound to the items, with direction, wrap, justification, both alignments, and padding. Info controls are "Batches" and "Gap". Add the Flex Items tab after Collection View. Verify the tab order and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 5. Uniform Items

- [x] 5.1 Add `UniformItemsPageModel`. Both maximums open on Fit. Setting columns from Fit to 2 records "Changed max columns" / "Fit to 2". A request below 1 changes nothing and records nothing. Add `UniformItemsPageModelTests` for Fit at open, the cap to 2, and the refused zero. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter UniformItemsPageModelTests` passes.
- [x] 5.2 Add `UniformItemsPage` on `LayoutPlayground` with a `ScrollView` around `UniformItemsLayout`. Fit writes `int.MaxValue`. The pickers offer Fit, 1, 2, 3, 4, 6, and 8. Replacing the card's width, height, margin, font, or arrangement replaces the layout so the manager measures the new first child. There is no orientation setting. Info controls are "First cell" and "Max rows". Add the Uniform Items tab after Flex Items. Verify `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 6. List View

- [x] 6.1 Add `ListViewPageModel`. It opens recycling, uneven rows off, row height 120, Pin and Delete on, Copy off. `Pin`, `Copy`, and `Delete` update the displayed items only. Choosing the catalog again restores a deleted name. Add `ListViewPageModelTests` for that open state, pin then unpin, a copy inserted after the source, "Deleted swatch" / "AliceBlue", the name returning when the catalog is chosen again, "Changed caching" / "Retain", and "Changed copy action" / "On". Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter ListViewPageModelTests` passes.
- [x] 6.2 Add `SwatchCell` and `ListViewPage`. The page creates the `ListView` with the model's caching strategy and replaces it when the strategy changes. The cell rebuilds context actions from the three flags and from `IsPinned` whenever its binding context changes. Delete is destructive. A flag change replaces the item template. The pin mark is an 8-point dot. Info controls are "Caching" and "Swipe". Add the List View tab after Uniform Items. Verify the four tabs are in spec order and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 8. Opening cost

- [x] 8.1 Cache `ColorCatalog.Load`. Read the named colors when the list is first asked for. Read the XKCD file and the 12-letter file the first time that catalog's colors are walked. The picker still names them with 949 and 3,099 before those files are read. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter "CollectionPageModelTests|FlexItemsPageModelTests"` passes.
- [x] 8.2 Keep the Collections page constructors to the playground. Create each page's model and items in `OnAppearing`, so Flex Items, Uniform Items, and List View stay empty until their tab is shown, and Collection View fills when the group opens on it. Verify `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 9. Levers and reset

- [x] 9.1 Add Reset on every collection page. It restores the opening settings, including the canvas color, resumes automatic fitting, rebuilds the opening catalog, and leaves the timings switch as set. While timings are on, the notice times that rebuild from the top start of the canvas. Record one "Reset" action with an empty detail. Add an About info control for each collection, an Arrangement info control, and a Scroll info control. Varied sizes are a stable mix of widths 160, 200, 240, and 280 and heights 96, 132, 168, and 204, not the name length. Disable a setting the current mode ignores: height unless the arrangement is Card, width for Varied, span and the unused linear spacing on Collection View, and row height while List View rows are uneven. Draw tick marks on `NumberLever` sliders. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter "CollectionPageModelTests|CollectionViewPageModelTests|ListViewPageModelTests|NumberScaleTests"` passes.

## 10. Tabs and panel chrome

- [x] 10.1 Put the collection About control on the right of the panel header. Put Reset on the left and Timings on the right of a flat row under the header. Make the docked panel flat and each levers section a floating card. Make the four collection pages bottom tabs. Make Grid, Absolute, and Dock their own Layouts bottom tabs, and put Stack, Flex, Composite, and Radial in a More Layouts bottom tab as top tabs. Verify `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds and `openspec validate collection-controls --type change --strict` passes.

## 11. Forms and alignment

- [x] 11.1 Rename the Experiments flyout group to Forms and move Form Controls into it with Dialogs and Menu Button. Delete the Scrolling Layout page. Inset the header info control and the timings switch so their right edges line up with the floating cards. Verify `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds and `openspec validate collection-controls --type change --strict` passes.

## 12. Manual pass follow-up

- [x] 12.1 Let the bottom-docked panel be resized from a grip, and keep that height across playgrounds. Share the timings switch across the Collections pages. Yield between batches when Flex Items or Uniform Items replace a long list, and apply catalog, sort, and arrangement when the picker closes. Clip Uniform Items cells to the first item's size. On Mac Catalyst, offer List View actions from a secondary click. Right-align picker text and move section switches to the trailing edge. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter CollectionPageModelTests` passes.

## 7. Integration

- [ ] 7.1 Verify `dotnet test MauiLabTests/MauiLabTests.csproj` passes and `openspec validate collection-controls --type change --strict` passes. Build `dotnet build MauiLab/MauiLab.csproj -f net11.0-maccatalyst` and run Collections. Tabs read Grid, Linear, Flex, Uniform, and the page titles read CollectionView Grid Layout, CollectionView Linear Layout, FlexView, UniformItemsView, and the levers panel reads Playground. The panel docks across 800, timings are off until the switch is on, Grid has no Layout setting, Flex fills the XKCD survey in batches, Uniform lays later cards out in the first card's cell when the arrangement is Varied, and a larger card paints outside that cell, and Linear reveals Pin from the left and Delete from the right (swipe down for Pin, swipe up for Delete when the list is horizontal). On Mac Catalyst a mouse drag reveals them the same way, and a secondary click does too. Delete removes the row. Linear offers the 12-letter catalog.
