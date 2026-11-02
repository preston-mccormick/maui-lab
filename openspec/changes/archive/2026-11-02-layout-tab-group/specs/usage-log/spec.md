# Spec Delta

## ADDED Requirements

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
