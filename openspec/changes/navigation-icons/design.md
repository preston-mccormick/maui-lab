# Design

## Context

See proposal.md for why this change exists. The glyph assigned to each destination is in the `shell-navigation` delta.

`AppShell.xaml` lists Home, Colors, and Contacts as top-level `ShellContent`s, then `FlyoutItem`s for Layouts, Collections, and Forms, then Geolocation and Usage. A `FlyoutItem` is one flyout row. Its direct `ShellContent` children are bottom tabs. More Layouts is a `Tab` whose children are top tabs. No item sets `Icon` or `FlyoutIcon`.

`Resources/Images/*` is a `MauiImage` include and is not recursive. Image names cannot contain hyphens. Shell tints a bottom-tab glyph with `TabBarForegroundColor` and `TabBarUnselectedColor`. Those are DarkSecondary (`#FC49FF`) and White for the selected state, and `LightBackground` / `DarkBackground` for the unselected state, which match the white and black tab bars. `Shell.UnselectedColor` is LighterGray (`#CCCCCC`) in both themes. `Shell.ForegroundColor` is Primary (`#78CDD4`) on every platform.

## Goals / Non-Goals

**Goals:**

- One black SVG per destination, drawn by the shell's own icon properties.
- Unselected tab glyphs that read on both bars, without moving the selected magenta and white.

**Non-Goals:**

- A custom flyout template, a font image, or a change to `Shell.ForegroundColor`. That color also paints the hamburger.
- Promising that iOS paints the four More Layouts glyphs. Those tabs still carry `Icon`.

## Decisions

### SVG files, not an icon font

Each glyph is a Material Symbols Rounded SVG at 24px, weight 400, fill 0. The files live in `Resources/Images/nav` and the project includes that folder as `MauiImage` with `BaseSize="24,24"`. A downloaded file whose fill is `currentColor` and nothing else is rewritten to a solid black fill before it is checked in, so the resizer does not emit an empty bitmap.

`FlyoutIcon` is set on each flyout row: the five lone `ShellContent`s and the three `FlyoutItem`s. `Icon` is set on each bottom-tab `ShellContent`, on the More Layouts `Tab`, and on the four top-tab `ShellContent`s inside it. A group does not reuse its children's names.

An icon font through `FontImageSource` was the alternative. One file would hold the set, and the glyph color would be baked in, so the tab bar's selected and unselected tint would be less reliable. Individual SVGs let the bar tint one alpha mask.

### Flyout rows get a white twin

The tab bar tints a black glyph, so one SVG covers both themes there. The flyout draws the bitmap as-is. A black glyph disappears on the dark menu, and the selected row only lifts it enough to look faint. Each flyout name therefore has a white copy, `science_dark` and the rest, and `FlyoutIcon` uses `AppThemeBinding` to pick the black file in light mode and the white file in dark mode. Tab `Icon` values stay on the black files.

### Unselected colors only

`Shell.TabBarUnselectedColor` becomes Gray (`#808080`) in light and LighterGray (`#CCCCCC`) in dark. Light-mode `Shell.UnselectedColor` becomes Gray. Selected tab colors stay DarkSecondary and White. Dark-mode `Shell.UnselectedColor` stays LighterGray, which already reads on the dark flyout.

### Where a later page records its glyph

The assignment list in `shell-navigation` is the record. Adding a page means adding its glyph there and a file beside the others. The layout, collection, and forms specs keep describing structure and do not repeat the names.

## Risks / Trade-offs

- [iOS top tabs stay text] → The four inner pages still have `Icon`, so a platform that draws a top-tab glyph uses the assigned one. The bottom bar is the place the empty slot is filled.
- [Unselected tab labels change with the icon color] → They use the same unselected color, so they become Gray and LighterGray too. That is the visible pair.
