# Spec Delta

## MODIFIED Requirements

### Requirement: Layouts is its own flyout group
The app SHALL include a flyout group titled Layouts. Its bottom tabs SHALL be Grid Layout, Absolute Layout, Dock Layout, and More Layouts, in that order. More Layouts SHALL contain Stack Layout, Flex Layout, Composite Layout, and Radial Layout as top tabs, in that order. Grid Layout MUST NOT remain under Forms.

#### Scenario: Open from the flyout
- **WHEN** the user opens the flyout
- **THEN** Layouts is its own group, and its bottom tabs are Grid Layout, Absolute Layout, Dock Layout, and More Layouts

#### Scenario: Experiments no longer contains Grid
- **WHEN** the user opens Forms
- **THEN** Grid Layout is not among its tabs

### Requirement: Layout tabs sit on top
Grid Layout, Absolute Layout, and Dock Layout SHALL each be their own bottom tab. More Layouts SHALL be the fourth bottom tab and SHALL show Stack Layout, Flex Layout, Composite Layout, and Radial Layout as top tabs, in that order. The app MUST NOT add its own overflow menu. Page titles SHALL stay "Grid Layout" and so on.

#### Scenario: Top tabs in order
- **WHEN** the user opens More Layouts
- **THEN** Stack Layout, Flex Layout, Composite Layout, and Radial Layout are top tabs in that order, and the grid page's title is still "Grid Layout"

### Requirement: The panel scrolls and its sections collapse
The docked panel SHALL be flat, with no shadow of its own. Each section SHALL be a floating card with a shadow. The panel SHALL stack its sections vertically and scroll when they exceed the panel. Each section SHALL collapse to its title and expand again. The panel SHALL include a Layout section, and an Item section on every page whose canvas has selectable items. The Item section SHALL name the selected item, or say that none is selected. Composite Layout has no selectable items and SHALL have no Item section.

#### Scenario: Collapse a section
- **WHEN** the user taps the Layout section's title
- **THEN** that section shows only its title, and the Item section is unchanged

#### Scenario: Nothing selected
- **WHEN** no item is selected
- **THEN** the Item section says that none is selected

### Requirement: The canvas fills and the panel docks
Each layout page SHALL show the layout on a canvas that fills the space the panel does not take. Below 800 points of width the panel SHALL dock to the bottom. At 800 and above it SHALL dock to the trailing edge. Crossing that width SHALL move the panel and give the freed space back to the canvas. While the panel is on the bottom, dragging its top edge SHALL resize the split, and that height SHALL follow the user to the next page.

#### Scenario: Narrow window
- **WHEN** a layout page is 799 wide
- **THEN** the panel sits along the bottom and the canvas fills the width above it

#### Scenario: Wide window
- **WHEN** the page widens from 799 to 800
- **THEN** the panel moves to the trailing edge and the canvas grows taller by the space the bottom panel released

#### Scenario: Drag the bottom split
- **WHEN** the page is narrower than 800 and the user drags the top of the panel upward
- **THEN** the panel grows taller and the canvas keeps the space that remains

## REMOVED Requirements

### Requirement: Scrolling Layout stays an experiment
