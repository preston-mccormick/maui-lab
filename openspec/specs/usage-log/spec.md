# usage-log Specification

## Purpose

Records how a person moves through the app: sessions, pages, and how much of that time the app was in the foreground, kept on the device for a later upload.

## Requirements

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

### Requirement: Screen changes belong to the open page
A window size record SHALL be attached to the page visit that is open when the size is measured. The recorded mode is the event title and the width and height are the detail.

#### Scenario: Resize while a page is open
- **WHEN** the window size changes during an open page visit
- **THEN** that size record references the open page visit

### Requirement: Install and update metadata
Android SHALL supply package first-install time and last-update time. When the log is empty, Installed SHALL use the first-install time if Android provides one. A last-update time later than the install SHALL be stored once as Updated. iOS and Mac Catalyst provide neither time. On every platform, a changed app version SHALL record Updated with that version.

#### Scenario: Android package times
- **WHEN** the log is empty and Android reports a first-install time and a later last-update time
- **THEN** Installed uses the first-install time and one Updated event uses the last-update time

#### Scenario: Version change without a system time
- **WHEN** the recorded version differs from the running version and the platform has no update time
- **THEN** the log gains one Updated event for the running version

### Requirement: Permission requests
A system permission request SHALL record the permission name and the status returned. The Geolocation page SHALL request location this way. A permission that is already granted MUST NOT add a row.

#### Scenario: Location request
- **WHEN** location access is requested and the system returns a status
- **THEN** the log gains a Location row whose detail is that status

#### Scenario: Already granted
- **WHEN** location access is already granted
- **THEN** no new permission row is recorded

### Requirement: A recording failure does not stop the app
A failure while recording a window or navigation signal MUST NOT take down the app.

#### Scenario: The recorder throws
- **WHEN** recording a lifecycle or navigation signal throws
- **THEN** the failure is written to the debug log and the window continues

### Requirement: A layout action is one event
A completed gesture on a Layouts page SHALL append one action to the open page visit. The action's name SHALL be the verb and the noun, such as "Added row". It SHALL carry a short detail, which MAY be empty. It MUST NOT append a button event, a surface event, or a choice event. An action recorded while no page visit is open MUST be ignored.

#### Scenario: Add a row
- **WHEN** the user adds a row on Grid Layout and the grid then has 4 rows
- **THEN** the open visit gains one action named "Added row" with detail "4 rows", and no button, surface, or choice event

#### Scenario: No open visit
- **WHEN** a layout action is recorded with no page visit open
- **THEN** the log is unchanged

### Requirement: Values record when the gesture ends
A drag SHALL record one action when it ends and none while it moves. An edited number or text SHALL record one action when editing finishes, and only when the value changed. A picker or switch SHALL record one action when its choice changes.

#### Scenario: Drag ends
- **WHEN** the user drags an item on Absolute Layout and releases it at 120, 80
- **THEN** the visit gains one action named "Moved item" with detail "120, 80", and none from the drag itself

#### Scenario: Spacing left unchanged
- **WHEN** the user focuses a spacing field and leaves it with the same value
- **THEN** no action is recorded

#### Scenario: Spacing committed
- **WHEN** the user changes row spacing from 48 to 12 and finishes editing
- **THEN** the visit gains one action named "Changed row spacing" with detail "48 to 12"

### Requirement: Incidental interaction is not recorded
Selecting an item, collapsing or expanding a section, collapsing or expanding the panel, dismissing the info banner, and the panel moving because the window crossed 800 points MUST NOT record an action. Tapping an info control SHALL record one action whose name is "Viewed" plus the control's title. The info text MUST NOT be stored.

#### Scenario: Select and collapse
- **WHEN** the user selects an item and collapses the Layout section
- **THEN** no action is recorded

#### Scenario: View a note
- **WHEN** the user taps a note titled "Cross axis"
- **THEN** the visit gains one action named "Viewed Cross axis" with an empty detail

### Requirement: Grid gestures
Grid Layout SHALL record "Added column", "Removed column", "Added row", and "Removed row" with the resulting count, "Changed column spacing" and "Changed row spacing" with the old and new value, "Changed column" and "Changed row" with the track's index and its unit and value, "Added child" and "Removed child", and "Placed child" with the cell and spans. A refused removal of the last track MUST NOT record an action.

#### Scenario: Last column stays quiet
- **WHEN** one column remains and the user tries to remove it
- **THEN** no action is recorded

#### Scenario: Place a child
- **WHEN** the user finishes placing a child at row 1, column 2, spanning 1 row and 2 columns
- **THEN** the visit gains one action named "Placed child" with detail "row 1, column 2, span 1 by 2"

### Requirement: Stack gestures
Stack Layout SHALL record "Added item", "Added set", "Added stack" with the new stack's orientation, "Removed item", "Changed spacing" with the old and new value, and "Changed orientation" with Vertical or Horizontal. A nested stack's orientation and spacing record under the same names.

#### Scenario: Turn the stack
- **WHEN** the user switches the stack to Horizontal
- **THEN** the visit gains one action named "Changed orientation" with detail "Horizontal"

#### Scenario: Nest a stack
- **WHEN** the page's stack is vertical and the user adds a stack
- **THEN** the visit gains one action named "Added stack" with detail "Horizontal"

### Requirement: Absolute gestures
Absolute Layout SHALL record "Added item" and "Removed item" with the illustration's file name, "Moved item" with the position, "Resized item" with the size, "Changed rotation", "Changed opacity", "Changed clip", "Changed z order", "Changed flags", and "Changed background".

#### Scenario: Add a beet
- **WHEN** the user adds a beet
- **THEN** the visit gains one action named "Added item" with detail "beet"

#### Scenario: Clip an item
- **WHEN** the user turns on the elliptical clip
- **THEN** the visit gains one action named "Changed clip" with detail "ellipse"

### Requirement: Flex gestures
Flex Layout SHALL record "Added item", "Added set", "Added mixed set", "Removed item", "Changed direction", "Changed wrap", "Changed justification", "Changed alignment", "Changed item alignment", "Changed grow", "Changed shrink", "Changed basis", "Changed order", "Changed margin", and "Changed padding" with the old and new value. An item change made with nothing selected SHALL append "(all)" to its detail.

#### Scenario: Grow all items
- **WHEN** nothing is selected and the user sets grow to 1
- **THEN** the visit gains one action named "Changed grow" with detail "1 (all)"

#### Scenario: Add a mixed set
- **WHEN** the user adds a mixed set
- **THEN** the visit gains one action named "Added mixed set" with the number of items as its detail

#### Scenario: Turn wrap on
- **WHEN** the user turns wrap on
- **THEN** the visit gains one action named "Changed wrap" with detail "Wrap"

### Requirement: Composite gestures
Composite Layout SHALL record "Switched layout" with Flex, Uniform, or Columns, "Showed description", "Hid description", "Showed validation", "Hid validation", and "Changed width" with the old and new value when the width edit finishes and the value changed. A width edit that does not change the value MUST NOT record an action.

#### Scenario: Switch to uniform
- **WHEN** the user switches the field to Uniform
- **THEN** the visit gains one action named "Switched layout" with detail "Uniform"

#### Scenario: Narrow the field
- **WHEN** the user finishes changing the field width from 320 to 200
- **THEN** the visit gains one action named "Changed width" with detail "320 to 200"

### Requirement: Dock and radial gestures
Dock Layout SHALL record "Added item", "Removed item", "Changed edge" with Left, Top, Right, Bottom, or None, "Changed expand last" with On or Off, and "Changed horizontal spacing" and "Changed vertical spacing" with the old and new value. Radial Layout SHALL record "Added item", "Removed item", "Changed start", "Changed sweep", "Changed radius", and "Changed angle".

#### Scenario: Pin to the top
- **WHEN** the user docks an item to the top
- **THEN** the visit gains one action named "Changed edge" with detail "Top"

#### Scenario: Stop filling the last item
- **WHEN** the user turns expand-last off
- **THEN** the visit gains one action named "Changed expand last" with detail "Off"

#### Scenario: Widen the arc
- **WHEN** the user changes the sweep from 180 to 270
- **THEN** the visit gains one action named "Changed sweep" with detail "180 to 270"

### Requirement: A layout action survives a recording failure
A failure while recording a layout action MUST NOT take down the page or drop the gesture the user just made.

#### Scenario: The log rejects the row
- **WHEN** appending the action fails
- **THEN** the page still shows the result of the gesture

### Requirement: A collection action is one event
A completed gesture on a Collections page SHALL append one action to the open page visit. The action's name SHALL be the verb and the noun. It SHALL carry a short detail, which MAY be empty. It MUST NOT append a button event, a surface event, or a choice event. An action recorded while no page visit is open MUST be ignored.

#### Scenario: Sort the list
- **WHEN** the user sorts the grid page by hue ascending
- **THEN** the open visit gains one action named "Sorted" with detail "Hue (ascending)", and no button, surface, or choice event

#### Scenario: No open visit
- **WHEN** a collection action is recorded with no page visit open
- **THEN** the log is unchanged

### Requirement: Painting the canvas and dismissing a timing are quiet
Tapping a swatch, and dismissing the timing notice, MUST NOT record an action.

#### Scenario: Tap a swatch
- **WHEN** the user taps a swatch on a collection page
- **THEN** no action is recorded

#### Scenario: Dismiss the timing
- **WHEN** the user taps the timing notice
- **THEN** no action is recorded

### Requirement: Shared collection gestures
A collection page SHALL record "Swapped list" with the catalog name, "Sorted" with the sort name, "Changed arrangement" with Row, Card, or Varied, "Changed font", "Changed item width", "Changed item height", and "Changed margin" with the old and new value, and "Changed timings" with On or Off. The catalog name SHALL be "HTML/CSS colors", "XKCD color survey", or "12-letter names". An edit that does not change the value MUST NOT record an action.

#### Scenario: Swap catalogs
- **WHEN** the user chooses the XKCD color survey
- **THEN** the visit gains one action named "Swapped list" with detail "XKCD color survey"

#### Scenario: Font left unchanged
- **WHEN** the user finishes a font edit without changing the value
- **THEN** no action is recorded

### Requirement: Reset is one action
A collection page SHALL record "Reset" with an empty detail. Reset SHALL record even when the page was already at its defaults.

#### Scenario: Reset the page
- **WHEN** the user resets a collection page
- **THEN** the visit gains one action named "Reset" with an empty detail

### Requirement: Collection View gestures
The grid page SHALL record "Changed orientation" with Vertical or Horizontal, "Changed span" with the number, "Changed horizontal spacing" and "Changed vertical spacing" with the old and new value, "Changed sizing" with "Measure first" or "Measure all", "Changed grouping" with On or Off, and "Changed scroll" with "Keep offset", "Keep item", or "Keep last".

#### Scenario: Measure every item
- **WHEN** the user switches sizing to measure all
- **THEN** the visit gains one action named "Changed sizing" with detail "Measure all"

#### Scenario: Widen the gap
- **WHEN** the user changes the horizontal spacing from 36 to 16
- **THEN** the visit gains one action named "Changed horizontal spacing" with detail "36 to 16"

### Requirement: Flex Items gestures
Flex Items SHALL record "Changed direction", "Changed wrap", "Changed justification", "Changed item alignment", "Changed content alignment", and "Changed padding" with the old and new value, and "Changed batch loading" with On or Off.

#### Scenario: Turn wrap off
- **WHEN** the user turns wrap off
- **THEN** the visit gains one action named "Changed wrap" with detail "NoWrap"

#### Scenario: Inset the items
- **WHEN** the user changes padding from 0 to 16
- **THEN** the visit gains one action named "Changed padding" with detail "0 to 16"

### Requirement: Uniform Items gestures
Uniform Items SHALL record "Changed max columns" and "Changed max rows", and "Changed batch loading" with On or Off. An unlimited value SHALL be recorded as "Fit". A numbered value SHALL be that number. A request below 1 MUST NOT record an action.

#### Scenario: Cap the columns
- **WHEN** the user sets the maximum columns from Fit to 2
- **THEN** the visit gains one action named "Changed max columns" with detail "Fit to 2"

#### Scenario: Zero stays quiet
- **WHEN** a maximum of 0 is requested
- **THEN** no action is recorded

### Requirement: Linear gestures
The Linear page SHALL record "Changed orientation" with Vertical or Horizontal, "Changed spacing" with the old and new value, "Changed sizing" with "Measure first" or "Measure all", "Changed scroll" with "Keep offset", "Keep item", or "Keep last", and "Changed pin action", "Changed copy action", and "Changed delete action" with On or Off. Pinning, unpinning, copying, and deleting SHALL record "Pinned swatch", "Unpinned swatch", "Copied swatch", and "Deleted swatch" with the color name.

#### Scenario: Delete a swatch
- **WHEN** the user deletes AliceBlue
- **THEN** the visit gains one action named "Deleted swatch" with detail "AliceBlue"

#### Scenario: Turn the list sideways
- **WHEN** the user switches orientation to Horizontal
- **THEN** the visit gains one action named "Changed orientation" with detail "Horizontal"

#### Scenario: Offer copy
- **WHEN** the user turns the copy action on
- **THEN** the visit gains one action named "Changed copy action" with detail "On"

### Requirement: A collection action survives a recording failure
A failure while recording a collection action MUST NOT take down the page or drop the gesture the user just made.

#### Scenario: The log rejects the row
- **WHEN** appending the action fails
- **THEN** the page still shows the result of the gesture
