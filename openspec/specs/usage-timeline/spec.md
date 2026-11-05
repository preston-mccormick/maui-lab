# usage-timeline Specification

## Purpose

Shows the usage log as a newest-first nested timeline so a person can see where they spent time, including how long the current page has been open.

## Requirements

### Requirement: Usage is a flyout page
The app SHALL include a flyout item titled Usage that opens the usage timeline page. The page title SHALL be Usage.

#### Scenario: Open the experiment
- **WHEN** the user chooses Usage in the flyout
- **THEN** the usage timeline page is shown

### Requirement: Page header
The page SHALL show the title Usage, the subtitle "Sessions, pages, and time in the foreground.", and a Reset button. The subtitle SHALL use DarkGray text in the light theme and LighterGray text in the dark theme.

#### Scenario: Header is visible
- **WHEN** the usage page is shown
- **THEN** the title, that subtitle, and a Reset button are visible

### Requirement: Newest-first nested order
The timeline SHALL run from the newest event at the top to the oldest root at the bottom. An event that happens after a page navigation SHALL appear above that navigation row and one level deeper. A newer page returns to the page depth, then indents again for what happens on that page. The session-started row sits below that session's pages. Installed or History reset is the last row.

#### Scenario: Current page above an earlier sleep
- **WHEN** the user opened Home for 30 seconds, then Colors, backgrounded, returned, then opened Usage
- **THEN** the row titles from top to bottom are "Navigated to Usage page", Foregrounded, Backgrounded, "Navigated to Colors page", "Navigated to Home page", Session started, and Installed
- **THEN** the row depths are 1, 2, 2, 1, 1, 0, and 0

### Requirement: Page visit rows
A page row title SHALL be "Navigated to {name} page", using the page name, otherwise the route, otherwise "Page". Its timestamp SHALL be the local time the visit opened, including seconds. An open visit's detail SHALL be "in progress · open {elapsed} · active {active}". A closed visit's detail SHALL be "open {elapsed} · active {active}".

#### Scenario: Open visit during sleep
- **WHEN** Home has been open for 2 minutes and the app has then been backgrounded for 8 hours
- **THEN** the top row is Backgrounded, and the "Navigated to Home page" row contains "in progress", "open 8h 2m", and "active 2m"

#### Scenario: Closed visit after sleep
- **WHEN** that Home visit later gains 3 more foreground minutes and the user leaves it
- **THEN** the Home row detail is "open 8h 5m · active 5m"

### Requirement: Duration text
A duration SHALL be formatted from whole seconds. At least one hour shows hours, plus minutes when minutes are nonzero. Under one hour and at least one minute shows minutes, plus seconds when seconds are nonzero. Under one minute shows seconds. A negative span SHALL display as 0s.

#### Scenario: Hour scale drops seconds
- **WHEN** a duration is 8 hours 5 minutes
- **THEN** the text is "8h 5m"

#### Scenario: Exact minutes
- **WHEN** a duration is 5 minutes
- **THEN** the text is "5m"

#### Scenario: Seconds only
- **WHEN** a duration is 30 seconds
- **THEN** the text is "30s"

#### Scenario: Minutes and seconds
- **WHEN** a duration is 1 minute 1 second
- **THEN** the text is "1m 1s"

### Requirement: Activity rows
A foreground event row SHALL be titled Foregrounded. A background event row SHALL be titled Backgrounded. The detail SHALL be empty. The timestamp column SHALL be the event's local time, including seconds: time of day when it is the same calendar day as now, month and day plus time of day in the same year, and month, day, year, plus time of day otherwise.

#### Scenario: Event earlier today
- **WHEN** a background event falls on the same local calendar day as now
- **THEN** its detail is empty and its timestamp column is the local time of day including seconds

### Requirement: Session rows
A session row SHALL be titled "Session started". Its timestamp column SHALL be the local start time. While the session is open, the detail SHALL be empty. When the session has ended, the detail SHALL include elapsed and active time and MUST NOT repeat the start time. A terminated session's detail SHALL include the word "terminated".

#### Scenario: Open session
- **WHEN** the current session has not ended
- **THEN** its timestamp column is the local start time, its detail is empty, and it does not include "terminated"

#### Scenario: Terminated session
- **WHEN** a previous session ended because the process was killed
- **THEN** that session row's detail contains "terminated" and the session's elapsed and active durations

### Requirement: Root row
The last timeline row SHALL be titled Installed or History reset. Its timestamp column SHALL be the local date as month and day when the year is the current year, and month, day, and year otherwise. Its detail SHALL be empty.

#### Scenario: Install in the current year
- **WHEN** the log's root is Installed and that date is in the current local year
- **THEN** the last row is titled Installed, its detail is empty, and its timestamp column is the month and day

#### Scenario: After reset
- **WHEN** history has been reset
- **THEN** the last row is titled History reset

### Requirement: The open visit updates while the page is visible
While the Usage page is appearing, the open visit's duration text SHALL refresh once per second. That refresh MUST NOT record an event. When the row identities are unchanged, only detail text updates and the scroll offset is kept. When the user leaves the page, the refresh MUST stop.

#### Scenario: Sitting on the timeline
- **WHEN** the Usage page is visible and the current visit stays open
- **THEN** the open visit row's duration text advances about once a second and the log gains no event for those ticks

#### Scenario: Leave the page
- **WHEN** the user navigates away from Usage
- **THEN** the one-second refresh stops

### Requirement: Category colors
The node and the title SHALL share a category color. Navigation is blue. Screen is green. Lifecycle, including foreground, background, session, install, reset, and update, is pink. Button, menu, and selection rows are amber. Permission rows are violet. The graph line is Gray in the light theme and LighterGray in the dark theme. Detail and timestamp text stay DarkGray in the light theme and LighterGray in the dark theme.

#### Scenario: Dark theme categories
- **WHEN** the app theme is dark
- **THEN** a page title is blue, a screen title is green, a lifecycle title is pink, an action title is amber, and a permission title is violet

### Requirement: Connected graph
Rows SHALL meet with no gap. A line SHALL join the nodes. When the next older row is shallower, the line SHALL curve from this node into that row's column. When the next older row is deeper, this column SHALL continue and that row SHALL start its branch. The title starts just to the right of its node. The timestamp stays on the right edge.

#### Scenario: Nested visit
- **WHEN** a foreground event is shown above its page visit
- **THEN** that event is indented 22 points farther than the visit, and its line curves down into the visit's column

### Requirement: Timeline row layout
Each row SHALL be at least 44 points tall. Rows MUST NOT be selectable. The timestamp column MUST NOT be indented.

#### Scenario: Row chrome
- **WHEN** any event is shown
- **THEN** that row is at least 44 points tall and cannot be selected

### Requirement: Reset asks before clearing
Reset SHALL ask for confirmation with title "Reset history", message "This clears the usage log and starts over.", and actions Reset and Cancel. Cancel MUST leave the log unchanged. Confirm MUST reset history and show the new session with Usage as the open visit.

#### Scenario: Cancel
- **WHEN** the user taps Reset and then Cancel
- **THEN** the timeline is unchanged

#### Scenario: Confirm
- **WHEN** the user taps Reset and then Reset
- **THEN** the top row is "Navigated to Usage page" and the last row is History reset

### Requirement: Actions and screen rows
A screen row's title SHALL be its mode, or Screen when no mode was set, and its detail SHALL be the width and height. It SHALL appear above the page that was open, one level deeper. A selection SHALL appear above its menu or dialog, which SHALL appear above its button, which SHALL appear above the page. Each step is one level deeper than the earlier one.

#### Scenario: Menu above the page
- **WHEN** Top Left was pressed on Menu Button and Mine was chosen
- **THEN** above "Navigated to Menu Button page" the rows are Mine, Menu, and Top Left at depths 4, 3, and 2

#### Scenario: Split view title
- **WHEN** a split-view size is recorded
- **THEN** the title is "Split View" and the detail is the width and height

### Requirement: Timestamp column
Every row SHALL show its local timestamp, including seconds, in a column whose right edge lines up. That column MUST NOT be indented. Page rows use the open time. Other event rows use their event time. Session rows use the start time. The root uses its date. The detail MUST NOT repeat that timestamp.

#### Scenario: Activity time is on the right
- **WHEN** a background event from today is shown
- **THEN** its detail is empty and the right-hand column is the local time of day

### Requirement: A layout action sits under its page
A layout action SHALL appear above the page that was open, one level deeper than that page. Its title SHALL be the action's name. Its detail SHALL be the action's short value, and the detail SHALL be empty when the value is empty. The title SHALL use the amber action color. The action MUST NOT appear as a button, a surface, and a choice.

#### Scenario: Added row under Grid Layout
- **WHEN** the user added a row on Grid Layout and that visit is shown
- **THEN** above "Navigated to Grid Layout page" there is one row titled "Added row" at one level deeper, its detail is the row count, and the title is amber

#### Scenario: Viewed info has no detail
- **WHEN** a "Viewed Cross axis" action is shown
- **THEN** the title is "Viewed Cross axis" and the detail is empty

### Requirement: A collection action sits under its page
A collection action SHALL appear above the page that was open, one level deeper than that page. Its title SHALL be the action's name. Its detail SHALL be the action's short value, and the detail SHALL be empty when the value is empty. The title SHALL use the amber action color. The action MUST NOT appear as a button, a surface, and a choice.

#### Scenario: Deleted swatch under Linear
- **WHEN** the user deleted a swatch on the Linear page and that visit is shown
- **THEN** above "Navigated to CollectionView Linear Layout page" there is one row titled "Deleted swatch" at one level deeper, its detail is the color name, and the title is amber

#### Scenario: Viewed swipe has no detail
- **WHEN** a "Viewed Swipe" action is shown
- **THEN** the title is "Viewed Swipe" and the detail is empty
