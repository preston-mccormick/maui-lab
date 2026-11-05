# Spec Delta

## Purpose

Gives the large lists one shared playground: a Collections group of four pages where the user changes how a collection arranges many items, how each item is built, and, on the linear page, what a swipe reveals.

## ADDED Requirements

### Requirement: Collections is its own flyout group
The app SHALL include a flyout group titled Collections, placed after Layouts. It SHALL contain four bottom tabs, in this order: Grid, Linear, Flex, and Uniform.

#### Scenario: Open from the flyout
- **WHEN** the user opens the flyout
- **THEN** Collections follows Layouts, and its bottom tabs are Grid, Linear, Flex, and Uniform

### Requirement: Colors and the layout pages stay
Colors SHALL remain a top-level entry. Flex Layout and Composite Layout SHALL remain Layouts tabs.

#### Scenario: Existing entries remain
- **WHEN** the user opens the flyout
- **THEN** Colors is a top-level entry and Flex Layout is a Layouts tab

### Requirement: Collection tabs sit on the bottom
The four pages SHALL be bottom tabs in the group order, so each page is created when that tab is shown. The app MUST NOT add its own overflow menu. Tab labels SHALL be Grid, Linear, Flex, and Uniform. Page titles SHALL be CollectionView Grid Layout, CollectionView Linear Layout, FlexView, and UniformItemsView.

#### Scenario: Bottom tabs in order
- **WHEN** the user opens Collections
- **THEN** the bottom tabs are Grid, Linear, Flex, and Uniform, and the first page's title is CollectionView Grid Layout

### Requirement: The canvas fills and the panel docks
Each collection page SHALL show its collection on a canvas that fills the space the panel does not take. Below 800 points of width the panel SHALL dock to the bottom. At 800 and above it SHALL dock to the trailing edge. Crossing that width SHALL move the panel and give the freed space back to the canvas. While the panel is on the bottom, dragging its top edge SHALL resize the split.

#### Scenario: Narrow window
- **WHEN** a collection page is 799 wide
- **THEN** the panel sits along the bottom and the canvas fills the width above it

#### Scenario: Wide window
- **WHEN** the page widens from 799 to 800
- **THEN** the panel moves to the trailing edge and the canvas grows taller by the space the bottom panel released

### Requirement: The panel has three sections
The docked panel SHALL be flat, with no shadow of its own. Its header SHALL read Playground. Data, the control, and Item SHALL each be a floating card with a shadow. The control card SHALL be titled CollectionView, FlexView, or UniformItemsView. The cards SHALL stack and scroll when they exceed the panel. Each card SHALL collapse to its title and expand again. The panel itself SHALL collapse to a strip and expand again. Data SHALL offer the catalog and the sort.

#### Scenario: Collapse Data
- **WHEN** the user taps the Data section's title
- **THEN** that section shows only its title, and the control card and Item are unchanged

#### Scenario: Collapse the panel
- **WHEN** the user collapses the panel
- **THEN** only the panel's title strip remains and the canvas fills the rest of the page

### Requirement: Reset and timings sit under the header
The panel header SHALL show Playground, with an info control on its right that describes that collection. Under the header, above the cards, a flat row SHALL put Reset on the left and the timings switch on the right. That row SHALL NOT be a collapsing card. The right edges of the header info control and the timings info control SHALL line up with the info controls inside the cards.

#### Scenario: About sits in the header
- **WHEN** the grid page is showing
- **THEN** the header reads Playground and the info control is on its right

#### Scenario: Reset is left of timings
- **WHEN** a collection page is showing
- **THEN** Reset is on the left of the row under the header, and the timings switch is on the right

#### Scenario: Header and timings line up with the cards
- **WHEN** a collection page is showing
- **THEN** the right edge of the header info control and the right edge of the timings info control line up with the info controls in the cards

### Requirement: Pages open on the named colors
Each page SHALL open on the named-color catalog, sorted by name ascending. The card SHALL be the three-label card at 180 by 120 points, with font size 14 and no margin. Timings SHALL be off, and no timing notice SHALL be showing. Each page SHALL keep its own catalog, sort, and items.

#### Scenario: Open any collection page
- **WHEN** a collection page opens
- **THEN** it shows the named colors in ascending name order, as 180 by 120 cards, with no timing notice

#### Scenario: One page does not change another
- **WHEN** the user chooses another catalog on Flex and opens Grid
- **THEN** the grid page is still showing the named colors

### Requirement: Three catalogs
The Grid and Linear pages SHALL offer the named colors, the XKCD survey, and the 12-letter names. Flex and Uniform SHALL offer only the named colors and the XKCD survey. The catalog list SHALL be cached. Named colors SHALL be ready when first asked. A catalog file SHALL be read the first time its colors are used. Choosing one SHALL replace the items in the current sort. A pin, a copy, or a deletion SHALL NOT survive that replacement.

#### Scenario: The long files wait
- **WHEN** the grid page opens on the named colors
- **THEN** the catalog control names the XKCD survey and the 12-letter names, and those files have not yet been read

#### Scenario: The other pages stop at the survey
- **WHEN** Flex opens
- **THEN** its catalog offers the named colors and the XKCD survey, and not the 12-letter names

#### Scenario: Swap to the survey
- **WHEN** the user chooses the XKCD color survey
- **THEN** the page shows 949 items from that survey

#### Scenario: A pin does not survive the swap
- **WHEN** the user pins a swatch and then chooses another catalog
- **THEN** the new list has no pinned swatch

### Requirement: Hidden tabs stay empty
Each collection page is its own bottom tab. Opening the group SHALL create only the selected page. Linear, Flex, and Uniform SHALL NOT build their items until that page is shown. The grid page SHALL build its items when it is shown.

#### Scenario: Open the group on Grid
- **WHEN** the user opens the Collections group
- **THEN** the grid page shows the named colors, and Linear, Flex, and Uniform have not built items

#### Scenario: Show Flex
- **WHEN** the user selects the Flex tab
- **THEN** Flex builds its items, and a later choice of the XKCD survey is added in batches

### Requirement: Sort covers the color measures
The sort control SHALL offer name, luminosity, hue, and saturation, each ascending and descending. It SHALL open on name ascending. Choosing a sort SHALL reorder the displayed items. A pin, a copy, or a deletion SHALL NOT survive the reorder.

#### Scenario: Sort by hue
- **WHEN** the user sorts by hue ascending
- **THEN** each item's hue is greater than or equal to the hue of the item before it

### Requirement: Timings stay quiet until asked
The timings switch SHALL start off, SHALL be shared by every Collections page, and SHALL have an info control titled Timings that says so. While it is on, a catalog swap, a sort, an arrangement change, a font change, and a reset SHALL show a notice over the top of the canvas naming the operation and its duration. The notice MUST NOT change the collection's size. Tapping it SHALL dismiss it. Turning timings off SHALL dismiss it, and later operations MUST NOT show a notice.

#### Scenario: Open is quiet
- **WHEN** a collection page opens
- **THEN** no timing notice is showing

#### Scenario: The switch is shared
- **WHEN** the user turns timings on Grid and opens Flex
- **THEN** Flex timings are on

#### Scenario: A sort reports its time
- **WHEN** timings are on and the user sorts the list
- **THEN** a notice over the top of the canvas names the sort and shows a duration

#### Scenario: A reset reports its time
- **WHEN** timings are on and the user resets
- **THEN** a notice over the top of the canvas names the reset and shows a duration

#### Scenario: Turn timings off
- **WHEN** a notice is showing and the user turns timings off
- **THEN** the notice is gone

### Requirement: The card has three arrangements
The Item section SHALL offer Row, Card, and Varied. Row SHALL be one line, 44 points tall. Card SHALL show the name three times at the item height. Varied SHALL assign a stable width and height that does not follow the name's length, and no varied card SHALL be smaller than 160 by 96. Width, height, font, and margin SHALL be settings. Font SHALL run from 8 to 40. An info control titled Arrangement SHALL describe the three arrangements.

#### Scenario: Switch to a row
- **WHEN** the user sets the arrangement to Row
- **THEN** each item is one line, 44 points tall

#### Scenario: Varied sizes are not the name length
- **WHEN** the arrangement is Varied
- **THEN** each item is 160, 200, 240, or 280 wide and 96, 132, 168, or 204 tall, the same name keeps that size, and the items are not all one size

#### Scenario: Larger type
- **WHEN** the user finishes setting the font to 24
- **THEN** the item labels are 24 points

### Requirement: An ignored setting is disabled
A setting the current mode ignores SHALL be disabled. Item height SHALL be disabled unless the arrangement is Card. Item width SHALL be disabled for Varied.

#### Scenario: Row disables height
- **WHEN** the arrangement is Row
- **THEN** the height setting is disabled and the width setting stays enabled

#### Scenario: Varied disables width and height
- **WHEN** the arrangement is Varied
- **THEN** the width and height settings are disabled

### Requirement: Reset restores the opening page
Reset SHALL restore that page's opening settings, including the canvas color, and SHALL rebuild the items from the opening catalog and sort. Pins, copies, and deletions SHALL NOT survive. Automatic fitting SHALL resume. The timings switch SHALL stay as the user left it.

#### Scenario: Reset clears a painted canvas
- **WHEN** the user has painted the canvas and changed the arrangement, and then resets
- **THEN** the canvas is the theme background, the arrangement is Card, and the items match the opening catalog

#### Scenario: Reset resumes the automatic span
- **WHEN** the user has set a span and then resets while the canvas is 880 points wide
- **THEN** the span follows the fit

#### Scenario: Timings stay on across a reset
- **WHEN** timings are on and the user resets
- **THEN** timings stay on

### Requirement: Tapping a swatch paints the canvas
Tapping an item SHALL set the canvas background to that item's color.

#### Scenario: Tap a swatch
- **WHEN** the user taps a swatch
- **THEN** the canvas background becomes that color

### Requirement: Collection levers match the layout panels
Each setting SHALL sit on its own row, with a horizontal line between settings. A numeric setting SHALL show a right-aligned fixed-width entry and a slider on the next row that spans that row. The label SHALL be centered on the entry. A long label MUST NOT narrow the value of another setting. An info control SHALL sit beside the setting it explains.

#### Scenario: Width slider spans the row
- **WHEN** the user looks at item width
- **THEN** the slider spans the full width of the next row and the label is centered on the entry

#### Scenario: A long label stays on its row
- **WHEN** the user looks at horizontal spacing on a row above span
- **THEN** the span value is not narrowed by the longer label

### Requirement: Picker text and switches sit on the right
A picker SHALL show its selected text aligned to the right. A switch SHALL sit on the right of its row, with its label staying on the left.

#### Scenario: A switch is trailing
- **WHEN** the user looks at a switch in a collection section
- **THEN** the label is on the left and the switch is on the right

### Requirement: Info sits beside the setting
An info control SHALL be a small round blue button with an "i" glyph, beside the setting it explains. Tapping it SHALL show that control's title and text in a banner over the top of the page. The banner MUST NOT move the canvas or the panel. Each collection page SHALL have at least two info controls.

#### Scenario: Tap an info control
- **WHEN** the user taps an info control on a collection page
- **THEN** a banner at the top of the page shows that control's title and text, and the canvas and panel keep their place

### Requirement: Collection View opens like the color swatches
Collection View SHALL open as a vertical grid, measuring the first item for every row, ungrouped, and keeping the scroll offset when items are replaced. Until the user sets the span, a gap, or the item width, those values SHALL follow the swatch fit. A canvas size change SHALL fit them again and SHALL NOT be recorded. A value the user has set SHALL stay.

#### Scenario: A wide canvas holds four columns
- **WHEN** the canvas is 880 points wide and the user has not set the span or the gaps
- **THEN** the grid has four columns of 180-point cards with 36 points between them

#### Scenario: A narrow canvas keeps two columns
- **WHEN** the canvas is 390 points wide and the user has not set the gaps
- **THEN** the grid has two columns and the gap is 15

#### Scenario: A chosen span stays
- **WHEN** the user sets the span and the canvas then changes width
- **THEN** the span stays at the user's value

#### Scenario: Horizontal orientation uses the height
- **WHEN** the grid is horizontal and the canvas is 390 points tall
- **THEN** the span follows that height the way a vertical grid follows the width

### Requirement: The swatch fit follows the canvas
The fit SHALL use at least two columns, a preferred card of 180, a preferred gap of 36, and a gap no smaller than 8. The card SHALL narrow only when the preferred card does not fit. Collection View SHALL put the gap in the item spacing. A horizontal grid SHALL fit its span to the canvas height. Flex Items SHALL put half the gap in the margin and the side inset in the padding.

#### Scenario: Preferred gap on a wide canvas
- **WHEN** the canvas is 880 points wide
- **THEN** the fit uses a 180-point card and a gap of 36

### Requirement: Collection View switches shape
The grid page SHALL stay a grid. The user SHALL set vertical or horizontal orientation, the span, and the horizontal and vertical gaps. Span SHALL change how many items sit on the cross axis.

#### Scenario: Three columns
- **WHEN** the span is 3 and the grid is vertical
- **THEN** each row of the grid holds three items

### Requirement: Collection View measures one item or every item
The user SHALL choose to measure the first item or every item. Measuring the first item SHALL give every row that item's height. Measuring every item SHALL keep each item's own height.

#### Scenario: The first height wins
- **WHEN** the list measures the first item and the arrangement is Varied
- **THEN** every row uses the first item's height

#### Scenario: Every item keeps its height
- **WHEN** the list measures every item and the arrangement is Varied
- **THEN** a tall item and a short item each keep their own height

### Requirement: Collection View groups by the first letter
Grouping SHALL start off. Turning it on SHALL group items by the first letter of the name. Each group SHALL show that letter as its header. Within a group, items SHALL keep the current sort. The groups SHALL follow the order those letters first appear in the sorted list.

#### Scenario: Group by letter
- **WHEN** the user turns grouping on
- **THEN** items that share a first letter sit under a header of that letter

### Requirement: Collection View keeps a chosen scroll anchor
When the displayed items are replaced, the list SHALL keep the scroll offset, the first visible item, or the last item, following the user's choice. An info control titled Scroll SHALL describe those three anchors.

#### Scenario: Keep the offset
- **WHEN** the scroll setting is keep offset and the user sorts
- **THEN** the list keeps its scroll offset

### Requirement: Flex Items flows and creates every item
Flex Items SHALL open as a row with wrap on. The user SHALL set direction, wrap, justification, item alignment, and content alignment. Every catalog entry SHALL have an item. Padding SHALL inset the items from the layout's edges. Until the user sets the item width, the margin, or the padding, those values SHALL follow the swatch fit. A canvas width change SHALL fit them again and SHALL NOT be recorded. A value the user has set SHALL stay.

#### Scenario: Open wrapped
- **WHEN** Flex Items opens
- **THEN** the direction is a row and wrap is on

#### Scenario: A wide canvas matches the swatch gap
- **WHEN** the canvas is 880 points wide and the user has not set the margin or the padding
- **THEN** the cards are 180 wide, the margin is 18, and the padding is 7.5

#### Scenario: A chosen margin stays
- **WHEN** the user sets the margin and the canvas then changes width
- **THEN** the margin stays at the user's value

#### Scenario: Wrap onto the next line
- **WHEN** the direction is a row, wrap is on, and the items are wider than the canvas
- **THEN** items that do not fit continue on the next line

#### Scenario: The long catalog is complete
- **WHEN** the user chooses the XKCD survey and the update finishes
- **THEN** the layout contains 949 items

#### Scenario: Inset the layout
- **WHEN** the user sets padding to 16
- **THEN** the items sit 16 points inside the layout's edges

### Requirement: Long lists can load in batches or at once
Flex Items and Uniform Items SHALL add a long list 40 items at a time unless the user turns that off. Off SHALL replace the list in one step. An arrangement change SHALL be timed through the update of the items, and the notice SHALL put a colon between the setting and the value.

#### Scenario: Batches start on
- **WHEN** Flex Items opens
- **THEN** load in batches is on

#### Scenario: Arrangement names its value
- **WHEN** timings are on and the user sets the arrangement to Varied
- **THEN** the notice reads "Arrangement: Varied" and shows a duration

### Requirement: Uniform Items stamps the first size
Uniform Items SHALL give every cell the first item's size. The column count SHALL be how many of that width fit in the canvas, and SHALL NOT exceed the maximum columns. The row count SHALL NOT exceed the maximum rows, and items past that SHALL NOT be arranged. Both maximums SHALL open unlimited. A maximum below 1 MUST NOT be applied. The page SHALL have no orientation setting.

#### Scenario: A short first item sets the cell
- **WHEN** the arrangement is Varied and the first item is shorter than a later item
- **THEN** the later item's cell is the first item's height, and that card paints outside the cell and overlaps its neighbors

#### Scenario: Two columns at most
- **WHEN** the user sets the maximum columns to 2
- **THEN** each row holds at most two items

#### Scenario: Two rows cut the list off
- **WHEN** the user sets the maximum rows to 2 and the items need more than two rows
- **THEN** only two rows are arranged

#### Scenario: Zero is refused
- **WHEN** a maximum of 0 is requested
- **THEN** the previous maximum stays and the page does not fail

### Requirement: Uniform Items matches the responsive swatches
Until the user sets item width or margin, Uniform Items SHALL follow the swatch fit. Cards SHALL use the responsive width, half the fitted gap as their margin, and the fitted side inset as the scroll view padding. A later canvas width change SHALL fit again without recording an action. A width or margin the user has set SHALL stay.

#### Scenario: Wide canvas has the swatch inset
- **WHEN** the canvas is 880 points wide
- **THEN** cards are 180 points wide with an 18-point margin and a 7.5-point horizontal scroll inset

#### Scenario: A chosen margin stays
- **WHEN** the user sets the margin and the canvas then changes width
- **THEN** the margin stays at the user's value

### Requirement: Linear opens as one vertical line
The Linear page SHALL open as a vertical CollectionView, measuring the first item, with no gap between items, and keeping the scroll offset when items are replaced. Pin and Delete SHALL be offered. Copy SHALL be off. It SHALL offer the 12-letter catalog.

#### Scenario: Open Linear
- **WHEN** the Linear page opens
- **THEN** the list is vertical, the gap is 0, the first item's size is reused, and the offered actions are Pin and Delete

### Requirement: Linear can run either way
The user SHALL switch the list between vertical and horizontal. Pin and Copy SHALL sit on the leading side: the left of a vertical list, revealed by a downward swipe when the list is horizontal. Delete SHALL sit on the trailing side: the right of a vertical list, revealed by an upward swipe when the list is horizontal. The swipe MUST NOT run along the scroll. The user SHALL set the gap, whether the first item's size is reused, and which scroll anchor is kept when the items are replaced.

#### Scenario: Vertical swipes sideways
- **WHEN** the list is vertical and Pin and Delete are offered
- **THEN** a swipe toward the right reveals Pin, and a swipe toward the left reveals Delete

#### Scenario: Horizontal swipes across the scroll
- **WHEN** the list is horizontal and Pin and Delete are offered
- **THEN** a downward swipe reveals Pin, and an upward swipe reveals Delete

#### Scenario: Every item keeps its height
- **WHEN** the list measures every item and the arrangement is Varied
- **THEN** a tall item and a short item each keep their own height

### Requirement: Context actions follow the platform gesture
Each offered action SHALL be a swipe item on the card, across the scroll. With none offered, a swipe SHALL show nothing. iOS, Android, and Mac Catalyst SHALL reveal the actions by that swipe, including a mouse drag on Mac Catalyst. A secondary click on Mac Catalyst SHALL offer the same actions. Delete SHALL be marked destructive.

#### Scenario: Swipe on iOS
- **WHEN** Pin and Delete are offered and the user swipes a row on iOS
- **THEN** the user can choose Pin and Delete, and Delete is destructive

#### Scenario: Swipe on Android
- **WHEN** Pin and Delete are offered and the user swipes a row on Android
- **THEN** the user can choose Pin and Delete

#### Scenario: Drag with the mouse on Mac Catalyst
- **WHEN** Pin and Delete are offered and the user drags a row with the mouse on Mac Catalyst
- **THEN** the user can choose Pin and Delete

#### Scenario: Secondary click on Mac Catalyst
- **WHEN** Pin and Delete are offered and the user secondary-clicks a row on Mac Catalyst
- **THEN** the user can choose Pin and Delete

#### Scenario: Nothing offered
- **WHEN** every action is turned off and the user swipes a row on iOS
- **THEN** no action is revealed

### Requirement: Pin, copy, and delete change the displayed list
Pin SHALL toggle a mark on that item, and the action SHALL read Unpin while the mark is showing. Copy SHALL insert the same color and name immediately after that item. Delete SHALL remove that item from the displayed list and SHALL leave the catalog unchanged.

#### Scenario: Pin then unpin
- **WHEN** the user pins a swatch and then chooses Unpin on it
- **THEN** the mark appears and then disappears, and the action reads Pin again

#### Scenario: Copy inserts after
- **WHEN** the user copies a swatch
- **THEN** the next item has the same color and name

#### Scenario: Delete leaves the catalog
- **WHEN** the user deletes a swatch and then chooses that catalog again
- **THEN** the deleted name is back in the list
