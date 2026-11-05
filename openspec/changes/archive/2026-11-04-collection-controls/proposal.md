# Proposal

## Why

Colors is a CollectionView browser for looking up colors. Layouts has a playground for a handful of items. The large-set controls need their own pages, including a linear CollectionView whose items reveal Pin, Copy, and Delete with a swipe.

## What Changes

- Add a flyout group titled Collections, after Layouts. Give it four bottom tabs: Grid, Linear, Flex, and Uniform, so each page is created when its tab is shown. Page titles are CollectionView Grid Layout, CollectionView Linear Layout, FlexView, and UniformItemsView. The levers panel is titled Playground. Colors stays the color browser. The Layouts Flex page stays the small-set flex engine, and Composite stays the small-set uniform layout. Layouts puts Grid, Absolute, and Dock on their own bottom tabs, and groups Stack, Flex, Composite, and Radial as top tabs under a More Layouts bottom tab.
- Put each page on the existing layout playground. The collection fills the canvas. The docked panel is flat and its header reads Playground. Data, the control, and Item are floating cards. The control card is titled CollectionView, FlexView, or UniformItemsView. The header's right side describes that collection. Reset and the timings switch share a flat row under the header. Timings start off.
- Reuse the color catalogs as the workload. Every collection page opens on the named-color set, sorted by name, and can switch to the XKCD survey (949). Grid and Linear also offer the 12-letter names (3,099). Flex and Uniform do not. The catalog list is cached. A long file is read the first time that catalog is chosen. Each collection page is its own bottom tab, so the other pages stay uncreated until their tabs are shown. Grid fills when it is shown.
- Let every page change the card: a single-line row, the three-label card, or a card with a stable mix of widths and heights, plus width, height, font, and margin. Tapping a swatch paints the canvas with that color.
- Grid is a CollectionView grid. It plays with orientation, span, spacing, item sizing, grouping by first letter, and the scroll anchor used when the items change.
- Linear is a CollectionView with `LinearItemsLayout` and a `SwipeView` on each item. It plays with orientation, spacing, item sizing, the scroll anchor, and Pin, Copy, and Delete. Pin and Copy are on the leading edge. Delete is on the trailing edge: the right side of a vertical list, and a swipe up when the list is horizontal. A mouse drag on Mac Catalyst uses those same edges, and a secondary click lists them too. Pin and Delete start on, Copy starts off, and Delete is destructive. Pin marks the swatch, Copy inserts a duplicate after it, and Delete removes it from the displayed list.
- Flex plays with direction, wrap, justification, and alignment on a layout that realizes every child. A long list is added in batches.
- Uniform plays with max columns and max rows. The toolkit measures the first child once and stamps that size on every cell. This version of the layout has no orientation.
- Record one usage action per completed gesture on these pages, and show it on the timeline under the page that was open. Skip section collapse, the panel, the info banner, the timing notice, and the tap that paints the background.
- Rename the Experiments flyout group to Forms. Move Form Controls into that group with Dialogs and Menu Button. Remove the Scrolling Layout page. Inset the header info control and the timings info control so they line up with the info controls inside the cards.

## Capabilities

### New Capabilities

- `collection-playground`: The Collections group, the four collection pages, the shared data and item levers, the timings notice, and the swipe actions on the linear page.

### Modified Capabilities

- `usage-log`: A collection page records one action per completed gesture, with a verb, a noun, and a short detail.
- `usage-timeline`: That action appears as one row under the collection page that was open.
- `layout-playground`: Grid, Absolute, and Dock are separate bottom tabs. Stack, Flex, Composite, and Radial are top tabs inside More Layouts. The docked panel is flat and each section is a floating card. Scrolling Layout is removed.
- `form-controls`: Form Controls moves into a flyout group titled Forms, with Dialogs and Menu Button.

## Impact

- `MauiLab/AppShell.xaml`: a Collections `FlyoutItem` after Layouts, with four bottom tabs labeled Grid, Linear, Flex, and Uniform. Page titles are CollectionView Grid Layout, CollectionView Linear Layout, FlexView, and UniformItemsView, so the timeline can tell FlexView from Flex Layout. The levers panel is titled Playground. Layouts' bottom tabs are Grid Layout, Absolute Layout, Dock Layout, and More Layouts. The former Experiments group is Forms, route `forms`, holding Form Controls, Dialogs, and Menu Button. `ScrollingLayoutPage` is removed.
- `MauiLab/Collections` owns the pages, the swatch view, and the page models. The pages use `LayoutPlayground`, `LeversSection`, `NumberLever`, `SettingSeparator`, `InfoControl`, and `ILayoutUsage` from `MauiLab.Layouts`. The catalogs stay in `MauiLab.Colors`, cached, with the two long files deferred. No new NuGet package. Uniform Items uses `CommunityToolkit.Maui.Layouts.UniformItemsLayout` 15.0.1, already referenced.
- `MauiLab/Usage`: no new event kind and no schema change. Collection actions use the existing action event.
- `MauiLabTests` gains headless tests for the page models. Reveal gestures stay on the device. The timeline test covers one collection action under its page.
