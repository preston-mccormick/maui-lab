# Contact lessons

## Reset is a button, not a navigation-bar item

A `ToolbarItem` is drawn by the navigation bar, so it stays a tinted link and can pick up the title color. A button placed in the centered title is clipped. Reset uses the same button as New and sits at the right of the top bar.

## Close the filter before changing the list

On iOS the picker stays open while its selection changes. Rebuilding the list in that moment blocks the dismiss animation, so the cards change and then the picker vanishes. The selection closes the picker first. The list updates about a third of a second later, after the picker is gone.

## A shorter card has to be scrolled after the turn

The open form is taller than the read-only card. Scrolling while the form is still up uses that tall height, so the short card can sit off screen once the turn finishes. The page scrolls to the card again after the read-only face is back.

## A picker shadow has to follow the rounded photo

The collection view paints a square behind the selected item, and the control shadow follows the view bounds rather than the rounded photo. Once the selected card grows, that square shows around the picture. The picker tracks its own selection, and on iOS the shadow is a rounded layer path on a transparent card.

## A form-height row fills the phone

A 640-point row and `MeasureFirstItem` made every contact the height of the edit form. On an iPhone that is one card, the Edit button sits below the canvas, and the card has no visible edge. Size the read-only card to its text, inset it on a grouped canvas, and use `MeasureAllItems` so the open form can grow alone.

## iOS can load only one SQLite

Opening Contacts, and rotating the phone, both died in `sqlite3PcacheTruncate` at the same address. The app had linked the bundled `e_sqlite3.a` and `/usr/lib/libsqlite3.dylib`. A handle from one copy was stepped by the other, so the page cache pointer was `getPageNormal` in the system library. On iOS the usage store selects the system provider and the bundled archive is not linked.

## Perspective is M34, and it has to be set on every frame

The Mac Catalyst binding names the perspective field `M34`. `RotateYToAsync` replaces the layer transform and clears it, so the turn squashes flat. The flip animation sets `RotationY` and then `M34` on each frame.

## Clear the SQLite pool before deleting the book

Entity Framework keeps SQLite connections in a pool. Deleting `contacts.db` and calling `EnsureCreated` reuses that connection, so SQLite reports that the Contacts table already exists. `SqliteConnection.ClearAllPools()` has to run before the file is deleted.
