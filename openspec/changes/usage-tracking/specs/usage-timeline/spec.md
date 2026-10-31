# Spec Delta

## Purpose

Shows the usage log as a newest-first nested timeline so a person can see where they spent time, including how long the current page has been open.

## ADDED Requirements

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
The timeline SHALL run from the present at the top to the oldest root at the bottom. The open page visit SHALL be the top row when one is open. Page visits in a session are newest first. Background and foreground events for a visit are nested under that visit, newest first, one level deeper than the visit. The session-started row for a session comes after that session's visits. Older sessions follow the same pattern. Installed or History reset is the last row.

#### Scenario: Current page above an earlier sleep
- **WHEN** the user opened Home for 30 seconds, then Colors, backgrounded, returned, then opened Usage
- **THEN** the row titles from top to bottom are "Navigated to Usage page", "Navigated to Colors page", Foregrounded, Backgrounded, "Navigated to Home page", Session started, and Installed
- **THEN** the row depths are 1, 1, 2, 2, 1, 0, and 0

### Requirement: Page visit rows
A page row title SHALL be "Navigated to {name} page", using the page name, otherwise the route, otherwise "Page". Its timestamp SHALL be the local time the visit opened. An open visit's detail SHALL be "in progress · open {elapsed} · active {active}". A closed visit's detail SHALL be "open {elapsed} · active {active}".

#### Scenario: Open visit during sleep
- **WHEN** Home has been open for 2 minutes and the app has then been backgrounded for 8 hours
- **THEN** the top row is titled "Navigated to Home page" and its detail contains "in progress", "open 8h 2m", and "active 2m"

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
A foreground event row SHALL be titled Foregrounded. A background event row SHALL be titled Backgrounded. The detail SHALL be empty. The timestamp column SHALL be the event's local time: time of day when it is the same calendar day as now, month and day plus time of day when it is another day in the same year, and month, day, year, plus time of day otherwise.

#### Scenario: Event earlier today
- **WHEN** a background event falls on the same local calendar day as now
- **THEN** its detail is empty and its timestamp column is the local time of day

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
- **THEN** the top row's duration text advances about once a second and the log gains no event for those ticks

#### Scenario: Leave the page
- **WHEN** the user navigates away from Usage
- **THEN** the one-second refresh stops

### Requirement: Timeline colors follow the theme
The rail, markers, and detail text SHALL use the app palette for the current theme. Row titles SHALL use the default label style.

#### Scenario: Light theme
- **WHEN** the app theme is light
- **THEN** the rail is Gray, detail text is DarkGray, an open visit marker is Primary, a closed visit marker is DarkGray, an activity marker is Gray, a session marker is DarkPrimary, and the root marker is DarkTertiary

#### Scenario: Dark theme
- **WHEN** the app theme is dark
- **THEN** the rail is LighterGray, detail text is LighterGray, an open visit marker is LightPrimary, a closed visit marker is LighterGray, an activity marker is LightGray, a session marker is Primary, and the root marker is Tertiary

### Requirement: Timeline row layout
Each row SHALL be at least 44 points tall, with a 2-point vertical rail and a 12-point circular marker. The rail, marker, and title SHALL indent 22 points per nesting level. Rows MUST NOT be selectable.

#### Scenario: Nested visit
- **WHEN** a foreground event is shown under a page visit
- **THEN** that event row is indented 22 points farther than the visit, and the visit is indented 22 points farther than the session row

### Requirement: Reset asks before clearing
Reset SHALL ask for confirmation with title "Reset history", message "This clears the usage log and starts over.", and actions Reset and Cancel. Cancel MUST leave the log unchanged. Confirm MUST reset history and show the new session with Usage as the open visit.

#### Scenario: Cancel
- **WHEN** the user taps Reset and then Cancel
- **THEN** the timeline is unchanged

#### Scenario: Confirm
- **WHEN** the user taps Reset and then Reset
- **THEN** the top row is "Navigated to Usage page" and the last row is History reset

### Requirement: Actions and area sit in the timeline
An area row SHALL use the same depth as a page visit and sort by its time among that session's visits, while an open visit stays the top row. A button row SHALL nest under its page, the menu or dialog under the button, and the selection under that surface. Siblings SHALL be newest first. Area, button, menu, and selection rows use the activity marker. Their detail is the mode for an area row and empty otherwise.

#### Scenario: Menu under the page
- **WHEN** Top Left was pressed on Menu Button and Mine was chosen
- **THEN** under "Navigated to Menu Button page" the rows are Top Left, Menu, and Mine at depths 2, 3, and 4

### Requirement: Timestamp column
Every row SHALL show its local timestamp in a column whose right edge lines up with the other rows. That column MUST NOT be indented with the row. Page rows use the open time. Activity, button, menu, selection, and area rows use their event time. Session rows use the start time. The root uses its date. The detail MUST NOT repeat that timestamp.

#### Scenario: Activity time is on the right
- **WHEN** a background event from today is shown
- **THEN** its detail is empty and the right-hand column is the local time of day
