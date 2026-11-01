# Proposal

## Why

The lab shows dialogs, menus, and layouts, and it never shows the controls a form is built from. A top-level Form Controls page should put each built-in input, and the form pieces already in the Community Toolkit, on one scrollable page with its variants grouped together and validation visible. It should also be a reference for a real form: the controls bind to a view model, and the page can swap that view model for another so the rebind of a large form, with short, long, empty, valid, and invalid data, can be watched.

## What Changes

- Add a top-level flyout item titled Form Controls. It opens a page titled Form Controls and is not nested under Experiments.
- Show each built-in form control, with the major variants of that control grouped under a heading that names the control type. Each variant has a shorter caption.
- Lay the variants of a group out in one, two, or three columns as the page width changes. The page scrolls vertically through the groups.
- Use `Border` as the group box for most control types. Use `Expander` for Entry (starts expanded) and Editor (starts collapsed) so a group can open and close.
- Bind every value the form shows or edits to a form view model. Hold five of them, named Empty, Short, Long, Valid, and Invalid, each with different data in every group.
- Keep a bar above the scrolling form, always visible, that shows which sample is bound and cycles to the previous or next one, wrapping at the ends. Cycling swaps the bound view model and leaves the open and closed groups as they were.
- Show toolkit validation on Entry: required text, email, numeric range, an exact string, a character rule, a URL, and combined rules. A phone mask shows masked entry. Invalid data shows a message, and the message clears when the value becomes valid.
- Show `RatingView` with a star rating, a heart rating, and a read-only rating.
- Leave `TableView`, `TabbedPage`, `Frame`, and any new package out. Leave toolkit popups, avatars, and drawing on their existing pages.

## Capabilities

### New Capabilities

- `form-controls`: A top-level page that demonstrates built-in and toolkit form controls, grouped by type, bound to one of several sample view models that an always-visible bar cycles through, with responsive variant columns and live validation.

### Modified Capabilities

- None.

## Impact

- `MauiLab/AppShell.xaml` gains one top-level `ShellContent`.
- A new `MauiLab/FormControls` folder: the page, a form view model, a gallery that holds the samples and the current one, and a sample factory. No new NuGet package: `CommunityToolkit.Maui` 15.0.1 and `CommunityToolkit.Mvvm` are already referenced.
- Existing implicit control styles stay as they are. Invalid fields use a style local to the page.
- `MauiLabTests` gains tests for the column breakpoints, the sample data, and cycling. Code-behind is limited to reporting each group's width.
