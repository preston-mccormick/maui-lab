# contacts Specification

## Purpose

Gives the lab an address book whose cards stay read-only until one card turns over into a form, filled from a generated local database at 10, 100, and 1,000 rows.

## Requirements

### Requirement: Contacts follows Colors
The app SHALL include a top-level flyout entry titled Contacts, placed immediately after Colors. The page title SHALL be Contacts.

#### Scenario: Open from the flyout
- **WHEN** the user opens the flyout
- **THEN** Contacts is the entry directly under Colors

### Requirement: The top bar offers filter, sort, and new contact
The Contacts page SHALL show Filter, Sort, and a New button above the canvas. Filter SHALL offer Friends, Family, and All, and each choice SHALL include how many contacts it shows. Sort SHALL offer Last name, First name, and City.

#### Scenario: Controls on the page
- **WHEN** Contacts is showing and a database exists
- **THEN** Filter, Sort, and New are visible above the cards

### Requirement: The page opens on friends by last name
Once a database exists, Contacts SHALL open with Friends selected and the cards ordered by last name ascending.

#### Scenario: Return to the page
- **WHEN** the user opens Contacts and the database has already been generated
- **THEN** Filter is Friends and the cards are in ascending last-name order

#### Scenario: Counts sit on the filter
- **WHEN** the generated book is showing
- **THEN** the filter choices read Friends (10), Family (100), and All (1,000)

### Requirement: An empty database asks before generating
When no contact database exists, the canvas SHALL explain that generating downloads 1,000 people and their photos, and SHALL offer a Generate Database button. The page MUST NOT start that download on its own.

#### Scenario: First visit
- **WHEN** the user opens Contacts and no contact database exists
- **THEN** the canvas shows the explanation and Generate Database, and no download has started

### Requirement: Generate builds the cached book
Generate Database SHALL download 1,000 US people from Random User Generator API 1.4 using seed `maui-lab`, download a dozen extra portraits, store that payload and the image files on the device, and fill the local database from that cache. Friends SHALL be the first 10 people. Family SHALL be the first 100, which includes those 10. All SHALL be the full 1,000. Every generated contact SHALL already pass the form's validation.

#### Scenario: Successful generate
- **WHEN** the user presses Generate Database and the download succeeds
- **THEN** the database contains 1,000 contacts, Friends shows 10, Family shows 100, and All shows 1,000

#### Scenario: The book is reused offline
- **WHEN** the user generates the book and later opens Contacts with no network
- **THEN** the cards and their pictures still appear from the cache

### Requirement: Generate shows progress and keeps a failed run empty
While a generate is running, the canvas SHALL replace Generate Database with a progress bar and text naming how far the download has gotten. The bar SHALL advance as each distinct picture finishes. If the download or the fill fails, the page SHALL return to the explanation and Generate Database, and SHALL NOT leave a partial contact database.

#### Scenario: Pictures advance the bar
- **WHEN** generate has the payload and is downloading pictures
- **THEN** the progress bar fills as each distinct picture finishes and the text names the count

#### Scenario: Failure mid-download
- **WHEN** generate fails after some pictures have been requested
- **THEN** the explanation and Generate Database are back, and a later open still shows that empty prompt

### Requirement: List controls wait for a database
Filter, Sort, and New SHALL be inactive while no contact database exists.

#### Scenario: Nothing to list yet
- **WHEN** Contacts is showing the generate prompt
- **THEN** Filter, Sort, and New cannot be used

### Requirement: Filter selects a nested subset
Friends SHALL show only the friend contacts. Family SHALL show the family contacts, including the friends. All SHALL show every contact. Changing the filter SHALL replace the visible cards with that subset in the current sort.

#### Scenario: Narrow and widen
- **WHEN** the user chooses Friends and then All on the generated book
- **THEN** Friends shows the 10 friends and All shows all 1,000 contacts

### Requirement: Sort reorders the visible cards
Last name, First name, and City SHALL each order the visible cards ascending by that field. Contacts that share the chosen field SHALL keep a stable order. Choosing the sort that is already selected MUST NOT rebuild the list.

#### Scenario: Sort by city
- **WHEN** the user changes the sort from Last name to City
- **THEN** the visible cards are ordered by city ascending

### Requirement: Filter and sort apply after the picker closes
Choosing a new filter or sort SHALL close that picker, and the list SHALL update after the picker is closed.

#### Scenario: Filter closes before the list changes
- **WHEN** the user picks a different filter
- **THEN** the picker closes and the cards then show that subset

### Requirement: Columns follow the card width
The grid SHALL show as many columns as fit a card at least 300 points wide, separated by a 16-point gap. A card SHALL grow to fill its column. When the canvas is at least 300 points wide, a card MUST NOT be narrower than 300 points. The count SHALL be at least one and SHALL change when the canvas width changes. The page SHALL NOT offer a span control.

#### Scenario: Phone width
- **WHEN** the canvas is 390 points wide
- **THEN** the grid has one column

#### Scenario: Portrait tablet
- **WHEN** the canvas is 768 points wide
- **THEN** the grid has two columns

#### Scenario: Landscape tablet
- **WHEN** the canvas is 1024 points wide
- **THEN** the grid has three columns

#### Scenario: Wide desktop
- **WHEN** the canvas is 1440 points wide
- **THEN** the grid has four columns

### Requirement: Reset sits on the right of the title
Contacts SHALL NOT show the playground. Reset SHALL be the same kind of button as New, shown at about half opacity, and SHALL sit at the right of the top bar. It SHALL ask before restoring the book. The title SHALL stay centered in the shell title color.

#### Scenario: Reset is on the title bar
- **WHEN** the user opens Contacts with a database
- **THEN** Reset matches New at the right of the top bar, the title stays centered, and no playground panel is shown

### Requirement: Reset Database restores the generated book
Reset Database SHALL ask once before replacing the book. Confirming SHALL delete the contact database and rebuild it from the cached download, without downloading again. The friend, family, and full counts SHALL return to 10, 100, and 1,000. Canceling the ask SHALL leave the current contacts unchanged.

#### Scenario: Restore after an edit
- **WHEN** the user has changed a contact and confirms Reset Database
- **THEN** the generated contacts are back, the edit is gone, and no new download runs

#### Scenario: Decline the reset
- **WHEN** the user cancels the Reset Database ask
- **THEN** the edited contact remains

### Requirement: A card floats
Every contact card SHALL show a shadow. The card that is open for editing SHALL show a larger shadow than a read-only card.

#### Scenario: Edit lifts the card
- **WHEN** the user presses Edit on a contact
- **THEN** that card's shadow is larger than the shadow on a read-only card

### Requirement: A card is read-only until Edit
A contact card SHALL show the name across the full width. Below that, the picture SHALL fill the left, and the address, email, phone, notes when present, and Edit SHALL sit to its right. The card SHALL NOT show text entries, a state picker, a notes editor, or Delete. Membership SHALL NOT be shown.

#### Scenario: Read a card
- **WHEN** a generated contact is visible and no card is being edited
- **THEN** the name spans the card, the picture fills the left below it, the address, email, phone, and any notes sit to the picture's right, and Edit is at the lower right with no editable fields

### Requirement: A missing picture uses the placeholder
A contact with no picture file SHALL show the bundled placeholder image. A new card SHALL show that same placeholder until a picture is chosen.

#### Scenario: New card picture
- **WHEN** the user starts a new contact
- **THEN** the card shows the placeholder image

### Requirement: Edit turns the card over
Pressing Edit SHALL turn that card over, showing the read-only face leaving and the form arriving upright. Editable fields SHALL exist only on the card that is open, and SHALL be removed after the card turns back to its read-only face. After that turn back, the page SHALL bring the shorter card into view. The card SHALL ignore further input until the turn finishes.

#### Scenario: Turn one card
- **WHEN** the user presses Edit on a contact
- **THEN** that card turns to an upright form and the other visible cards still have no editable fields

### Requirement: The form edits one contact
The form SHALL use the full card width for first name, last name, street, city, state, postal code, phone, email, and notes. A label SHALL sit left of its control, except Photo and Notes, whose labels sit above the value. Notes SHALL have a border. State SHALL be a picker of a blank choice and the US state names. Phone SHALL group a typed US number. Change photo SHALL sit under the picture. Delete and Cancel SHALL sit at the lower left, Delete before Cancel, and Save at the lower right.

#### Scenario: Fields on the form
- **WHEN** a card is open for editing
- **THEN** the form shows those fields, the state picker, Change photo under the picture, Delete and Cancel at the lower left, and Save at the lower right

### Requirement: Edit fields share the notes fill
In dark mode every edit field SHALL use the notes field's dark fill. Entry text SHALL be right aligned. Delete SHALL be red.

#### Scenario: Dark fields and red delete
- **WHEN** a card is open for editing in dark mode
- **THEN** the fields use that dark fill, the entry text is right aligned, and Delete is red

### Requirement: Names are required
First name and last name SHALL be required, MUST NOT be only whitespace, and SHALL be at most 50 characters each.

#### Scenario: Required names
- **WHEN** the user clears the first name
- **THEN** the first name shows an error and Save is disabled

### Requirement: Address and notes have a length limit
Street SHALL be at most 100 characters. City SHALL be at most 50 characters. Notes SHALL be at most 500 characters.

#### Scenario: Street too long
- **WHEN** the user enters a street longer than 100 characters
- **THEN** the street shows an error

### Requirement: Optional fields are checked when present
Email, phone, state, and postal code MAY be empty. A present email SHALL contain a local part, one @, and a domain with a dot. A present phone SHALL be 10 digits. A present state SHALL be a listed US state. A present postal code SHALL be 5 digits or ZIP+4.

#### Scenario: Optional email
- **WHEN** the user leaves email empty and then enters `ada@example.com`
- **THEN** the empty email has no error and the completed email has no error

#### Scenario: Implausible values
- **WHEN** the user enters `ada@example`, a 9-digit phone, and postal code `1234`
- **THEN** email, phone, and postal code each show an error

### Requirement: A field error clears when the value is fixed
Each failing field SHALL show its message. The message SHALL disappear once that field becomes valid.

#### Scenario: Fix the email
- **WHEN** a card shows an error for `ada@example` and the user changes it to `ada@example.com`
- **THEN** the email error is gone

### Requirement: Save keeps a valid card and Cancel restores it
Save SHALL stay disabled while any field has an error. A successful Save SHALL persist the contact, including its picture choice, and turn the card back to read-only. Cancel SHALL restore every field to the values from when Edit began and turn the card back. Cancel on a new unsaved card SHALL remove that card. Neither action SHALL prompt.

#### Scenario: Save a correction
- **WHEN** the user changes a city to a valid value and presses Save
- **THEN** the card turns back and a later visit shows the new city

#### Scenario: Cancel an edit
- **WHEN** the user changes a city and presses Cancel
- **THEN** the card shows the city from when Edit began

### Requirement: Delete is only on the edit form
Delete SHALL appear on the edit form and SHALL NOT appear on the read-only face. Confirming deletion of a saved contact SHALL remove that contact from the list. Declining SHALL leave the contact unchanged. Delete on a new unsaved card SHALL remove the card without asking. A portrait file still used by another contact SHALL remain.

#### Scenario: Read-only has no delete
- **WHEN** a contact is shown on its read-only face
- **THEN** the card has no Delete control

#### Scenario: Confirm deletion
- **WHEN** the user deletes a saved contact and confirms
- **THEN** that contact is gone from the list and from a later visit

#### Scenario: Decline deletion
- **WHEN** the user deletes a saved contact and declines
- **THEN** the contact remains on the form

#### Scenario: Delete a new card
- **WHEN** the user deletes a new unsaved card
- **THEN** the card is removed without a delete confirmation

### Requirement: A second edit asks before discarding
Edit on another card, New Contact, Filter, and Sort SHALL ask before discarding a dirty open card. The ask SHALL offer Discard and Keep editing. Discard SHALL restore the open card, or remove it when it is new and unsaved, and then perform the action. Keep editing SHALL leave the open card as it is and SHALL NOT perform the action. A card with no changes since Edit began MUST NOT ask.

#### Scenario: Edit someone else
- **WHEN** a card has an unsaved change and the user presses Edit on another card and chooses Discard
- **THEN** the first card returns to its saved values and the second card turns to its form

#### Scenario: Keep the open card
- **WHEN** a card has an unsaved change and the user chooses a new sort and then Keep editing
- **THEN** the sort is unchanged and the open card remains

#### Scenario: A clean card yields
- **WHEN** the user opens Edit and, without changing a field, chooses Family
- **THEN** no discard ask appears and Family is applied

### Requirement: Leaving the page asks while a card is dirty
Choosing another flyout entry while a card is dirty SHALL keep Contacts open and show the same Discard and Keep editing ask. Discard SHALL leave the edit unsaved and open the other entry. Keep editing SHALL stay on Contacts. A new unsaved card SHALL count as dirty even when its fields are still blank.

#### Scenario: Leave with a new card
- **WHEN** a new unsaved card is open and the user chooses Colors, then Discard
- **THEN** Colors opens and the new card is not in the contact list

#### Scenario: Stay
- **WHEN** a card is dirty and the user chooses Colors, then Keep editing
- **THEN** Contacts remains open on that card

### Requirement: New contact starts at the top in edit mode
New SHALL insert a blank card at the top of the visible list, scroll that card to the top, focus its first field, and turn it to the form. Its picture SHALL be the placeholder. On Friends the new contact SHALL belong to Friends. On Family it SHALL belong to Family and not to Friends. On All it SHALL belong to neither. After a successful Save it SHALL appear in the filters that membership implies.

#### Scenario: Add a friend
- **WHEN** the user is viewing Friends and presses New
- **THEN** a blank card is scrolled to the top, showing the form and the placeholder, with the first field focused

#### Scenario: Membership follows the filter
- **WHEN** the user saves a valid new contact while Family is selected, then chooses Friends
- **THEN** that contact is in Family and is absent from Friends

### Requirement: Save sorts the card into place
After a successful Save, the visible list SHALL be ordered by the current sort, and the saved card SHALL be brought into view on its read-only face.

#### Scenario: A new name lands in order
- **WHEN** the user saves a new contact whose last name falls in the middle of the current last-name sort
- **THEN** the card leaves the top, appears at its sorted position, and is scrolled into view

### Requirement: Change photo picks one of the cached portraits
Change photo SHALL open a popup with a horizontal list of the dozen cached portraits, plus OK and Cancel. The list SHALL preview a selection without changing the card. OK SHALL show that portrait on the card. Cancel, or dismissing the popup, SHALL leave the card's picture as it was. The popup SHALL NOT be its own page visit.

#### Scenario: Accept a portrait
- **WHEN** the user chooses a portrait in the popup and presses OK
- **THEN** the open card shows that portrait

#### Scenario: Dismiss the popup
- **WHEN** the user chooses a portrait and then cancels
- **THEN** the open card still shows the picture it had before the popup
