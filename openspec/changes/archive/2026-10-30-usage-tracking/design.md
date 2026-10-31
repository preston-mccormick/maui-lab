# Design

## Context

See proposal.md for why this change exists. The baseline log and timeline are already in the `MauiLab/Usage` vertical slice. This design adds window area, popup filtering, page labels, Menu Button and Dialogs actions, and a right-hand timestamp.

The app is a .NET MAUI lab. Experiments live in their own folders. Shell flyout items are `ShellContent` data templates with parameterless page constructors. Window lifecycle is wired on the `Window` created in `App.CreateWindow`.

## Goals / Non-Goals

**Goals:**

- Keep recording and presentation separate: an append-only event log, and a projection of that log into timeline rows.
- Match the timeline UI: header, nested rows, theme-specific rail and markers, live open-visit text, a right-hand timestamp, and a confirming reset.
- Record window size, and a mode only when the rectangle distinguishes it.
- Record Menu Button and Dialogs actions under the current page.

**Non-Goals:**

- Sending the log anywhere.
- Treating alerts, prompts, action sheets, or toolkit popups as page visits.
- Guessing Split View versus Slide Over when both fit the rectangle.
- A third-party timeline control.
- Multiple windows. Only the window created in `App.CreateWindow` is tracked.

## Decisions

### Two capabilities, one folder

Recording (`usage-log`) and the page (`usage-timeline`) are separate specs. Both live in `MauiLab/Usage` because the lab organizes by experiment, not by layer.

`UsageTracker` owns the state machine and is the only writer. `UsageTimeline.Project` is a pure function from events plus `now` to rows. The page never writes events except through Reset.

### SQLite inserts, not a rewritten file

The store is `sqlite-net-pcl` with `SQLitePCLRaw.bundle_green` 2.1.11 and native `e_sqlite3` 2.1.14 (the 2.1.10 native library is a known vulnerable SQLite build). The file is `FileSystem.AppDataDirectory/usage.db`, opened on first use so the path is resolved after the platform is ready.

Each call inserts one `usage_event` row. `UploadedUtc` is forced null on insert. Sequence is `MAX(Sequence)+1`. Reads are `ORDER BY TimestampUtcTicks, Sequence`. Indexes: `(TimestampUtcTicks, Sequence)` and `(UploadedUtcTicks)`. File databases use WAL and `busy_timeout`. In-memory databases skip WAL.

A JSON file rewritten after every event was rejected because a long history would be rewritten on each event. The upload column is the outbox; there is no sender.

Row fields: id, sequence, kind, UTC timestamp, session id, page-visit id, parent id, name, route, elapsed milliseconds, active milliseconds, end reason, width, height, mode, uploaded UTC ticks.

Kinds: Installed, HistoryReset, SessionStarted, SessionEnded, Backgrounded, Foregrounded, PageOpened, PageClosed, AreaChanged, ControlPressed, SurfaceOpened, SelectionMade, Updated, PermissionRequested. End reasons: Clean, Terminated. A permission result is stored in `Route`. An update stores the version in `Name`.

`CreateTable` does not add columns to a database that already exists. After creating the table, the store adds `Width`, `Height`, `Mode`, and `ParentId` with `ALTER TABLE` when `GetTableInfo` does not already list them.

### Session stays open across sleep

`App` maps window signals onto the tracker:

- `Activated` and `Resumed` call `Activate` with the shell's current page title, route, and page type name, then record the window area.
- `SizeChanged` records the window area again.
- `Deactivated` and `Stopped` both call `Background`. The second call is a no-op while already backgrounded, so deactivate-plus-stop is one event.
- `Destroying` calls `EndSession` (Clean).
- `Shell.Navigated` calls `Navigate` with the title, route, and page type name.

A failure in any of those handlers is written with `Debug.WriteLine` and does not escape.

Cold start: if this process has not started a session, close any DB session that has no end, write Installed only when the log is empty, then write SessionStarted and open the current page. That activation does not also write Foregrounded.

Resume: if a session is open and the tracker is backgrounded, write Foregrounded on the open visit, then change pages only if the reported page is a different title.

Page identity: when both sides have a title, titles must match. A later navigation that only adds a route does not split the visit. Blank title and route are ignored. A navigation that arrives before the first activation is held and used if activation has no page yet.

A navigation is ignored when the page type name starts with `PopupPage` or the route contains `PopupPage`. That is the Community Toolkit host: `ShowPopup` registers `PopupPage` plus a unique route and navigates to it, and Shell can also show it as `D_FAULT_PopupPage` plus a counter. The menu that opened it is recorded as an action, not as a page.

Active time: a visit starts a foreground slice at open. Each Backgrounded ends the slice. Each Foregrounded starts the next. Close adds a trailing slice only when a slice is still open and the close time is later than the slice start. A visit opened while backgrounded also writes Backgrounded at the same timestamp so the opening slice is zero.

Killed process: on the next activation, any SessionStarted without SessionEnded is closed first. If the last signal (background, foreground, page open, page close, or session start) is Backgrounded, close at that timestamp. Otherwise close at the last event in the session. Reason Terminated. Then the new session starts. The gap until this launch is not elapsed time.

Destroying the window closes the open visit at now, writes SessionEnded with elapsed and the sum of that session's closed-visit active times, and clears the in-process session so a later activation can start another one.

Reset deletes all rows, writes HistoryReset, then starts a session and opens the page that was passed in (the current shell page). It does not copy an in-progress menu or dialog into the new log.

Alternatives considered: ending the session on every background (rejects the sleep example, where one visit spans the night), and a separate Suspended event (the OS does not deliver the freeze).

### Window area

`NoteArea` stores width and height from `Window` in device-independent points, on the page visit that is open. It runs after activation and on `SizeChanged`. A size below 1 point is ignored. A later record is written only when width or height differs by at least 1 point. Each new session forgets the previous size so the same rectangle is recorded again. The timeline title is the mode, or Screen when the mode is unset. The detail is the rounded size, such as `1180 × 820`.

The screen comparison uses `DeviceDisplay.MainDisplayInfo` width and height divided by density. There is no MAUI or UIKit value for Slide Over. `UsageArea.Classify` returns:

- Full screen, when the window matches the screen within 24 points of width and 80 points of height and sits at the origin.
- Split View, when the window is nearly full height, edge aligned, and its width is about one third, one half, or two thirds of the screen, and that width is outside the compact overlay band.
- Slide Over, when the window is nearly full height, between 280 and 420 points wide, and that width is not a split fraction.
- Windowed, when the window is otherwise clearly smaller than the screen.
- No mode, when the compact band also matches a split fraction, or when the screen size is unknown.

`DeviceDisplay` is the full screen. The window rectangle is the area the app occupies, so Split View, Slide Over, Stage Manager, and a Mac window all change it. The Mac Catalyst lab window is 1250 by 750, which classifies as Windowed on a larger display.

### Menu Button and Dialogs

`BeginInteraction` writes `ControlPressed` and `SurfaceOpened` on the open page visit. The surface row's parent is the button row. `NoteChoice` stores the selection. `CompleteInteraction` writes `SelectionMade` under the surface, using `Dismissed` when no choice was noted. Closing the page finishes an open interaction the same way.

`MenuButton` calls this around `ContextMenu.ShowAsync`, with the button text and the surface name Menu. `MenuItem.Tapped` notes the item text before `BeforeTap` closes the popup, so the choice is stored before `ShowAsync` returns. A tap outside the menu notes nothing, so the selection is Dismissed.

Dialogs calls the same API around the existing alert, action sheet, and prompt. The button names are Show Alert, Show ActionSheet, and Show Prompt. The surfaces are Alert, Action sheet, and Prompt. The alert result is Acknowledge. The action sheet result is the returned button, or Dismissed when that call returns null. The prompt result is the entered text, or Dismissed when the call returns null. The toast that already follows a result stays.

Controls resolve `UsageTracker` from `IPlatformApplication.Current.Services` and skip recording when that service is missing.

### Timeline projection

`Project(events, now)` orders events by timestamp then sequence. The root is the latest Installed or HistoryReset. Only that root and events at or after it are shown.

Each session is one block, placed by its newest event so later sessions stay above older ones. Inside a session every visible event is sorted newest first by timestamp, then sequence. `PageClosed` and `SessionEnded` are not rows; their durations stay on the page and session rows. Because a later event sorts first, activity sits above the page navigation that started it, and a selection sits above its menu and button. A newer page is back at depth 1, so the graph outdents and then indents for that page's later events. `Updated` rows are their own blocks on the spine, ordered by their timestamp among sessions. The root row is last.

Closed visits use the durations stored on PageClosed. Open visits recompute from PageOpened, the lifecycle events, and `now`.

Depths: session, update, and root 0; page 1; lifecycle, screen, permission, and button 2; surface 3; selection 4. A screen or permission event with no page stays at depth 1. The title's left margin is `30 + depth * 22`, which puts it just to the right of its node. The timestamp is a separate column and is not indented. Stable ids: page-visit id, event id, session id, root event id.

Duration text is whole seconds: `8h 5m`, `8h`, `5m`, `1m 1s`, `30s`, `0s`. Hour scale drops seconds. A page title is `Navigated to {name} page`. Page detail is `in progress · open {elapsed} · active {active}` or `open {elapsed} · active {active}`. A screen title is the mode, or `Screen`. Its detail is the size. Activity, button, surface, and selection details are empty. A permission title is the permission name and its detail is the status. An update title is `Updated` and its detail is the version.

Every row has a `When` string with seconds: local `h:mm:ss tt`, or `MMM d, h:mm:ss tt` in the same year, or `MMM d, yyyy, h:mm:ss tt`. Page rows use the open time. Other event rows use their event time. An open session detail is empty and `When` is the start time. An ended session detail is `open` and `active`, prefixed with `terminated · ` when the reason is Terminated. The root `When` is `MMM d` or `MMM d, yyyy`.

`TimelineRail.Compute` decides, for each depth, whether the node column continues to the next row, curves down into a shallower column, or lets a deeper next row start a branch. Ancestor columns that still have a node above and below draw a straight pass-through. `UsageRailView` paints those segments in the row's real height, so the lines meet. The curve is a quadratic path, not a bitmap, because a page row with a detail line is taller than a one-line event. Row padding is 0.

Category colors live in `Colors.xaml` and are shared by the title triggers and the node. Navigation `#1D4ED8` / `#93C5FD`. Screen `#3F7D1F` / `#B8DC95`. Lifecycle `#C026D3` / `#F5A8FF`. Action `#C2410C` / `#FDBA74`. Permission `#6D28D9` / `#C4B5FD`. The first value is the light theme and the second is the dark theme. The graph line uses Gray / LighterGray.

A custom `CollectionView` was chosen over a commercial timeline. Those controls are linear event lists and do not model nested session and page spans or the two clocks.

### Usage page

`AppShell` adds `<ShellContent Title="Usage" ContentTemplate="{DataTemplate usage:UsagePage}" />`.

`UsagePage` has a parameterless constructor and resolves the singleton `UsagePageModel` from `IPlatformApplication.Current.Services`, with `Application.Current.Handler.MauiContext.Services` as fallback. Shell data templates cannot take a DI constructor. `OnAppearing` calls `StartClock(Dispatcher)`. `OnDisappearing` calls `StopClock`.

The page is a two-row grid, padding `12,12,12,0`, row spacing 8.

Header row, two columns:

- Title label, font size 25, text `Usage`. Default label style supplies theme text color.
- Subtitle label, font size 14, text `Sessions, pages, and time in the foreground.` `TextColor` is `AppThemeBinding` DarkGray / LighterGray.
- Reset button in column 1, bound to `ResetCommand`, vertically centered. Default button style.

List row: `CollectionView`, `ItemsSource` bound to `Rows`, `SelectionMode="None"`, `ItemsUpdatingScrollMode="KeepScrollOffset"`.

Item template, compiled binding to `TimelineRow`. A grid, columns `*,Auto`, padding 0, `MinimumHeightRequest="44"`. `UsageRailView` spans both columns, bound to `Rail`, and fills the row so each segment meets the next row. The title stack uses `Margin="{Binding TextIndent}"`. The title is bold, font size 16, and its color is a `DataTrigger` on `Category`. The detail label is font size 13, DarkGray / LighterGray, hidden when `HasDetail` is false. Column 1 is the timestamp: font size 13, the same gray, end aligned, bound to `When`.

`UsagePageModel` holds an `ObservableCollection<TimelineRow>` and subscribes to `UsageTracker.Changed`. `StartClock` stops any previous timer, refreshes, then starts a repeating one-second `IDispatcherTimer`. `Changed` marshals to that dispatcher when required. `Refresh` calls `Project`. If the row ids are unchanged, it copies `Detail` onto the existing rows so the binding updates in place and the collection is not rebuilt. Otherwise it clears and re-adds. The timer tick is not an event.

Reset shows `DisplayAlertAsync("Reset history", "This clears the usage log and starts over.", "Reset", "Cancel")`. Confirm calls `UsageTracker.Reset` with `Shell.CurrentPage.Title` and `Shell.CurrentState.Location`.

Services, registered in `MauiProgram`: `TimeProvider.System`, `IUsageStore` as `SqliteUsageStore`, singleton `UsageTracker`, singleton `UsagePageModel`. `App` takes `UsageTracker` in its constructor.

### Tests

`MauiLabTests` drives `UsageTracker` with `ManualTimeProvider` and `SqliteUsageStore.InMemory`, plus one temp-file reopen. They cover the sleep clocks, repeated slices, equal active and elapsed, coalesced background, cold start versus resume, killed-process close, clean window close, reset, same-page navigation, newest-first depths, insert order with a null upload timestamp, popup-route suppression, window area and mode, and nested button actions. UI theme and the alert are not unit-tested; they are the XAML contract above. `UsageArea.Classify` is tested without a window.

## Risks / Trade-offs

- [Mac Catalyst `Deactivated` fires on focus loss, not only on sleep] → Active time pauses whenever the window is inactive. That matches "foreground" and will look noisy on desktop.
- [iOS often kills a suspended app without `Destroying`] → The next launch infers Terminated. Time after the last event, if the app was still in front, is dropped.
- [Installed means first observed launch] → Reinstall that wipes app data looks like a new install. Uninstall is invisible.
- [Page identity is the title] → Two shell pages with the same title would collapse into one visit.
- [`DataTrigger` plus `AppThemeBinding` must stay in the item template] → Setting marker colors in C# at projection time would freeze the previous theme until the next rebuild.
- [Full row replace on a new event resets which rows exist] → Scroll offset is kept by the collection view, but the new row appears at the top. In-place updates are only for the ticking open visit.
- [Split View and Slide Over overlap near 320 points] → The size is still stored. The mode is left unset instead of picking one.
- [A menu choice is noted before the popup closes] → If that order were reversed, `ShowAsync` could finish and record Dismissed before the item text was seen.

## Migration Plan

Opening an existing `usage.db` adds `Width`, `Height`, `Mode`, and `ParentId` when those columns are missing. Older rows read back with empty area and parent fields. No upload is added.

### Release and permission metadata

`PlatformInstall.Read` returns Android `PackageInfo` first-install and last-update times. On iOS and Mac Catalyst it returns nothing; those platforms have no public install or update time. `App.OnActive` calls `NoteRelease` with `AppInfo` version and build before `Activate`. An empty log writes Installed at the system install time when there is one, otherwise at now, and stores the version. A system update time more than a minute after install is written once. A later version string writes `Updated` at now.

`RecordPermission` writes `PermissionRequested` on the open page. `GeolocationViewModel` checks location when in use, and calls `RequestAsync` only when it is not already granted, then records the status. The Mac Catalyst and iOS plists include `NSLocationWhenInUseUsageDescription`. There is no camera page, so camera is not requested.

## Open Questions

None. The graph uses drawn curves rather than bitmap tiles so the line still meets when a row is one line tall and the next is two.
