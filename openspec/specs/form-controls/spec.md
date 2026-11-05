# form-controls Specification

## Purpose

Shows the built-in form controls and the toolkit form pieces on a page in the Forms group, grouped by control type, bound to one of several sample records that the user can cycle through, with live validation.

## Requirements

### Requirement: Form Controls is a Forms page
The app SHALL include a flyout group titled Forms. Form Controls SHALL be a page in that group, with the page title Form Controls. Dialogs and Menu Button SHALL remain in that group.

#### Scenario: Open from the flyout
- **WHEN** the user opens Forms
- **THEN** Form Controls is one of its pages, and choosing it shows a page titled Form Controls

### Requirement: Five samples in a fixed order
The page SHALL hold five samples named Empty, Short, Long, Valid, and Invalid, in that order. The page SHALL open on Empty.

#### Scenario: Page opens
- **WHEN** the page opens
- **THEN** the form shows the Empty sample

### Requirement: The sample bar is always visible
A bar above the form SHALL stay visible while the form scrolls. It SHALL show the bound sample's position and name in the form "1 of 5 · Empty", with a previous control and a next control.

#### Scenario: Scrolled to the bottom
- **WHEN** the user scrolls the form to its last group
- **THEN** the sample bar is still visible with the current position and name

### Requirement: Cycling wraps
Next SHALL bind the following sample, and previous the preceding one. Next on Invalid MUST bind Empty. Previous on Empty MUST bind Invalid.

#### Scenario: Next wraps
- **WHEN** the bound sample is Invalid and the user taps next
- **THEN** the bound sample is Empty and the bar shows "1 of 5 · Empty"

#### Scenario: Previous wraps
- **WHEN** the bound sample is Empty and the user taps previous
- **THEN** the bound sample is Invalid and the bar shows "5 of 5 · Invalid"

### Requirement: Cycling rebinds every group
Every value the form shows or edits SHALL come from the bound sample. Cycling MUST update every group to the new sample's values without leaving the page.

#### Scenario: Empty to Long
- **WHEN** the user cycles from Empty to Long
- **THEN** the text fields, editors, search, pickers, dates, times, boxes, switches, radio choice, slider, stepper, and ratings all show the Long sample's values

### Requirement: Edits belong to the bound sample
An edit SHALL change only the bound sample. Cycling away and back MUST show that edit.

#### Scenario: Edit survives a round trip
- **WHEN** the user types "Ada" into the Short sample's name field, cycles to Long, then cycles back to Short
- **THEN** the name field shows "Ada" and the Long sample's name was not changed

### Requirement: Groups scroll in a fixed order
The page SHALL scroll vertically through one group per control type, in this order: Entry, Editor, SearchBar, Picker, DatePicker, TimePicker, CheckBox, Switch, RadioButton, Slider, Stepper, Button, RatingView. Each group heading SHALL name that control type.

#### Scenario: Reach every group
- **WHEN** the groups are taller than the page
- **THEN** scrolling reveals every group, in the order above, and each heading names its control type

### Requirement: Variant columns follow the variant area width
Variants inside a group SHALL use one column when that group's variant area is narrower than 640, two columns from 640 up to but not including 960, and three columns at 960 and wider.

#### Scenario: Narrow area
- **WHEN** a group's variant area is 639 wide
- **THEN** that group's variants sit in one column

#### Scenario: Two-column threshold
- **WHEN** a group's variant area is 640 wide
- **THEN** that group's variants sit in two columns

#### Scenario: Still two columns
- **WHEN** a group's variant area is 959 wide
- **THEN** that group's variants sit in two columns

#### Scenario: Three-column threshold
- **WHEN** a group's variant area is 960 wide
- **THEN** that group's variants sit in three columns

### Requirement: Each variant has a smaller caption
Each variant SHALL show descriptive text, smaller than the group heading, that names the variant on display.

#### Scenario: Caption stays with its control
- **WHEN** a group's variants are visible
- **THEN** each variant shows its smaller caption with that variant's control

### Requirement: Entry starts open and Editor starts closed
The Entry group SHALL start open and the Editor group SHALL start closed. Tapping either heading MUST toggle only that group's variants. Every other group MUST stay open. Cycling samples MUST NOT change which groups are open.

#### Scenario: Page opens
- **WHEN** the page opens
- **THEN** the Entry variants are visible and the Editor variants are hidden

#### Scenario: Toggle the headings
- **WHEN** the user taps the Editor heading and then the Entry heading
- **THEN** the Editor variants are visible and the Entry variants are hidden

#### Scenario: Cycling keeps the open groups
- **WHEN** the user opens the Editor group and then taps next
- **THEN** the Editor group is still open

### Requirement: Entry shows text variants
The Entry group SHALL show a text field with a placeholder, a password field that masks characters and can be cleared while editing, a read-only field that shows the sample's text and does not accept edits, and a disabled field that does not accept input.

#### Scenario: Password and read-only
- **WHEN** the Entry group is open
- **THEN** the password field masks its characters, the read-only field shows its text and ignores edits, and the disabled field ignores input

#### Scenario: Long sample wraps
- **WHEN** the Long sample is bound
- **THEN** the read-only field shows text longer than its width

### Requirement: Required text shows and clears
The required Entry SHALL show "Required" while its text is empty or only whitespace, and SHALL hide that message when the text contains a non-whitespace character.

#### Scenario: Empty sample
- **WHEN** the Empty sample is bound
- **THEN** the required field is empty and shows "Required"

#### Scenario: Whitespace stays required
- **WHEN** the Invalid sample is bound
- **THEN** the required field contains only spaces and shows "Required"

#### Scenario: A character clears it
- **WHEN** the user enters "a" in the required field
- **THEN** "Required" is hidden

### Requirement: Email validation shows and clears
The email Entry SHALL show "Enter a valid email address" while its text is not an email address, and SHALL hide that message for an email address.

#### Scenario: Invalid sample
- **WHEN** the Invalid sample is bound
- **THEN** the email field contains "not-an-email" and shows "Enter a valid email address"

#### Scenario: Valid sample
- **WHEN** the Valid sample is bound
- **THEN** the email field contains "ada@example.com" and hides the message

#### Scenario: A valid address clears it
- **WHEN** the user replaces the email text with "a@b.co"
- **THEN** "Enter a valid email address" is hidden

### Requirement: Numeric range shows and clears
The numeric Entry SHALL show "Enter a number from 1 to 10" while its value is outside 1 through 10, and SHALL hide that message for a value in that range.

#### Scenario: Invalid sample
- **WHEN** the Invalid sample is bound
- **THEN** the numeric field contains "0" and shows "Enter a number from 1 to 10"

#### Scenario: In range clears it
- **WHEN** the user replaces the numeric text with "5"
- **THEN** "Enter a number from 1 to 10" is hidden

#### Scenario: Above the range
- **WHEN** the numeric field contains "11"
- **THEN** it shows "Enter a number from 1 to 10"

### Requirement: Exact text shows and clears
The exact-text Entry SHALL show "Enter yes" unless the text is exactly "yes", and SHALL hide that message when the text is exactly "yes".

#### Scenario: Different casing stays invalid
- **WHEN** the Invalid sample is bound
- **THEN** the exact-text field contains "Yes" and shows "Enter yes"

#### Scenario: Exact value clears it
- **WHEN** the user replaces that text with "yes"
- **THEN** "Enter yes" is hidden

### Requirement: A required digit shows and clears
The digit Entry SHALL show "Include a digit" while the text has no digit, and SHALL hide that message when the text includes a digit.

#### Scenario: Invalid sample
- **WHEN** the Invalid sample is bound
- **THEN** the digit field contains "abcdef" and shows "Include a digit"

#### Scenario: A digit clears it
- **WHEN** the user replaces that text with "abcdef1"
- **THEN** "Include a digit" is hidden

### Requirement: URL validation shows and clears
The URL Entry SHALL show "Enter a URL" while its text is not an absolute URL, and SHALL hide that message for an absolute URL.

#### Scenario: Invalid sample
- **WHEN** the Invalid sample is bound
- **THEN** the URL field contains "not a url" and shows "Enter a URL"

#### Scenario: An absolute URL clears it
- **WHEN** the user replaces that text with "https://example.com"
- **THEN** "Enter a URL" is hidden

### Requirement: Combined rules show each failure
The combined Entry SHALL show "Required" while its text is empty and "Letters only" while its text contains a character that is not a letter. Both messages MUST be hidden when the text is one or more letters and nothing else.

#### Scenario: Empty sample
- **WHEN** the Empty sample is bound
- **THEN** the combined field is empty and shows "Required"

#### Scenario: Invalid sample
- **WHEN** the Invalid sample is bound
- **THEN** the combined field contains "abc123", shows "Letters only", and hides "Required"

#### Scenario: Letters pass both
- **WHEN** the combined field contains "abc"
- **THEN** "Required" and "Letters only" are both hidden

### Requirement: Phone entry keeps mask punctuation
The phone Entry SHALL show mask punctuation around the digits the user enters, in the form "(###) ###-####".

#### Scenario: Digits gain punctuation
- **WHEN** the user enters the digits 5551234567 in the phone field
- **THEN** the field displays "(555) 123-4567"

### Requirement: Valid sample hides every message
When the Valid sample is bound, no validation message SHALL be visible.

#### Scenario: Valid sample
- **WHEN** the Valid sample is bound
- **THEN** "Required", "Enter a valid email address", "Enter a number from 1 to 10", "Enter yes", "Include a digit", "Enter a URL", and "Letters only" are all hidden

### Requirement: Editor shows multiline variants
The Editor group SHALL show a multiline field with a placeholder, a field that grows as its text grows, and a disabled field that does not accept input.

#### Scenario: Growing field
- **WHEN** the Editor group is open and the user adds lines to the growing field
- **THEN** that field grows to show the added lines, and the disabled field ignores input

### Requirement: SearchBar shows a query and a disabled field
The SearchBar group SHALL show a field with the placeholder "Search" that accepts a query, and a disabled field that does not accept input.

#### Scenario: Query and disabled
- **WHEN** the Empty sample is bound
- **THEN** the query field shows "Search" until the user types, and the disabled field ignores input

### Requirement: Picker shows title, selection, and disabled
The Picker group SHALL show a picker whose title "Choose one" is visible while the sample has no choice, a picker that shows the sample's choice, and a disabled picker that does not open.

#### Scenario: Title until a choice
- **WHEN** the Empty sample is bound
- **THEN** the first picker shows "Choose one"

#### Scenario: Selection from the sample
- **WHEN** the Valid sample is bound
- **THEN** the first picker shows "Green"

#### Scenario: Disabled picker
- **WHEN** the user tries to open the disabled picker
- **THEN** it does not change its selection

### Requirement: DatePicker shows format, a maximum of today, and disabled
The DatePicker group SHALL show a date, a date that cannot be later than today, and a disabled date that does not open.

#### Scenario: No date after today
- **WHEN** the user chooses a date on the constrained date field
- **THEN** the chosen date is today or earlier

#### Scenario: Disabled date
- **WHEN** the user tries to open the disabled date field
- **THEN** its date does not change

### Requirement: TimePicker shows 12-hour, 24-hour, and disabled
The TimePicker group SHALL show a time with an AM/PM marker, a time using hours 00 through 23, and a disabled time that does not open.

#### Scenario: Both clocks
- **WHEN** the TimePicker group is visible
- **THEN** one time shows an AM/PM marker and the other shows an hour from 00 through 23

#### Scenario: Disabled time
- **WHEN** the user tries to open the disabled time field
- **THEN** its time does not change

### Requirement: CheckBox shows two boxes and a disabled box
The CheckBox group SHALL show two boxes the user can toggle and a disabled box that does not toggle. Their states come from the bound sample.

#### Scenario: Valid sample
- **WHEN** the Valid sample is bound
- **THEN** the first box is unchecked, the second is checked, and the disabled box is checked

#### Scenario: Disabled box
- **WHEN** the user tries to toggle the disabled box
- **THEN** its checked state stays unchanged

### Requirement: Switch shows two switches and a disabled switch
The Switch group SHALL show two switches the user can toggle and a disabled switch that does not toggle. Their states come from the bound sample.

#### Scenario: Valid sample
- **WHEN** the Valid sample is bound
- **THEN** the first switch is off, the second is on, and the disabled switch is on

#### Scenario: Disabled switch
- **WHEN** the user tries to toggle the disabled switch
- **THEN** its on state stays unchanged

### Requirement: RadioButton is one shared group
The RadioButton group SHALL show three choices in one variant. The third is disabled. The selected choice comes from the bound sample, and choosing an enabled choice MUST leave only that choice selected.

#### Scenario: Empty sample
- **WHEN** the Empty sample is bound
- **THEN** no choice is selected

#### Scenario: One selection
- **WHEN** the user selects the second choice
- **THEN** the second choice is selected, the first is not, and the third stays unselected and disabled

### Requirement: Slider reports its value
The Slider group SHALL show a slider from 0 to 100 whose caption includes the current value as it moves, and a disabled slider that does not move. Values come from the bound sample.

#### Scenario: Long sample
- **WHEN** the Long sample is bound
- **THEN** the enabled slider is at 100 and its caption shows 100

#### Scenario: Value follows the slider
- **WHEN** the user moves the enabled slider
- **THEN** the caption shows the new value, and the disabled slider stays where it started

### Requirement: Stepper reports its value
The Stepper group SHALL show a stepper from 0 to 10, stepping by 1, whose caption includes the current value, and a disabled stepper that does not change. Values come from the bound sample.

#### Scenario: Value follows the stepper
- **WHEN** the user steps the enabled stepper
- **THEN** the caption shows the new value, and the disabled stepper stays where it started

### Requirement: Button shows text, disabled, and image
The Button group SHALL show a text button that records a press on the bound sample, a disabled text button that does not activate, and an image button that shows an image already in the app and records a press.

#### Scenario: Press count
- **WHEN** the user presses the text button twice on the Empty sample
- **THEN** the caption shows 2 presses, and cycling to Short shows that sample's own count

#### Scenario: Disabled button
- **WHEN** the user tries to activate the disabled text button
- **THEN** the press count does not change

### Requirement: RatingView shows star, heart, and read-only
The RatingView group SHALL show a star rating the user can change, a heart rating the user can change, and a read-only rating that does not change when tapped. Each rating SHALL show five shapes and its value comes from the bound sample.

#### Scenario: Long sample
- **WHEN** the Long sample is bound
- **THEN** all three ratings show 5

#### Scenario: Tap changes the star rating
- **WHEN** the user taps a star
- **THEN** the star rating updates to that star

#### Scenario: Read-only ignores taps
- **WHEN** the user taps the read-only rating
- **THEN** its rating stays unchanged
