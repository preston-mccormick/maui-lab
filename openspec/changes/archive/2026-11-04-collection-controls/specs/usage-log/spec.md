# Spec Delta

## ADDED Requirements

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
