# Design

## Context

See proposal.md for why this change exists. Behavior is in the `layout-playground`, `usage-log`, and `usage-timeline` deltas. The info banner, the nested stacks, the absolute drag-to-resize, and the flex all-items editing came out of the first on-device review.

`MauiLab/Layouts` already has Grid Layout (a full-bleed grid rebuilt in code-behind, tracks on `GridLayoutPageModel`, washes drawn behind `r,c` labels), an unused `LayoutLevers` view left over from the deleted dialog page, and Scrolling Layout, which stays under Experiments. `FormColumnsLayout` is the existing custom layout: a `Layout` plus an `ILayoutManager`. The test project references the app and already calls `GridLayoutPageModel` with no handlers. `UsageTracker` records menu and dialog actions as three events (`ControlPressed`, `SurfaceOpened`, `SelectionMade`). `UsageEvent` has no free-text detail; `SqliteUsageStore.EnsureColumns` adds columns with `ALTER TABLE` and does not rewrite old rows. `CommunityToolkit.Maui` 15.0.1 is already referenced. It supplies Snackbar, Toast, and `DockLayout`. That dock layout positions children to the left, top, right, or bottom, treats `None` like top when arranging, defaults `ShouldExpandLastChild` to true, and has horizontal and vertical spacing. Its only other layout is `UniformItemsLayout`, which measures the first child once and reuses that size.

## Goals / Non-Goals

**Goals:**

- One playground shell, used by all seven pages, built on the toolkit `DockLayout`.
- Page models that hold the settings and the items, bind to the layouts, and stay testable without a device.
- Radial placement tested as pure math, with the manager only applying it. Dock arrangement stays the toolkit's.
- One usage row per completed gesture, threaded through the models so tests can see it.

**Non-Goals:**

- A Collections group, or any change to the Colors page. Flex and Uniform as a large set of items belong there, later.
- Changing Scrolling Layout.
- A second dock implementation. The playground and the Dock page both use the toolkit `DockLayout`.
- A separate Uniform page or a separate Columns page. Those two layouts appear only as ways to build the composite field. The Flex page remains the engine playground.
- A splitter, new image assets, or virtualizing any of these layouts.
- Recording selection, section collapse, panel collapse, or the panel's own edge change.

## Decisions

### Layouts group, Grid moves

`AppShell` gains a `FlyoutItem` titled Layouts, route `layouts`, placed where Experiments is now, and Experiments follows it. The seven pages sit in one `Tab`, so they are top tabs, in order: Grid Layout, Stack Layout, Absolute Layout, Flex Layout, Composite Layout, Dock Layout, Radial Layout. Grid Layout's `ShellContent` moves out of Experiments. Scrolling Layout stays. This change does not build a More menu. Page titles stay "Grid Layout" and so on, so the timeline reads "Navigated to Grid Layout page".

### The playground is the toolkit DockLayout

Every page is a `CommunityToolkit.Maui.Layouts.DockLayout` with `ShouldExpandLastChild` true. The panel is the first child. The canvas is the last child and fills. `LayoutPanelEdge.ForWidth` returns Bottom below 800 and Right at 800 and above, from the page width. The page sets `DockLayout.SetDockPosition` on the panel when that changes. Right is the physical right; the app is not localized. A phone is under 800, so the panel sits below the canvas. A tablet at 800 or wider puts the panel on the right. An iPad mini in portrait is about 744, so it keeps the panel below.

The panel is 320 wide when docked right, and 280 tall when docked bottom. Inside it: a header row and a `ScrollView`. The header is a two-column grid: a bold 22-point glyph in a 24-wide column, then a bold 18-point title, so the two never overlap. Tapping it toggles `IsExpanded` on the panel. Collapsed on the right, the title hides and the strip is 44 wide; collapsed on the bottom, the title row stays. `LayoutPlayground` is the content view that owns this chrome. Pages pass the canvas as one property and the sections as the other; the playground drops each into a plain `ContentView` host. `ContentPresenter` is not used: outside a `ControlTemplate` it renders nothing.

Sections are a `LeversSection`: a title row that toggles its body. `LayoutLevers` is the shared item-property list and sits inside a section rather than drawing its own header. Each of its settings is its own row, so a long label does not narrow the value on another row. Collapse state stays on the views. It is not recorded and does not belong on the models.

Every page's panel has a Layout section holding that layout's settings and the add commands, with an info button beside each setting that needs one. Pages with selectable items also have an Item section: `LayoutLevers` bound to the selected item's view, plus the attached-property fields bound to the item model. With nothing selected it shows "None selected" (Flex instead edits all items). Composite has no Item section.

### One item model

`LayoutItem` is the child on every page except Grid's tracks, which stay `GridTrack`. Kind is Rectangle, Ellipse, Triangle, Star, Image, Panel, or Stack. Shared fields: color, z, rotation, opacity, clip, margin, horizontal and vertical options, and a text for the panel's entry. Per-layout fields live on the same object so a template can bind them: grid row, column, and spans; absolute bounds and flags, with `X`, `Y`, `Width`, and `Height` as scalar views of the bounds for entries; flex grow, shrink, basis, basis-as-percent, order, and align-self; `LayoutDock`; radial angle override (`null` means even spacing); and for a Stack item, `Children`, `IsVertical`, and `Spacing`. A page ignores the fields it does not show.

`LayoutItemView` is the one template. It wraps content in an unrotated cell. Rotation, opacity, and clip apply to the inner view. The selection frame is a sibling of that inner view, so it stays the axis-aligned rectangle the layout assigned. That is the render-versus-layout lesson, and it is why shapes, images, rotation, opacity, and clip are on the pages. The view owns the tap gesture and reports to the nearest ancestor that implements `ILayoutItemHost`; a tap on a nested item that also reaches its stack is dropped by a short window. Grid, Stack, Flex, Dock, and Radial also tap the layout itself. That tap clears the selection when the point misses every item view, and does nothing when it lands on one, so it does not undo the item's own tap. Flex returns to editing all items. A Stack item renders a dashed border around a vertical or horizontal stack of nested `LayoutItemView`s bound to `Children`.

Images on the other pages are the bundled pngs already in `Resources/Images`: apple, book, rose, diamondsword, gold_ingot, sign, steelpick, stonehoe. Absolute Layout uses the veggie SVGs instead. A panel item is a `Border` with a label and an entry bound to `LayoutItem.Text`.

### Binding, with one grid exception

Stack, Absolute, Flex, Dock, and Radial put `Items` on a `BindableLayout` and bind attached properties in the template. One `LayoutItemView` shows the kind by visibility: rectangle, ellipse, triangle, star, image, panel, or stack.

Grid binds `ColumnDefinitions` and `RowDefinitions` to collections the model rebuilds when a track changes. Washes move to a background grid bound to the same definitions, generated when the tracks change, behind the bound children. The per-cell `r,c` labels go away; an Auto track with no child collapses, which is the grid behavior worth seeing. The page still forces measure when an Auto track changes and the grid's own bounds do not, which is the workaround `GridLayoutPage` has now.

`AbsoluteLayout.LayoutBounds` is bound. A long press on the item, then a drag, updates the model on each move so the item follows the finger, and records once when the finger lifts. The recognizer lives on `LayoutItemView` (behind `CanDrag`), not in the item template: a pan declared in the template never received the touch on iOS. `AbsolutePlacement.Move` converts a translation into absolute or proportional coordinates given the canvas size and the flags. Random placement is `AbsolutePlacement.Random` with an injected `Random`.

### Toolkit dock, custom radial

The Dock page binds the toolkit layout directly: `DockLayout.DockPosition` on each item, `ShouldExpandLastChild`, `HorizontalSpacing`, and `VerticalSpacing`. Positions are `Left`, `Top`, `Right`, `Bottom`, and `None`. `None` is the toolkit default and, inside a dock that has been given a size, arranges like `Top`. The page opens with expand-last on and five rectangles that cover the canvas: a top bar and a bottom bar across the full width, a left bar and a right bar filling the height between them, and a last item with no size request that fills the middle. Turning expand-last off docks that last item to its own edge. The edge picker stays available on the last item. No lab code reimplements this arrangement; the toolkit's unit tests own it.

`RadialLayout` is the one custom layout, in `MauiLab.Layouts`, following `FormColumnsLayout`. `RadialLayout.Angle` is an attached `double?`. `RadialPlacement.Place` takes the center, radius, start, sweep, and each item's angle override, and returns the center point of each item. Items with an override drop out of the even spacing; the rest divide the sweep. Angles are degrees clockwise from the top. A radius of 0 means fit: half the shorter side, minus half the largest item. Tests call `RadialPlacement` directly. The manager only applies those points and skips children that are not visible.

### Numeric levers

A numeric setting in a levers panel is a `NumberLever`: an 80-point-wide entry with right-aligned text, sitting at the trailing edge, and a slider that lands on whole numbers. `NumberScale` maps the slider. Beside a label, the slider moves to the next row of the parent grid and spans that row, and the label stays centered on the entry. A value with no label, such as a grid track, keeps the entry and the slider on one row: the slider on the left, filling the space left by that 80-point entry on the right, with a gap under the control above. The entry stays 80 points wide at the trailing edge when it sits beside a label. A horizontal line separates settings in the levers panels. Group labels such as Columns and Rows are bold. Each item-lever row sizes its own label, so MinimumWidthRequest does not narrow Height and Width; those measured values stay on one line at the trailing edge. A request whose unset value is -1 has a stop below the normal range, and a request with no maximum has a stop for positive infinity. The radial angle uses the low stop for no override, which is an empty entry. Typing still sets the value, including a fraction. A drag updates the layout as it moves and records once when the finger lifts. The sample field entries and an item's text entry are not levers.

### Composite field

The Composite page is not another item playground and not a collection. It hosts one field, in this order: a label "Name" with `WidthRequest` 120, an entry with `MinimumWidthRequest` 160, a description, and a validation message that is taller than the label. Description and validation start hidden. A width control sets the field host's `WidthRequest`, opening at 320, and records when the edit finishes. There is no Item section; the parts are not selectable items.

Three hosts share those parts, and the mode shows one of them:

- Flex: `Direction` row, `Wrap` wrap. The entry has `Grow` 1. The description and the validation have `Basis` of 100%, so each takes the next line at its own height. At 320 the label and the entry share a line. At 200 the entry wraps and keeps its height.
- Uniform: the toolkit `UniformItemsLayout`, label first. Every cell is the label's size, including a shown validation message. That is the point of putting Uniform next to the others.
- Columns: the existing `FormColumnsLayout`. Column count still comes from `FormColumnCount` of the width it is given, so the field's width is what matters, not the window. The breakpoints stay 640 and 960. A phone-width field stays one column; the width control is how the user reaches two and three. A row grows to its tallest part.

`FormColumnsLayout` stays in `MauiLab.FormControls`. The page uses it; it does not move.

### Info

`InfoControl` is a 24-point round blue `Button` with a bold "i", top aligned, and has `Title` and `Text`. It sits in the same grid row as the setting it explains. On tap it raises `InfoTapped`, which the page uses to record `Viewed {title}`, then finds its `LayoutPlayground` ancestor and calls `ShowInfo(title, text)`.

The playground shows the info as a banner floating over the top of the page, layered above the `DockLayout` in a `Grid`, so it takes no layout space: title, text, and a close button. It stays until closed or until another info control replaces it. The toolkit Toast and Snackbar were tried and dropped: the snackbar had no clear dismiss and competed with the panel for the bottom edge. No info-style setting remains on the models.

Titles, so the usage scenario has a real string: Grid "Tracks" and "Cells", Stack "Cross axis", "Spacing", and "Nesting", Absolute "Background", "Moving", "Flags", and "Rotation", Flex "Wrap", "Justify", "All items", "Grow", and "Margin", Composite "Equal cells" and "Parts", Dock "Expand last" and "Edges", Radial "Sweep" and "Angle override".

### Usage

`UsageEventKind.ActionTaken` is appended by `UsageTracker.RecordAction(string name, string? detail)`. Name and detail are stored as-is. Detail is a new nullable `Detail` column, added in `EnsureColumns` the way `ParentId` was. The event's page visit is the open one. No parent id, so it does not join the button/surface/choice chain. With no open visit, `RecordAction` returns without writing. `UsageTimeline` maps it to one row at depth 2, title from the name, detail from the detail, category Action (amber).

`ILayoutUsage.Record(name, detail)` is what models call. `TrackerLayoutUsage` wraps `UsageServices.Current` and swallows a failure from the tracker so a bad write cannot take down the gesture. Tests pass `ListLayoutUsage`. Models take `ILayoutUsage` in the constructor; the existing `GridLayoutPageModel()` constructor keeps a no-op default so current tests still compile.

Commands record. Entries record from `Completed` and `Unfocused` through a `Commit` method that no-ops when the value is unchanged. A drag records from its completed handler only. Removing the last grid track does not record, matching the command that already refuses.

Gesture names and details are the ones in the usage-log delta. Orientation details are `Vertical` and `Horizontal`.

### Stack modes and nesting

One `Items` collection feeds two layouts whose visibility follows the mode: `VerticalStackLayout` and `HorizontalStackLayout`. The legacy `StackLayout` is left out. It is not marked obsolete, but its `*AndExpand` options are, and the docs steer new code to the two newer stacks. A Stack `LayoutItem` nests another stack: `AddStack` creates one running across the target's axis with two rectangles, and while a stack is selected `AddOne` and `AddSet` add into its `Children`. `Remove` searches nested stacks. The Item section for a stack shows its orientation and spacing.

### Absolute editing

A tap on the canvas outside every item, and outside the resize handle, clears the selection. The canvas tap ignores a point that lands on an item, so it does not undo the item's own tap. A long press on the item, reported through `LayoutItemView.Dragged`, moves it through `AbsolutePlacement.Move` by the finger's travel in the canvas, updating the model on each move and recording once on completion. The shell flyout pan takes any touch that starts in the leading 10% of the screen, which is where the first illustration sits, so on iOS and Mac Catalyst that pan declines a touch that hits a draggable item. A swipe from empty canvas at that edge still opens the flyout. A canceled press puts the item back. Absolute items are created with no `WidthRequest` or `HeightRequest`; `LayoutBounds` is their only size. MAUI fills the arranged bounds only when the view has no explicit size, so an item with a request would stay a fixed box centered in its growing bounds and a resize would look like a slide. One resize handle lives in a second `AbsoluteLayout` layered over the items, not inside the item view: a handle nested in the item shared the item's pan and moved it instead. The model exposes `HandleBounds`, the pixel rectangle centered on the selected item's bottom-right corner, recomputed when the selection, its bounds, its flags, or the canvas size change. A pan on it resizes through `AbsolutePlacement.Resize`, which honors proportional size flags and a minimum side. `X`, `Y`, `Width`, and `Height` entries edit the same bounds and record through `CommitPosition` and `CommitSize`. `AddRandom` picks one of the veggie illustrations and a rectangle anywhere inside `CanvasSize`, which the page sets from the canvas grid's own `SizeChanged`. The illustrations are the SVGs in `Layouts/Veggies`. Each file started with a white rectangle covering the view box; that rectangle is removed so the background is transparent, and the SVGs are included as `MauiImage` at 256. The resource name is the file name plus `.png`, with hyphens turned into underscores because a MAUI image name cannot contain a hyphen.

### Flex items without a selection

`FlexLayoutPageModel.AllItems` is a stand-in `LayoutItem`. `EditTarget` is the selection or that stand-in, and the Item section binds to it. A commit with no selection writes the value to every item and records with "(all)" in the detail. Tapping the selected item again clears the selection. Flex has no spacing property, so `Padding` on the model insets the layout and `ItemMargin` on each item is the gap between items.

### Radial invalidation

`RadialLayout`'s start, sweep, radius, and attached angle do not change any child's size, so nothing asks for a new layout pass on its own. Their `propertyChanged` callbacks call `InvalidateMeasure` on the layout.

### What each page starts with

- Grid: unchanged seeded tracks (star 1/4/1, spacing 48, the seed colors) and no children.
- Stack: vertical, spacing 8, three rectangles.
- Absolute: solid background and four veggie illustrations (broccoli, red bell pepper, corn, mushroom).
- Flex: row, wrap on, four rectangles of mixed requested sizes.
- Composite: Flex, width 320, label and entry only.
- Dock: expand-last on, top and bottom bars across the full width, left and right bars between them, and a last item that fills the middle. Spacing starts at 0.
- Radial: start 0, sweep 180, radius 0 (fit), four items.

## Risks / Trade-offs

- [Seven top tabs may not fit a phone] → Shell scrolls the top tab strip. The app does not add a More menu. Page titles stay long, so the strip scrolls.
- [Bottom panel covers the canvas on a phone] → The panel collapses to a strip, and it is 280 tall rather than half the page.
- [A message along the bottom collides with the panel] → Info is a banner across the top of the page, not a toast or snackbar.
- [Auto grid tracks do not relayout when only a definition changes] → Keep the existing forced measure on the grid page.
- [A `LayoutItem` carries every page's fields] → One bindable shape for the templates and the tests. Pages show only their own fields.
- [Detail column on an existing table] → Nullable, added with `ALTER TABLE`, old rows unchanged. A failed alter is the same class of failure the store already survives for the other added columns.

## Migration Plan

No data migration beyond the new nullable column. Rolling back the app leaves an unused `Detail` column, which old builds ignore.
