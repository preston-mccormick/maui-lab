# Spec Delta

## Purpose

Records how a person moves through the app: sessions, pages, and how much of that time the app was in the foreground, kept on the device for a later upload.

## ADDED Requirements

### Requirement: First launch records an install
The system SHALL record an Installed event when a session starts and the log has no events. A later launch MUST NOT record another Installed event.

#### Scenario: Empty log
- **WHEN** the app becomes active and the log is empty
- **THEN** the oldest event is Installed, followed by a session start and the current page visit

#### Scenario: Existing log
- **WHEN** the app becomes active and the log already contains events
- **THEN** no additional Installed event is recorded

### Requirement: A session stays open across background
A session SHALL start when this process becomes active with no open session. Returning to the foreground during that session MUST record a foreground event and MUST NOT start another session. The cold start itself MUST NOT also record a foreground event.

#### Scenario: Cold start
- **WHEN** the app becomes active with no session open in this process
- **THEN** the log gains one session start and no foreground event

#### Scenario: Resume
- **WHEN** the app returns to the foreground during an open session on the same page
- **THEN** the log gains one foreground event for that page visit and still has one session start

### Requirement: Background is one event
Leaving the foreground during an open session SHALL record one Backgrounded event. A further leave signal while already backgrounded MUST be ignored. Background and suspend are the same event.

#### Scenario: Deactivate then stop
- **WHEN** the window deactivates and then stops
- **THEN** the log contains one Backgrounded event for that transition

### Requirement: A page visit stores elapsed and active time
Closing a page visit SHALL store elapsed time and active time on that close. Elapsed time is the span from open to close. Active time SHALL include only foreground slices. A visit starts in the foreground unless it opens while the session is already backgrounded.

#### Scenario: Sleep on a page
- **WHEN** a page is open for 2 minutes, the app is backgrounded for 8 hours, the app returns for 3 minutes, and the user leaves the page
- **THEN** that visit's elapsed time is 8 hours 5 minutes and its active time is 5 minutes

#### Scenario: Several sleeps
- **WHEN** a visit includes more than one background and foreground cycle before it closes
- **THEN** active time is the sum of the foreground slices and elapsed time includes the background gaps

#### Scenario: Stay in the foreground
- **WHEN** the user leaves a page and the app was never backgrounded during that visit
- **THEN** active time equals elapsed time

#### Scenario: Still open while asleep
- **WHEN** a page visit is still open after the app has been backgrounded
- **THEN** elapsed time includes the time since the visit opened and active time stops at the background event

#### Scenario: Opened while backgrounded
- **WHEN** a page visit starts while the session is already backgrounded
- **THEN** active time stays at zero until a later foreground event

### Requirement: The same page is one visit
Navigation to the page that is already open SHALL NOT close or split the visit. When both the open visit and the navigation have a title, the title is the page identity.

#### Scenario: Route filled in later
- **WHEN** a visit opened with a title and a later navigation reports that same title plus a route
- **THEN** the log still has one open event for that visit and no close event

### Requirement: Dialogs stay inside the current page
Alerts, prompts, and action sheets SHALL NOT be recorded as their own page visits.

#### Scenario: Alert on a page
- **WHEN** the user opens an alert, prompt, or action sheet and then dismisses it
- **THEN** the current page visit stays open and no new page visit is recorded

### Requirement: Popup hosts are not page visits
A navigation whose page type or route is a Community Toolkit PopupPage SHALL NOT open or close a page visit.

#### Scenario: Menu popup
- **WHEN** Menu Button is the open page and navigation reports a PopupPage route
- **THEN** the Menu Button visit stays open and no page visit is recorded for that route

### Requirement: Window area
The system SHALL record window width and height, in device-independent points, when a session starts and again when either dimension changes by at least one point. A mode of Full screen, Split View, Slide Over, or Windowed SHALL be included only when the rectangle distinguishes it. When a compact full-height window also matches a split fraction, the mode MUST be left unset.

#### Scenario: Session start
- **WHEN** a session starts and the window is 1024 by 768 on a matching screen
- **THEN** the session contains an area record of width 1024, height 768, and mode Full screen

#### Scenario: Size change
- **WHEN** that window later changes by at least one point
- **THEN** the session contains a second area record with the new width and height

#### Scenario: Ambiguous compact width
- **WHEN** the window is full height, edge aligned, and its width matches both a one-third split and a compact overlay
- **THEN** the area record stores the size and has no mode

### Requirement: Menu and dialog actions
On Menu Button, pressing a menu button SHALL record that button, then Menu, then the chosen item or Dismissed. On Dialogs, Show Alert, Show ActionSheet, and Show Prompt SHALL record the button, then Alert, Action sheet, or Prompt, then the result or Dismissed.

#### Scenario: Menu choice
- **WHEN** the user presses Top Left and chooses Mine
- **THEN** that page visit contains Top Left, with Menu under it, and Mine under Menu

#### Scenario: Prompt dismissed
- **WHEN** the user presses Show Prompt and dismisses it without a result
- **THEN** that page visit contains Show Prompt, with Prompt under it, and Dismissed under Prompt

### Requirement: Destroying the window ends the session
Destroying the window SHALL close the open page visit and end the session at that time with reason Clean. Session active time SHALL be the sum of the closed visits' active times in that session.

#### Scenario: Close while in the foreground
- **WHEN** the window is destroyed one minute after a foreground visit opened
- **THEN** the visit and the session end at that time, the end reason is Clean, and both elapsed and active time are one minute

### Requirement: A killed process closes at the last known time
When a new process starts and a previous session has no end, the system SHALL close that session before starting a new one. If the last signal in the old session is backgrounded, the close time MUST be that background time. Otherwise the close time MUST be the last event in that session. The gap until the new launch MUST NOT be counted. The end reason MUST be Terminated.

#### Scenario: Killed while backgrounded
- **WHEN** a visit has been open for 2 minutes, the app is backgrounded, the process ends, and a later launch opens another page
- **THEN** the earlier visit closes at the background time with elapsed and active time of 2 minutes, and the session end reason is Terminated

#### Scenario: Killed while in the foreground
- **WHEN** an open session's last signal is not backgrounded and the process ends
- **THEN** the system closes that session at its last recorded event and starts a new session

### Requirement: The log is append-only
Each event SHALL be inserted as its own row. An insert MUST NOT rewrite earlier rows. A read SHALL return rows oldest-first by timestamp, then by insert sequence. A new row MUST have an empty upload timestamp. The system SHALL NOT send events off the device.

#### Scenario: Inserts arrive out of timestamp order
- **WHEN** events are inserted with timestamps that are not in insert order
- **THEN** a read returns them oldest timestamp first and every upload timestamp is empty

#### Scenario: The database is reopened
- **WHEN** the database file is closed and opened again
- **THEN** the previously inserted events are still present and still have empty upload timestamps

### Requirement: History reset replaces the log
Reset SHALL delete every stored event, record a History reset event, and start a new session on the current page. Earlier page names MUST NOT remain.

#### Scenario: Reset after visiting other pages
- **WHEN** history is reset while a page is open
- **THEN** the oldest event is History reset, Installed is absent, and the only page visit is the current page

### Requirement: A recording failure does not stop the app
A failure while recording a window or navigation signal MUST NOT take down the app.

#### Scenario: The recorder throws
- **WHEN** recording a lifecycle or navigation signal throws
- **THEN** the failure is written to the debug log and the window continues
