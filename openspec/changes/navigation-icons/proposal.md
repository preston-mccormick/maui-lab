# Proposal

## Why

The bottom tab bar reserves a slot above each label, and every tab leaves that slot empty. The flyout is the same list of destinations with no glyphs. A matched set of icons makes each page recognizable, and the unselected tab color is currently the same as the bar, so a glyph drawn there would disappear.

## What Changes

- Add a Material Symbols Rounded icon for every flyout row and every tab. Home, Colors, Contacts, Geolocation, and Usage are flyout rows only. Layouts, Collections, and Forms keep one flyout row each, and their pages keep the existing bottom tabs and the More Layouts top tabs. Each of those tabs gets its own icon. A group's flyout icon is not reused on a child.
- Keep the selected tab colors. Change the unselected tab color, and the light-mode unselected shell color, so an icon is visible on the white bar and the white flyout. The flyout does not tint its bitmap, so each flyout row uses a white copy of its glyph in dark mode. Tab icons stay on the black files.
- Require the same treatment for a page added later: a new flyout row or tab ships with an icon from this set.

## Capabilities

### New Capabilities

- `shell-navigation`: The flyout and tab icons, the glyph assigned to each current destination, and the rule that a new shell entry brings its own icon.

### Modified Capabilities

## Impact

- `MauiLab/AppShell.xaml`: `FlyoutIcon` on each flyout row and `Icon` on each tab, including the four top tabs under More Layouts.
- `MauiLab/Resources/Images/nav`: twenty-three Material Symbols Rounded SVGs, 24 by 24, solid black on a transparent ground. `MauiLab.csproj` includes that folder as `MauiImage`, because `Resources/Images/*` does not pick up a subfolder.
- `MauiLab/Resources/Styles/Styles.xaml`: `Shell.TabBarUnselectedColor` becomes Gray in light and LighterGray in dark. Light-mode `Shell.UnselectedColor` becomes Gray. Selected tab colors stay DarkSecondary and White.
