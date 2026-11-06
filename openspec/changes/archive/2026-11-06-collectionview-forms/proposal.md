# Proposal

## Why

Editable controls inside a CollectionView item template have been slow and unreliable on another MAUI app. This lab needs a Contacts page that keeps the cards read-only, flips one card into a form, and can show that cost at about 10, 100, and 1,000 rows.

## What Changes

- Add a top-level flyout entry titled Contacts, directly under Colors. The page title is Contacts. The canvas is a vertical CollectionView grid of contact cards, hosted on the existing playground. The playground starts collapsed and holds Reset Database where the grid page holds Reset. It has no catalog, span, spacing, sizing, grouping, scroll, item, or timings levers. The column count follows the canvas width: as many cards as fit at 300 points wide, with a 16-point gap. A phone shows one column, a portrait tablet two, a landscape tablet three, and a wider window more. The card stretches to fill its column.
- A top bar, in the style of the Colors toolbar, offers Filter, Sort, and New Contact. Filter is Friends, Family, or All. Sort is last name, first name, or city. The page opens on All, sorted by last name, once a database exists. Until then those controls are inactive.
- The first visit with no database shows a message and a Generate Database button. Generating downloads 1,000 US people from Random User Generator API 1.4 with seed `maui-lab`, plus a dozen extra portraits, caches the JSON and image files, and fills a local SQLite database. A progress bar and a count replace the button while the pictures download. A failure leaves the prompt and does not keep a partial database. Friends are the first 10 of that set, Family is the first 100, and All is the full set.
- Reset Database asks once, then deletes the database and rebuilds it from the cache. It does not download again. The playground stays the place for controls that should stay out of the way.
- Each card shows a picture at the upper left and the contact's name, address, phone, email, and notes. Edit sits at the lower right. A missing picture uses a bundled placeholder. The card floats on a shadow, and the open card's shadow is larger so it sits higher. Pressing Edit turns the card over: the read-only face on one side, the form on the other. The form is created for that card only, and removed after the card turns back. The card keeps a fixed height through the turn.
- The form edits first name, last name, street, city, state, postal code, phone, email, and notes with stock MAUI controls. First and last name are required. Email, phone, state, and postal code are optional and, when present, must be plausible. Phone is masked for display. State is a picker. Save stays disabled while the card has errors. Cancel throws away the edit. Delete sits to the left of Cancel and is absent from the read-only face. Deleting a saved contact asks once, then removes the row. Deleting a new card removes it without asking. Shared portrait files stay. A new card starts blank, at the top, already turned to the form, with the placeholder picture.
- Only one card is open. Edit on another card, New Contact, Filter, Sort, and leaving the page ask before discarding a dirty card. A clean card does not ask. Reset Database's confirm is the only prompt for that action. Saving a valid card writes it, sorts it into the current list, and scrolls it into view.
- Edit mode can replace the picture from a popup: a horizontal CollectionView of the dozen cached portraits, with OK and Cancel. The choice is applied when OK is pressed.
- Contacts are stored with Entity Framework Core and SQLite. The entity is the item model's storage. Validation annotations live on the entity. The change tracker of the one attached card is the undo checkpoint. Picture files stay outside the database. The app pins one `SQLitePCLRaw` version for Contacts and the usage log. The usage log stays on `sqlite-net-pcl` and its own file.

## Capabilities

### New Capabilities

- `contacts`: The Contacts flyout page, the generated address book, the read-only and editable card, and the local SQLite store.

### Modified Capabilities

## Impact

- `MauiLab/AppShell.xaml`: a `ShellContent` titled Contacts immediately after Colors.
- `MauiLab/Contacts`: the page, the card, the photo popup, the page model, the item model, the EF Core `Contact` and `DbContext`, and the generator that caches the Random User payload and image files under app data.
- New package: `Microsoft.EntityFrameworkCore.Sqlite`, aligned with the net11 SDK. `SQLitePCLRaw.bundle_green`, `SQLitePCLRaw.lib.e_sqlite3`, and `SQLitePCLRaw.lib.e_sqlite3.android` move to one shared version. `sqlite-net-pcl` stays. `SqliteUsageStore` changes only if that pin changes how the native library starts. The two databases remain separate files.
- `MauiLabTests`: headless tests for import, group counts, validation, save, discard, delete, column counts, a canceled new card, and reset from the cache. The usage-store tests stay green on the shared native library. Tests use a fixture and a stub downloader. The flip, the shadow, the scroll-into-view, and cell recycling stay a device check.
- No change to Colors, Collections, Forms, or the usage log's action list. A Contacts visit is still an ordinary page visit. The discard alert and the photo popup are not page visits.
