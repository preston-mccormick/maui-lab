# layout-playground Specification

## Purpose

Gives the layouts one shared playground: a Layouts group of seven pages where the user changes a layout's settings, adds and selects items, and reads a short explanation of what the layout is doing.

## Requirements

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

### Requirement: The panel scrolls and its sections collapse
The docked panel SHALL be flat, with no shadow of its own. Each section SHALL be a floating card with a shadow. The panel SHALL stack its sections vertically and scroll when they exceed the panel. Each section SHALL collapse to its title and expand again. The panel SHALL include a Layout section, and an Item section on every page whose canvas has selectable items. The Item section SHALL name the selected item, or say that none is selected. Composite Layout has no selectable items and SHALL have no Item section.

#### Scenario: Collapse a section
- **WHEN** the user taps the Layout section's title
- **THEN** that section shows only its title, and the Item section is unchanged

#### Scenario: Nothing selected
- **WHEN** no item is selected
- **THEN** the Item section says that none is selected

### Requirement: The panel itself collapses
The panel SHALL collapse to a strip and expand again. The panel title SHALL be set apart from the section titles. On the bottom the strip SHALL show the title; on the trailing edge the strip SHALL show only the expand glyph so it stays thin. While collapsed, the canvas SHALL take the space the panel released.

#### Scenario: Collapse the panel on a wide page
- **WHEN** the page is 800 or wider and the user collapses the panel
- **THEN** a thin strip with the expand glyph remains at the trailing edge and the canvas fills the rest of the page

#### Scenario: Collapse the panel on a narrow page
- **WHEN** the page is narrower than 800 and the user collapses the panel
- **THEN** only the title strip remains along the bottom

### Requirement: Tapping an item selects it
Tapping an item on the canvas SHALL select it and show its settings in the Item section. Tapping another item SHALL move the selection.

#### Scenario: Select then switch
- **WHEN** the user taps one item and then another
- **THEN** the Item section shows the second item's settings

### Requirement: Tapping empty canvas clears the selection
On Grid, Stack, Flex, Dock, and Radial, tapping the canvas outside every item SHALL clear the selection. Flex SHALL return to editing all items. A tap that lands on an item SHALL still select that item.

#### Scenario: Tap beside a stack item
- **WHEN** an item is selected and the user taps the stack canvas outside every item
- **THEN** no item is selected and the Item section says that none is selected

#### Scenario: Tap beside a flex item
- **WHEN** a flex item is selected and the user taps the canvas outside every item
- **THEN** the Item section is editing all items

### Requirement: Info opens on tap
An info control SHALL be a small round blue button with an "i" glyph, placed beside the setting it explains rather than grouped at the end of the panel. Tapping it SHALL show that control's title and text in a banner floating over the top of the page. The banner MUST NOT move the canvas or the panel. It SHALL stay until the user dismisses it or taps another info control, which replaces it. Each layout page SHALL have at least two info controls about that layout.

#### Scenario: Tap an info control
- **WHEN** the user taps an info control
- **THEN** a banner at the top of the page shows that control's title and text

#### Scenario: Dismiss the banner
- **WHEN** the banner is showing and the user taps its close button
- **THEN** the banner is gone and the canvas takes back its space

#### Scenario: Tap a second info control
- **WHEN** a banner is showing and the user taps a different info control
- **THEN** the banner shows the second control's title and text

### Requirement: The panel edges are not free space
Below 800 points of width the panel occupies the bottom edge, and at 800 and above it occupies the trailing edge. Anything the page presents outside the canvas MUST NOT be placed by assuming those edges are clear. The info banner uses the top edge.

#### Scenario: Banner over the page
- **WHEN** the user taps an info control
- **THEN** the banner floats over the top of the page and the canvas and panel keep their size and place

### Requirement: Grid opens on the seeded tracks
Grid Layout SHALL open with three columns and three rows. Each axis SHALL be star 1, star 4, star 1. Column spacing and row spacing SHALL each be 48. The first three columns SHALL be red, green, and blue. The first three rows SHALL be magenta, yellow, and cyan.

#### Scenario: Open Grid Layout
- **WHEN** Grid Layout opens
- **THEN** it shows that three-by-three arrangement with spacing 48 and those colors

### Requirement: Grid tracks can be added and edited
The user SHALL add a column or a row, and remove one while more than one remains on that axis. Each track SHALL choose Auto, Star, or Absolute, and SHALL edit a value unless the unit is Auto. A non-numeric spacing or value MUST NOT fail the page; the layout SHALL use zero for that number.

#### Scenario: Last track stays
- **WHEN** one column remains and the user removes it
- **THEN** that column remains

#### Scenario: Auto hides the value
- **WHEN** the user sets a track's unit to Auto
- **THEN** that track shows no value field

### Requirement: Grid children occupy cells
The user SHALL add a child to the grid and remove it. A child SHALL have a row, a column, a row span, a column span, and a horizontal and vertical alignment inside its cell. Changing any of these SHALL move or stretch the child accordingly. Track colors SHALL remain visible behind the children.

#### Scenario: Span two columns
- **WHEN** the user sets a child's column span to 2
- **THEN** the child stretches across those two columns

### Requirement: Stack switches axis
Stack Layout SHALL place its items in a vertical stack or a horizontal stack, and SHALL open on the vertical stack. A spacing value SHALL separate the items along the stack axis. The user SHALL add one item and SHALL add a set of items. The legacy `StackLayout` is not shown.

#### Scenario: Turn the stack sideways
- **WHEN** the user switches from the vertical stack to the horizontal stack
- **THEN** the same items lay out in a row with the same spacing

### Requirement: Stacks nest
The user SHALL add a nested stack. A new nested stack SHALL run across its parent's axis and SHALL start with two items. While a stack is selected, adding an item or a set SHALL put them inside that stack. A nested stack SHALL have its own orientation and spacing. Removing an item SHALL find it inside nested stacks.

#### Scenario: Rows inside a column
- **WHEN** the page's stack is vertical and the user adds a stack
- **THEN** a horizontal stack holding two items appears as one item of the vertical stack

#### Scenario: Fill the nested stack
- **WHEN** the user selects a nested stack and adds one item
- **THEN** the item appears inside that stack, not in the page's stack

### Requirement: Stack alignment follows the cross axis
An item's alignment SHALL apply on the cross axis only. The stack MUST NOT hand out leftover space along its axis.

#### Scenario: Expand in a vertical stack
- **WHEN** the stack is vertical and the selected item is set to expand vertically
- **THEN** the item keeps its own height and the extra space stays with the stack

### Requirement: Absolute items keep a rectangle
Absolute Layout SHALL place each item at an x, y, width, and height. The user SHALL add a chosen veggie illustration, and SHALL add a random illustration at a random rectangle anywhere inside the canvas. Each item SHALL have a z order, and a higher z SHALL paint above a lower one. Flags SHALL choose whether x, y, width, and height are absolute or proportional to the canvas.

#### Scenario: Proportional position
- **WHEN** an item's position flags are proportional and the canvas grows
- **THEN** the item's position stays at the same fraction of the canvas

#### Scenario: Random uses the whole canvas
- **WHEN** the user adds many random items
- **THEN** they land across the full width and height of the canvas, and more than one illustration appears

### Requirement: Absolute items move and resize
The user SHALL move an item by pressing and holding it and then dragging, and SHALL move it by entering x and y. A tap SHALL select without moving. Tapping the canvas outside every item and outside the resize handle SHALL clear the selection. The user SHALL resize the selected item by dragging a handle at its bottom-right corner, and SHALL resize it by entering width and height. A drag SHALL update the item while it moves and record once when it ends.

#### Scenario: Tap empty canvas
- **WHEN** an item is selected and the user taps the canvas outside every item
- **THEN** no item is selected and the Item section says that none is selected

#### Scenario: Drag to move
- **WHEN** the user presses and holds an item, drags it, and lets go
- **THEN** the item follows the finger and one move is recorded at the final position

#### Scenario: Drag to resize
- **WHEN** the user drags the selected item's corner handle outward
- **THEN** the item's width and height grow by that drag and its top-left corner stays put

### Requirement: Dragging an item does not open the flyout
A drag that starts on an item SHALL NOT pull the flyout open. A swipe that starts on empty canvas at the leading edge MAY still open it.

#### Scenario: Drag an item near the leading edge
- **WHEN** the user presses and holds an item that sits near the leading edge and drags it
- **THEN** the item follows the finger and the flyout stays closed

### Requirement: Render changes do not move the layout rectangle
An item SHALL have a rotation, an opacity from 0 to 1, and an optional elliptical clip. Rotation, opacity, and clip MUST NOT change the item's layout rectangle. The selection frame SHALL stay the axis-aligned rectangle the layout assigned.

#### Scenario: Rotate a square
- **WHEN** the user sets a square item's rotation to 45 degrees
- **THEN** the item draws rotated and its selection frame stays the unrotated rectangle

### Requirement: Absolute content and background
An item SHALL be one of the veggie illustrations, drawn with a transparent background so the canvas shows through. The canvas background SHALL be a solid color, a gradient, or a bundled image. Changing the background MUST NOT move the items.

#### Scenario: Add an illustration
- **WHEN** the user adds a beet
- **THEN** the canvas shows the beet illustration at that item's rectangle, with the canvas showing through around it

#### Scenario: Image background
- **WHEN** the user sets the background to a bundled image
- **THEN** the image fills the canvas behind the items and the items stay where they were

### Requirement: Flex wraps and distributes
Flex Layout SHALL expose direction, wrap, justification, item alignment, and content alignment. The user SHALL add one item, add a set of the same shape, and add a mixed set containing a shape, an image, and a panel. Changing wrap SHALL move items that no longer fit onto another line.

#### Scenario: Wrap a full row
- **WHEN** wrap is off and the items overflow the line, and the user turns wrap on
- **THEN** the overflowing items move to the next line

### Requirement: Flex items grow, shrink, and reorder
A flex item SHALL have grow, shrink, basis, order, margin, and its own alignment. A larger order SHALL place the item later along the direction. Basis SHALL accept a length or a percentage of the line. The layout has no spacing setting: the layout's padding insets all the items, and each item's margin is the gap between them.

#### Scenario: Order jumps ahead
- **WHEN** the user sets one item's order above the others
- **THEN** that item lays out after the items with a lower order

### Requirement: Flex item settings apply to all when nothing is selected
With no item selected, the Item section SHALL say it is editing all items, and changing a value there SHALL apply it to every item. Selecting an item SHALL edit that item alone. Tapping the selected item again, or tapping the canvas outside every item, SHALL return to editing all items.

#### Scenario: Grow everything
- **WHEN** nothing is selected and the user sets grow to 1
- **THEN** every item has grow 1 and one change is recorded for all items

### Requirement: Composite lays out one field three ways
Composite Layout SHALL show one field: a label, an entry, an optional description, and an optional validation message. The user SHALL switch that field between Flex, Uniform, and Columns. It SHALL open on Flex, at width 320, with the description and the validation hidden. The label and the entry SHALL stay visible. The field's width SHALL be the width its layout is given.

#### Scenario: Open on Flex
- **WHEN** Composite Layout opens
- **THEN** the field is 320 wide, shows the label and the entry laid out with Flex, and hides the description and the validation

#### Scenario: Switch to columns
- **WHEN** the user switches the field to Columns
- **THEN** the same label and entry are laid out with the form columns layout

### Requirement: Optional parts change the field
Showing the description SHALL add it to the field, and hiding it SHALL remove it. Showing the validation SHALL add a message taller than the label, and hiding it SHALL remove it. The entry's text MUST NOT change when a part is shown or hidden.

#### Scenario: Show validation
- **WHEN** the user shows the validation message
- **THEN** the message appears in the field and is taller than the label

### Requirement: Flex lets the parts keep their own size
While the field uses Flex and its width is 320, the label and the entry SHALL share one line. At width 200 the entry SHALL wrap onto the next line and keep its own height. The description and the validation message SHALL each take a following line at their own height.

#### Scenario: Wide enough to share a line
- **WHEN** the field uses Flex at width 320
- **THEN** the label and the entry sit on one line

#### Scenario: Narrow wrap
- **WHEN** the field uses Flex at width 200
- **THEN** the entry moves to the line below the label and keeps its own height

### Requirement: Uniform gives every part the label's cell
While the field uses Uniform, every visible part SHALL occupy a cell sized from the label. A taller validation message MUST NOT increase that cell's height.

#### Scenario: Validation stays in the label's cell
- **WHEN** the field uses Uniform and the validation message is shown
- **THEN** the message is placed in a cell no taller than the label

### Requirement: Columns grow the row and follow the field width
While the field uses Columns, each row SHALL be as tall as its tallest part. The column count SHALL follow the field's width: one column below 640, two from 640 up to but not including 960, and three at 960 and above.

#### Scenario: Narrow field stacks
- **WHEN** the field uses Columns and its width is 639
- **THEN** the parts sit in one column

#### Scenario: Two columns
- **WHEN** the field uses Columns and its width is 640
- **THEN** the parts sit in two columns

#### Scenario: Validation makes the row taller
- **WHEN** the field uses Columns at width 640 and the user shows the validation message
- **THEN** the row that holds the message grows to the message's height

### Requirement: Dock pins each item to an edge
Dock Layout SHALL be the toolkit dock layout. Each item SHALL choose left, top, right, bottom, or none. None SHALL occupy the same rectangle Top would: the item's own height across the remaining width, placed from the top. Horizontal and vertical spacing SHALL separate docked items. Changing an earlier item's edge SHALL reflow the items after it.

#### Scenario: None matches top
- **WHEN** the user sets an item's edge to none
- **THEN** that item takes its own height and the remaining width from the top, as Top would

#### Scenario: Redock the left item
- **WHEN** the user moves the left item to the top edge
- **THEN** that item spans the top and the items after it lay out in what is left

#### Scenario: Horizontal spacing
- **WHEN** the user sets horizontal spacing to 12
- **THEN** a gap of 12 separates items docked to the left or the right

### Requirement: The last docked item can fill
The dock page SHALL open with expand-last on. While it is on, the last item SHALL fill the space that remains, whatever edge it has. While it is off, the last item SHALL dock to its own edge and leave the remainder empty.

#### Scenario: Last item fills
- **WHEN** expand-last is on
- **THEN** the last item fills the space the earlier items leave

#### Scenario: Expand-last off
- **WHEN** the user turns expand-last off
- **THEN** the last item docks to its own edge and the remainder stays empty

### Requirement: The dock page opens covered
The dock page SHALL open with expand-last on and five rectangles that together cover the canvas: top, bottom, left, right, and a last item in the middle. The top and the bottom SHALL span the full width. The left and the right SHALL fill the height between them. The last item SHALL fill the middle.

#### Scenario: Five rectangles cover the canvas
- **WHEN** the dock page opens
- **THEN** a bar spans the top, a bar spans the bottom, bars fill the left and the right between them, and the last item fills the middle

### Requirement: Numeric levers have a slider
A numeric setting in a levers panel SHALL show an entry and a slider. The slider SHALL land on whole numbers. The entry SHALL accept a typed number. A request whose unset value is -1 SHALL use a slider stop for -1, outside the normal range. A request with no maximum SHALL use a slider stop for positive infinity. The angle override SHALL use a slider stop for no override. A drag SHALL update the layout while it moves and SHALL record one action when it ends.

#### Scenario: Width request unset
- **WHEN** the user moves the WidthRequest slider onto its unset stop
- **THEN** the request is -1

#### Scenario: Maximum request unlimited
- **WHEN** the user moves the MaximumWidthRequest slider onto its unlimited stop
- **THEN** the request is positive infinity

#### Scenario: Typed value still works
- **WHEN** the user types 40 into a spacing entry and leaves it
- **THEN** the spacing is 40 and the slider shows 40

#### Scenario: Slider lands on a whole number
- **WHEN** the user drags a spacing slider
- **THEN** the spacing is a whole number

### Requirement: A slider spans its row
Beside a label, the slider SHALL sit on the next row and span that row's full width, and the label SHALL be centered on the entry. A value with no label, such as a grid track, SHALL keep the entry and the slider on one row, with the slider on the left.

#### Scenario: Spacing slider spans the row
- **WHEN** the user looks at column spacing on the grid page
- **THEN** the slider spans the full width of its row and the label is centered on the entry

#### Scenario: Track value shares a row
- **WHEN** the user looks at a star or absolute grid track
- **THEN** the slider is on the left, the entry is on the right, and a gap separates that row from the picker

### Requirement: A line separates settings
A levers panel SHALL draw a horizontal line between settings. A group label, such as Columns or Rows, SHALL be bold.

#### Scenario: Line between spacing settings
- **WHEN** the user looks at the grid page
- **THEN** a line separates column spacing from row spacing

### Requirement: Lever entry text is right aligned
A numeric lever entry SHALL show its text aligned to the right. The entry SHALL keep a fixed width instead of stretching across the row.

#### Scenario: Width request reads from the right
- **WHEN** the user looks at a WidthRequest entry
- **THEN** the text is aligned to the right and the entry is only as wide as a short number

### Requirement: Each lever row sizes its own label
A row in the item levers SHALL size its label on its own. A long label SHALL NOT narrow the value on another row. A measured height or width SHALL stay on one line.

#### Scenario: Height stays on one line
- **WHEN** the item levers show Height on a row above MinimumWidthRequest
- **THEN** the height value stays on one line

### Requirement: Radial relays out when its settings change
Changing the start, sweep, radius, or an item's angle SHALL move the items at once, without adding or removing an item.

#### Scenario: Widen the sweep
- **WHEN** the user changes the sweep from 180 to 270
- **THEN** the items spread across the wider arc immediately

### Requirement: Radial places items on an arc
Radial Layout SHALL place its items on an arc defined by a start angle, a sweep, and a radius. The user SHALL add and remove items. Changing the sweep SHALL spread the items along the new arc. An item SHALL be able to override its angle, and that item SHALL leave the even spacing.

#### Scenario: Wider sweep
- **WHEN** the user increases the sweep
- **THEN** the items spread farther along the arc and the start angle stays

#### Scenario: One item leaves the arc spacing
- **WHEN** the user sets an angle on one item
- **THEN** that item sits at that angle and the others stay on the even spacing
