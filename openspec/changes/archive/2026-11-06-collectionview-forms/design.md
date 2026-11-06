# Design

## Context

See proposal.md for why this change exists. Behavior is in the `contacts` delta.

`AppShell` lists Colors as a top-level `ShellContent`, then the Layouts flyout group. `LayoutPlayground` already docks and collapses its panel. Contacts does not use that panel. Reset is a button on the top bar, the same kind as New. `CollectionToolbar` is the grid page's Reset plus the timings switch, bound to `CollectionPageModel`. Contacts does not use that model or that toolbar.

Page models in this lab are created in `OnAppearing` and take their dependencies through the constructor. Tests call those constructors without a handler. The usage log already has its own SQLite file through `sqlite-net-pcl`. Its native packages are split today: `SQLitePCLRaw.bundle_green` 2.1.11 against `SQLitePCLRaw.lib.e_sqlite3` 2.1.14. `SqliteUsageStore` is an append-only writer with a long-lived connection, a lock, and an on-disk schema of string ids and tick counts. Dialogs already shows a toolkit popup with buttons, and `Shell.Current.GoToAsync` pushes a page that does not return a value. Alerts and toolkit popups are already excluded from page visits.

`CommunityToolkit.Maui` 15.0.1 is referenced, including `MaskedBehavior`. `CommunityToolkit.Mvvm` 8.4 supplies `ObservableObject` and commands. There is no Entity Framework package yet.

## Goals / Non-Goals

**Goals:**

- A page model, an item model, and a store that tests can run without a handler or the network.
- One card in the realized tree owns the editors. The other cards are text and an image.
- The open card's Entity Framework original values are the undo checkpoint.
- Generate is the only network trip. Reset rebuilds from the cache.
- One native SQLite for the usage log and the contact book.

**Non-Goals:**

- A camera, or the system photo library.
- Moving the usage log onto Entity Framework. The log stays an append-only `sqlite-net` store.
- Usage action rows for filter, sort, save, or reset. A Contacts visit is still recorded as a page visit.
- Shipping the portrait files in the repo.
- Tracking all 1,000 entities, or a second copy of each contact beside the entity.
- Timings, span, spacing, or the other collection levers.
- International addresses. The book is US data from `nat=us`.

## Decisions

### Contacts is its own page, and the columns follow the card

`AppShell` gains a `ShellContent` titled Contacts immediately after Colors. The page title is Contacts. `MauiLab/Contacts` holds the page, the card, the popup, the models, and the store as siblings.

The page is a grid: the Colors-style toolbar on top, and the contact canvas filling the rest. There is no playground panel. Reset is the same kind of button as New, at about half opacity, and sits at the right of that top bar. A navigation-bar item stays a tinted link, and a button inside the title view is clipped. The title stays the shell title. The canvas is a `CollectionView` with a vertical `GridItemsLayout` and `MeasureAllItems`. There is no span lever. The span is how many 300-point cards fit the canvas width with a 16-point gap, and at least one. The card then stretches across its column. A width of 390 yields one column, 768 yields two, 1024 yields three, and 1440 yields four. A resize refits and records nothing. Below 300 points the single card uses the canvas width. The color grid's fit stays at a minimum of two columns and a 180-point swatch. This page does not share that measure, because a contact form is wider than a swatch.

The model and the store are created in `OnAppearing`, as the collection pages do. The constructor takes the store and a shell-navigation hook so tests can call filter, sort, new, save, and reset without a page.

### The entity is the item model's storage

`Contact` is the EF entity and the only copy of the fields: id, first name, last name, street, city, state, postal code, phone digits, email, notes, photo file name, and group. Group is `Friend`, `Family`, or `Other`. Indexes 0 through 9 of the generated payload are `Friend`, 10 through 99 are `Family`, and the rest are `Other`. Friends filters `Friend`. Family filters `Friend` and `Family`. All returns every row. A new card on Friends is `Friend`, on Family is `Family`, and on All is `Other`. Group is not bound to the card.

`ContactItemModel` is an `ObservableObject` and `INotifyDataErrorInfo`. Its field properties get and set `Contact`. It does not use `ObservableValidator`, because that would move the annotations onto the wrapper. The annotations stay on `Contact`. After each write the item model validates that property with `Validator.TryValidateProperty` and raises `ErrorsChanged`. The page binds a short error label under each field.

Data annotations: first and last name `[Required]` and `[MaxLength(50)]`, street `[MaxLength(100)]`, city `[MaxLength(50)]`, notes `[MaxLength(500)]`. Email, phone, state, and postal code use a small custom attribute each so an empty value passes and a present value must match the spec. Phone is stored as ten digits. The entry shows the grouping through `MaskedBehavior`, and the value written to the entity is the unmasked digits. State is a picker of name-and-code pairs plus a blank. The card's read-only line shows the code.

The list is loaded with `AsNoTracking`. The page keeps those instances. A context is opened only for generate, load, save, reset, and the one open card.

### Original values are the checkpoint

Edit attaches that one entity as `Unchanged` before the form is shown, so `OriginalValues` are the saved fields. Typing changes `CurrentValues` and marks the entity `Modified`. A clean card is `Unchanged`. Discard assigns `CurrentValues.SetValues(OriginalValues)`, sets the state back to `Unchanged`, and disposes the context. Setting the state alone would keep the typed values. Save calls `SaveChanges`, which refreshes `OriginalValues`, then disposes the context. The list keeps the same instance.

A new card is not attached. It is dirty even while blank. Cancel removes it from the list. Save uses `Add`. There is no original row to restore.

Delete is on the edit face, to the left of Cancel. The read-only face has no Delete. A saved contact asks "Delete this contact?" and confirming calls `Remove` and `SaveChanges` on the open context, then drops the card. Declining leaves the form as it is. A new card is removed with no second ask, because nothing has been inserted. Portrait files are a shared pool, so delete does not remove a file.

`State = Unchanged` is not the revert. Relational savepoints are not used. They roll back SQL already sent inside a transaction, and this form has not been sent.

Reset Database does not stack a second discard ask on top of its own confirm. The confirm says that unsaved edits will be lost. Accepting it disposes the edit context without saving and rebuilds the database.

### Generate once, reset from the cache

`ContactCatalog` is the HTTP boundary. Generate requests:

`https://randomuser.me/api/1.4/?results=1000&seed=maui-lab&nat=us&inc=name,location,email,phone,picture`

and a dozen portraits from the same API version with seed `maui-lab-photos` and `inc=picture`. Portrait URLs repeat, so the downloader fetches each distinct medium URL once. Files land in a temp directory, then move into app data under `contacts/portraits` and `contacts/picker`. The payload is saved as `contacts/seed.json`. Rows are inserted in one transaction. Any failure deletes the partial database and the temp files and leaves the generate prompt. The cache from a failed run is not promoted.

The importer builds street as number plus name, maps the full state name to a two-letter code, stores phone digits, and accepts a numeric or string postal code. A leading US `1` is stripped from an 11-digit phone. A four-digit postal code is left-padded to five. If a row still fails validation, generate fails the whole run. Notes are empty. Login, id, and the SSN-shaped field are not requested and not stored.

Reset deletes `contacts.db` and inserts again from `seed.json` and the existing files. It does not download. If the cache is missing, Reset is inactive and the page shows the generate prompt. Tests pass a fixture through `ContactCatalog` and never call the host.

The empty canvas copy tells the user that generate downloads 1,000 people and their photos. The button is replaced by a progress bar and a count. The bar stays at the start while the payload downloads. Once the distinct portrait list is known, the bar is pictures finished divided by that count.

### The card turns, and only that card builds a form

`ContactCard` is a `Border` with a shadow, so the card floats above the canvas. The read-only shadow is shallow. While the card is open, the shadow radius and offset increase so that card reads as higher. The read-only face is XAML. The name spans the card. Below it, the picture fills the left and the address, email, phone, notes, and Edit sit to its right. The edit face is built in code when Edit begins, then the turn starts on the next frame so creating the entries does not hitch mid-animation. The form uses the card width: each label is left of its control, except Photo and Notes, which put the label above a full-width value. In dark mode the fields use the notes fill, `#2C2C2E`, and the entry text is right aligned. Delete is red. The notes editor is wrapped in a border because an Editor has none of its own. After the turn back finishes, the edit face is removed, the shadow returns to the shallow value, and the page scrolls the shorter card into view. A recycled cell snaps to the item's open or closed state and does not replay a half-finished turn. Input is ignored while the turn runs.

The turn is `RotationY` from 0 to 90, a swap at the edge, the incoming face set to -90 so its text is not mirrored, then `RotationY` back to 0. Each half is about 200 ms, ease-in then ease-out. A small handler sets perspective: `m34` on iOS and Mac Catalyst, `CameraDistance` on Android. Windows has no equivalent transform, so the same timing runs as `ScaleX` from 1 to 0 and back to 1.

The read-only card sizes to its text, with a margin so the canvas shows around it. The canvas is the grouped background. The card is white in light mode and a lighter gray in dark mode. `MeasureAllItems` lets the open form grow without stretching every other row. A shared form height filled an iPhone with one clipped card.

Change photo uses `ShowPopupAsync`, as Dialogs does. The popup shape is filled with the page's light or dark surface, because the toolkit shape defaults to white. A horizontal list shows the twelve picker files, and the list's own selection highlight is left off. Each photo is clipped flush to a transparent rounded card. The shadow is a rounded layer path: the control shadow follows the rectangular bounds and, once the card grows, paints a square behind the photo. The selected card animates up and larger, that rounded shadow deepens, and its border thickens and shifts to the accent color. The selection stays inside the popup until OK writes `PhotoFile` on the entity. Cancel writes nothing.

After Save, the page waits for the turn back, re-sorts the visible list, yields one dispatcher turn, and calls `ScrollTo` with the saved item. Replacing the order and scrolling in the same turn runs before the item is realized.

### Leaving is cancelled in Shell.Navigating

The page subscribes while it is visible. If the open card is dirty, the handler cancels the navigation and shows the discard alert. Discard reverts, or drops a new card, then navigates to the target that was cancelled. Keep editing stays. A re-entrant navigation caused by that second navigate must not ask again. Filter, sort, Edit on another card, and New Contact use the same alert and the same dirty check. A clean attached card does not ask.

The discard alert is "Discard changes?" with Discard and Keep editing. The reset alert is "Reset Database?" and says the generated book will replace the current contacts, including unsaved edits. The delete alert is separate and is not stacked with the discard alert.

### One native SQLite, two stores

`SQLitePCLRaw.bundle_green`, `SQLitePCLRaw.lib.e_sqlite3`, and `SQLitePCLRaw.lib.e_sqlite3.android` are pinned to one version, the version `Microsoft.EntityFrameworkCore.Sqlite` accepts on `net11.0`. `sqlite-net-pcl` stays. Contacts opens `contacts.db` through Entity Framework. The usage log opens `usage.db` through `SqliteUsageStore`. If the pin changes `Batteries_V2.Init`, that call in `SqliteUsageStore` is updated. The usage schema, the append-only row shape, and `IUsageStore` stay as they are. The usage log is not given a `DbContext`.

## Risks / Trade-offs

- [EF Core and the usage log pull two `SQLitePCLRaw` versions] → Pin bundle, e_sqlite3, and the Android library to one version. Keep `sqlite-net` and a separate `usage.db`. Run `SqliteUsageStoreTests` after the pin.
- [iOS linking strips EF] → The first device build is the check. Add the linker preserve the build actually needs, and no broader one.
- [`RotationY` stays a flat squash without perspective] → The handler sets `m34` or `CameraDistance`. Windows uses the scale fallback.
- [A recycled cell keeps another contact's editors] → The edit face is created for the open item and removed when it closes. Reuse snaps to that item's state.
- [ScrollTo runs before the new order is laid out] → Scroll after the turn and one dispatcher turn.
- [Random User changes a record behind an unversioned URL] → The request is pinned to API 1.4 and a seed. A row that cannot be made valid fails the generate.
- [Generate takes long enough to feel stuck] → The button is explicit. A progress bar fills as the distinct portraits finish, with a count beside it.
- [A wide form drops the landscape tablet to two columns] → The 300-point minimum is the starting measure. If the laid-out form needs more, raise that minimum and update the column scenarios with it.
- [Attach after the first keystroke stores the dirty card as the original] → Attach happens before the form is shown.

## Migration Plan

No existing contact database and no usage-log schema change. The native SQLite pin can replace the binary the usage log already loads. The `usage.db` file stays readable. Removing the page and the EF package leaves that file as it is. A bad generate deletes its partial database and returns to the prompt.
