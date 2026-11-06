# Shell lessons

## Unselected tab color was the bar color

`Shell.TabBarUnselectedColor` started as the same white and near-black as the tab bar. A glyph tinted with that color disappears into the bar, which is why the icon slot looked empty even after an image was assigned. Unselected tabs use Gray on the light bar and LighterGray on the dark bar. Selected tabs stay magenta in light mode and white in dark mode.

## The flyout does not tint its icons

The tab bar tints a black glyph, so one file works in both themes. The flyout paints the bitmap as it is. Those same black files vanish on the dark menu, and the selected row only lightens them enough to look faint. Flyout rows use a white copy in dark mode. Tab icons stay on the black file.
