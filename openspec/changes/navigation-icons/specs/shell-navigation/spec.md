# Spec Delta

## Purpose

Gives every flyout row and every tab a glyph from one matched set, and keeps those glyphs visible on the bar in both themes.

## ADDED Requirements

### Requirement: Flyout rows show their glyphs
Each flyout row SHALL show the glyph assigned to it beside its title. The rows and glyphs are Home `science`, Colors `palette`, Contacts `contacts`, Layouts `dashboard`, Collections `dataset`, Forms `dynamic_form`, Geolocation `location_on`, and Usage `timeline`.

#### Scenario: Open the flyout
- **WHEN** the user opens the flyout
- **THEN** each row shows its assigned glyph beside its title, in that order

#### Scenario: Dark flyout
- **WHEN** the app is in dark mode and the user opens the flyout
- **THEN** every row shows its glyph, including the rows that are not selected

### Requirement: Layout tabs show their glyphs
The Layouts bottom tabs SHALL show Grid Layout `grid_on`, Absolute Layout `open_with`, Dock Layout `view_sidebar`, and More Layouts `more_horiz` above their labels. The More Layouts top tabs SHALL carry Stack Layout `view_agenda`, Flex Layout `flex_direction`, Composite Layout `layers`, and Radial Layout `radar`.

#### Scenario: Layouts bottom tabs
- **WHEN** the user opens Layouts
- **THEN** the bottom tabs show `grid_on`, `open_with`, `view_sidebar`, and `more_horiz` above Grid Layout, Absolute Layout, Dock Layout, and More Layouts

#### Scenario: More Layouts top tabs
- **WHEN** the user opens More Layouts
- **THEN** the top tabs carry `view_agenda`, `flex_direction`, `layers`, and `radar` for Stack Layout, Flex Layout, Composite Layout, and Radial Layout

### Requirement: Collection tabs show their glyphs
The Collections bottom tabs SHALL show Grid `grid_view`, Linear `view_list`, Flex `flex_wrap`, and Uniform `view_module` above their labels.

#### Scenario: Collections bottom tabs
- **WHEN** the user opens Collections
- **THEN** the bottom tabs show `grid_view`, `view_list`, `flex_wrap`, and `view_module` above Grid, Linear, Flex, and Uniform

### Requirement: Forms tabs show their glyphs
The Forms bottom tabs SHALL show Form Controls `edit_note`, Dialogs `picture_in_picture`, and Menu Button `more_vert` above their labels.

#### Scenario: Forms bottom tabs
- **WHEN** the user opens Forms
- **THEN** the bottom tabs show `edit_note`, `picture_in_picture`, and `more_vert` above Form Controls, Dialogs, and Menu Button

### Requirement: A group glyph is not a child glyph
A flyout group's glyph MUST NOT be the same glyph as one of its tabs.

#### Scenario: Groups differ from their tabs
- **WHEN** the user compares Layouts, Collections, and Forms with the tabs inside them
- **THEN** `dashboard`, `dataset`, and `dynamic_form` do not appear on those tabs

### Requirement: Unselected tab glyphs stay visible
An unselected tab glyph SHALL be distinguishable from the tab bar in light mode and in dark mode. A selected tab glyph SHALL stay magenta on the light bar and white on the dark bar.

#### Scenario: Light tab bar
- **WHEN** the app is in light mode and a bottom tab is unselected
- **THEN** that tab's glyph is visible on the white bar

#### Scenario: Dark tab bar
- **WHEN** the app is in dark mode and a bottom tab is unselected
- **THEN** that tab's glyph is visible on the black bar

### Requirement: A new shell entry brings a glyph
A flyout row or tab added after this set SHALL show a glyph from the same matched set. Its glyph MUST be recorded with the entry.

#### Scenario: A later page is added
- **WHEN** a new page is added to the flyout or to a tab bar
- **THEN** that row or tab shows a glyph from the same set, and the shell navigation spec names it
