# Design

## Context

See proposal.md for why this change exists. Behavior is in the `collection-playground`, `usage-log`, `usage-timeline`, `layout-playground`, and `form-controls` deltas.

`LayoutPlayground` already docks the panel, collapses, and hosts the canvas. `LeversSection`, `NumberLever`, `SettingSeparator`, and `InfoControl` are the panel pieces. `InfoControl` finds the playground and calls `ShowInfo`. `ILayoutUsage` is what a page model records through, and `TrackerLayoutUsage` already swallows a failed write. `UsageTimeline` maps every `ActionTaken` to one amber row under the open page, so collection actions need no new event kind.

`ColorCatalog` loads the named colors, the XKCD survey (949), and the 12-letter names (3,099). The Grid and Linear pages offer all three, because both are virtualized CollectionViews. Flex and Uniform stop at the survey, because they create a view for every item. The Colors page is a CollectionView browser for those same lists: list, sort, font, and reset. `SwatchesView` replaces the iOS collection layout to get a side inset, because `CollectionView` has no padding in this build.

`CommunityToolkit.Maui` 15.0.1 `UniformItemsLayout` has `MaxRows` and `MaxColumns`, both defaulting to `int.MaxValue`, and throws if either is set below 1. It has no orientation. Its manager measures the first visible child once, with infinite constraints, and keeps that size. Column count is how many of that width fit, clamped to `MaxColumns`. Row count stops at `MaxRows`, and children past that are not arranged.

`LinearItemsLayout` is the single-line CollectionView, which is the replacement for `ListView`. `SwipeView` wraps each item. Pin and Copy are leading: `LeftItems` on a vertical list, `TopItems` on a horizontal list. Delete is trailing: `RightItems` on a vertical list, and `BottomItems` so a horizontal list deletes with a swipe up. A mouse drag on Mac Catalyst reveals the same edges. The page also sets a context flyout on the card.

## Goals / Non-Goals

**Goals:**

- Four pages on the existing playground, with models that tests can call without a handler.
- One shared model base for the catalog, the sort, the card, and the timings.
- Swipe actions on the linear page that pin, copy, and delete, rebuilt when the offered set or the orientation changes.
- Stock `CollectionView` and stock `UniformItemsLayout`. The lab shows those controls, including the uniform manager's cached first size.

**Non-Goals:**

- Changing what each Layout page's own levers do. Colors stays a CollectionView browser: list, sort, font, and reset.
- A carousel, a refresh wrapper, a stack page, or `SwipeView` on the grid page.
- Copying the Colors native inset. Side padding stays that page's workaround.
- Remembering a pin, a copy, or a deletion across a catalog swap or a sort.
- A new usage column. Virtualizing flex or uniform.

## Decisions

### Collections group, flat folder

`AppShell` gains a `FlyoutItem` titled Collections, route `collections`, immediately after Layouts. Its four `ShellContent`s are direct children, so they are bottom tabs and Shell creates a page when that tab is shown. Tab labels: Grid, Linear, Flex, Uniform. Page titles: CollectionView Grid Layout, CollectionView Linear Layout, FlexView, UniformItemsView. The levers panel header reads Playground. The control card is titled CollectionView, FlexView, or UniformItemsView. The timeline reads "Navigated to FlexView page" and not "Navigated to Flex Layout page".

Layouts uses the same bottom bar for the pages that should stay independent: Grid Layout, Absolute Layout, and Dock Layout. Stack Layout, Flex Layout, Composite Layout, and Radial Layout stay together as top tabs inside a fourth bottom tab titled More Layouts. While a playground panel is docked on the bottom, a grip on its top edge resizes the split. That height is shared by the playgrounds for the rest of the session. The side dock stays a fixed width.

The former Experiments group is titled Forms, route `forms`. Its pages are Form Controls, Dialogs, and Menu Button. Form Controls is no longer its own flyout item. Scrolling Layout is removed.

`MauiLab/Collections` holds the pages and the shared types as siblings. The pages reference `MauiLab.Layouts` for the playground and `MauiLab.Colors` for `ColorCatalog`, `ColorModel`, and `ColorSortOption`. The sort switch is copied into the base model. Colors keeps the catalog browser and drops its flex path, control picker, and timings.

Each page is a `LayoutPlayground`. The playground header reads Playground. The canvas is the collection. The docked panel is flat. Each `LeversSection` is its own floating card, so Data, the control, and Item scroll as separate cards. The control card is titled CollectionView, FlexView, or UniformItemsView. The same card treatment applies to the Layout pages, which use the same section. An info control sits on the right of the panel header and describes that collection. Under the header, a flat row puts Reset on the left and the timings switch on the right. Those two controls use a 24-point trailing margin so their info buttons line up with the info buttons inside the cards: 12 points keeps the scrolled content clear of the overlay scrollbar, each card adds 4, and the card padding adds 8. The panel content is a vertical stack of the three cards. Each setting is its own grid, as `LayoutLevers` does, so a long label does not narrow the value on the next row. A `SettingSeparator` sits between settings. A numeric setting is a `NumberLever` in that grid, not inline: the entry stays on the label's row, right aligned and fixed width, and the lever moves its slider onto the next row spanning the grid. The label is vertically centered on the entry. An `InfoControl` sits in the trailing column of the setting it explains. A drag updates the value while it moves and records when it ends. The slider draws tick marks: one per step when the range has at most twelve steps, otherwise a coarser interval that still marks both ends.

### One base model

`CollectionPageModel` takes `ILayoutUsage` and defaults to `NoLayoutUsage.Instance`. It loads `ColorCatalog` once. `ColorCatalog.Load` caches that list. The named colors are read immediately. The XKCD file and the 12-letter file are read the first time their `Colors` are walked. Grid and Linear include the 12-letter catalog. Flex and Uniform do not. Each page has its own model, so a swap on Flex does not change Grid.

A bottom tab is its own Shell section, so opening Collections creates the grid page and leaves Linear, Flex, and Uniform uncreated until their tabs are shown. The page constructors still only build the playground. The model and the items are created in `OnAppearing`. Flex's 40-at-a-time publish still runs when a shown Flex page rebuilds. More Layouts is one section with four top tabs, so those four layout pages are created together when that bottom tab is shown. Grid, Absolute, and Dock are not.

`SwatchItem` carries the color, the name, `IsPinned`, and the width and height the current arrangement wants. `SwatchItem.VariedSize` is a stable mix that does not follow the name's length: width is 160, 200, 240, or 280, and height is 96, 132, 168, or 204. Row uses 44 and the shared width. Card uses the height and width settings, opening at 120 by 180. Margin opens at 0, font at 14, running from 8 to 40. Height is disabled unless the arrangement is Card. Width is disabled for Varied.

`SetCatalog` and `SetSort` rebuild `Items` from the catalog. Pins, copies, and deletions die there because the new items are new objects. Rebuild applies the current sort. Hue, luminosity, saturation, and name follow the same directions as `ColorSortOption`.

A catalog, sort, or arrangement chosen in an open picker is applied when that picker closes. A change made while the picker is closed applies immediately. The publish path times the work and yields a dispatcher turn so the notice can show the headline before the work. Flex adds or removes 40 items per turn when batch loading is on. The other pages replace the collection in one assignment. `ShowTimings` opens false, and the four collection pages share one switch: turning it on or off updates the pages that exist, and a page created later adopts the current value. While it is false the notice text stays empty. The notice is a card at the top start of the canvas grid, over the collection, dismissed by a tap. Reset rebuilds the opening catalog and sort, restores the theme background, and clears the fit locks so automatic columns resume. It leaves the timings switch alone, and while timings are on the notice times that rebuild. The visit records one "Reset" action. The notice is not the playground's info banner. The info banner stays at the top of the page.

Tapping a swatch sets the canvas background and records nothing. The background starts as the theme background and follows the theme until a swatch has been tapped, as Colors does.

### Collection View is the stock control

The canvas `CollectionView` uses `GridItemsLayout`. The page builds that layout when the orientation, span, or spacing changes. It opens as a vertical grid on `MeasureFirstItem`. Until the user sets the span, a gap, or the item width, the page fits those from the canvas with the color swatches' measure: at least two columns, preferred card 180, preferred gap 36, and a gap no smaller than 8. The gap is the item spacing, not the card margin. A horizontal grid fits the span to the height. A resize, a rotation, or the panel docking runs the fit again and records nothing. A lever the user has set stays put. The recorded details are "Measure first" and "Measure all", "Keep offset", "Keep item", and "Keep last".

Grouping builds `SwatchGroup` lists from the first letter, in the order the letters appear in the sorted items. The group header is that letter. Grouping off binds the flat `Items`. `IsGrouped` and the source switch together.

Info titles: "Sizing", "Span", and "Scroll". Sizing says measure-first reuses the first row's height and measure-all can animate the correction while the page is appearing. Span says it counts columns on a vertical grid and rows on a horizontal grid. Scroll says the anchor applies when the items are replaced.

### Flex Items realizes every child

The canvas is a `ScrollView` around a `FlexLayout` with `BindableLayout` bound to `Items`. Direction, wrap, justification, align items, align content, and padding bind to the model. It opens as a row with wrap on. Until the user sets the item width, the margin, or the padding, the page fits those to the canvas width with the same swatch measure. The margin is half the gap and the padding is the side inset, so a resize or a rotation fits again and records nothing. Recorded wrap details are the enum names, so off is "NoWrap".

Info titles: "Batches" and "Gap". Batches says every item is created and a long list is added 40 at a time. Gap says flex has no spacing, padding insets the layout, and item margin is the gap.

### Uniform Items keeps the toolkit's stamp

The canvas is a `ScrollView` around a `UniformItemsLayout`. Until the user sets item width or margin, it uses the same responsive swatch fit as Collection View and Flex Items: the card width follows the fit, its margin is half the gap, and the scroll view gets the fitted side inset. A resize reruns that fit without recording it, while a user-set width or margin remains fixed. The model stores max columns and max rows as an int, with null meaning Fit, which the page writes as `int.MaxValue`. The panel pickers offer Fit, 1, 2, 3, 4, 6, and 8. `SetMaxColumns` and `SetMaxRows` treat a value below 1 as a no-op: the previous value stays and nothing is recorded. The layout's own setter throws below 1, so the page never assigns such a value.

The manager caches the first measure for its lifetime. When width, height, margin, font, or arrangement changes, the page replaces the `UniformItemsLayout` so the next measure reads the new first child. Changing a maximum does not replace it. Those values are read on every measure. There is no orientation control. A later card whose own size is larger than that first cell paints outside the cell and overlaps its neighbors. `IsClippedToBounds` does not stop that on iOS, and the overlap stays.

Info titles: "First cell" and "Max rows". First cell says the first item is measured once and kept, and that changing the card rebuilds the layout so the new size is read. Max rows says items past the maximum are not arranged.

### Linear is a CollectionView with a swipe on each item

The model opens vertical, spacing 0, measure-first, keep-offset, Pin on, Copy off, Delete on. It offers the 12-letter catalog. There is no layout switch and no caching switch. `ItemSizingStrategy` is the stand-in for a shared row height.

The page uses `LinearItemsLayout`. `SwipeSwatch` is a `SwipeView` whose content is the card. Pin and Copy are `LeftItems` when the list is vertical and `TopItems` when it is horizontal. Delete is `RightItems` when the list is vertical and `BottomItems` when it is horizontal, so the delete gesture is a swipe up. Turning a flag on or off, or changing orientation, rebuilds those items on the live cells. Pin's text is "Unpin" while the item is pinned. Delete's swipe item is red. `Pin`, `Copy`, and `Delete` behave as they did on the old list: the mark toggles, a copy is inserted after the source, and delete removes that item from `Items` only. Choosing the catalog again brings the name back. The pin mark is an 8-point dot in the card's top corner.

Info titles: "Sizing", "Scroll", and "Swipe". Swipe says the gesture is on the item and across the scroll, and that a mouse drag on Mac Catalyst uses the same edges. A right-click or Control-click lists them too. The Mac menu is a context flyout on the card. Replacing that menu waits until the click has finished, so the card is not left hidden.

### What each page's Collection section holds

- Grid: orientation, span, horizontal spacing, vertical spacing, sizing, grouping, scroll anchor. Sizing and Span carry the info controls.
- Linear: orientation, spacing, sizing, scroll anchor, and a switch each for Pin, Copy, and Delete. Sizing, Scroll, and Swipe carry the info controls. The Swipe control sits on the Pin switch, since that action is on at open.
- Flex: direction, wrap, justification, align items, align content, padding. Batches on wrap, Gap on padding.
- Uniform: max columns, max rows. First cell on max columns, Max rows on max rows.

Data on every page: catalog and sort. The About info control is on the right of the panel header. Reset and the timings switch share the flat row under that header. Item on every page: arrangement, width, height, font, margin. Arrangement has an info control. The height setting is what Card uses. Row and Varied disable it. Varied also disables width, because each card has its own.

### Usage

Models call `ILayoutUsage.Record`. Gesture names and details are the ones in the usage-log delta. `ViewInfo` records `Viewed {title}` with an empty detail. Catalog details are the catalog descriptions without the count. Sort details are the `ColorSortOption` names, such as "Hue (ascending)".

## Risks / Trade-offs

- [A swipe along the scroll fights the list] → Vertical lists swipe sideways. Horizontal lists swipe up for Delete and down for Pin and Copy.
- [Flex and Uniform also realize every child] → Flex adds 40 per turn. Both open on the small catalog and do not offer the 12-letter names.
- [The uniform manager ignores a later size change] → The page replaces the layout when the card changes. The First cell note says why.
- [Items past max rows stay in the tree and are not arranged] → The Max rows note says they are not arranged. The data is not trimmed, so raising the maximum shows them again.
- [Replacing the Mac menu while it closes hides the card] → The menu is replaced after the click, and the card's opacity is set back to 1. A mouse drag uses the swipe edges and does not need that menu.
- [Measure-all animates row heights during a Shell transition] → The Usage page already avoids this. Both CollectionView pages open on measure-first. The Sizing note says what measure-all does.
- [A grouped grid measures headers as well as rows] → Grouping opens off. The sizing behavior in the spec is about the items.

## Migration Plan

No stored data changes. Removing the pages leaves the usage log as it is. Old builds ignore action rows they do not know by name.
