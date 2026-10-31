# Tasks

## 1. Usage log

- [x] 1.1 Confirm `ColdStart_DoesNotRecordForegrounded` and `Resume_RecordsForegroundWithoutNewSession` pass, covering install-once, one session on cold start, and a foreground event on resume
- [x] 1.2 Confirm `RepeatedBackground_RecordsOneEvent`, `SleepOnPage_RecordsElapsedAndActiveSeparately`, `RepeatedSleepCycles_SumOnlyForegroundSlices`, and `StayingForeground_ActiveEqualsElapsed` pass
- [x] 1.3 Confirm `KilledWhileBackgrounded_ClosesAtBackgroundTime` and `ClosingTheWindow_EndsTheSessionCleanly` pass, including Terminated versus Clean
- [x] 1.4 Confirm `NavigatingToTheSamePage_DoesNotSplitTheVisit` and `Reset_ReplacesHistoryWithResetRoot` pass
- [x] 1.5 Confirm `InsertsComeBackInTimestampOrderWithUploadPending` and `FileDatabase_SurvivesReopen` pass, with empty upload timestamps

## 2. Timeline

- [x] 2.1 Confirm `Timeline_IsNewestFirstAndNested` passes with the labeled page titles, depths, duration strings, empty activity detail, and right-hand timestamps from the usage-timeline spec
- [x] 2.2 Confirm `UsagePage.xaml` has the Usage header and subtitle, Reset button, non-selectable `CollectionView` with `KeepScrollOffset`, 22-point indent on the rail and title, 2-point rail, 12-point marker, a right-aligned timestamp that is not indented, and the light/dark marker and text colors in design.md
- [x] 2.3 Confirm `UsagePage` starts the one-second clock on appear and stops it on disappear, and `UsagePageModel.Refresh` copies `Detail` in place when row ids are unchanged
- [x] 2.4 Confirm Reset shows the "Reset history" alert and calls `UsageTracker.Reset` only after the user confirms

## 3. Wiring

- [x] 3.1 Confirm `App.CreateWindow` maps activate, resume, deactivate, stop, destroying, and `Shell.Navigated` onto the tracker, and `MauiProgram` registers the SQLite store, `UsageTracker`, and `UsagePageModel`

## 4. Window area

- [x] 4.1 Record width and height at session start and when the size changes by at least one point, with a mode only when the rectangle distinguishes Full screen, Split View, Slide Over, or Windowed
- [x] 4.2 Leave the mode unset when a compact full-height edge-aligned window also matches a split fraction

## 5. Pages and popups

- [x] 5.1 Ignore navigations whose page type or route is PopupPage so the current visit stays open
- [x] 5.2 Title page rows "Navigated to {name} page"

## 6. Controls

- [x] 6.1 Record Menu Button presses as the button, then Menu, then the chosen item or Dismissed, nested under the page
- [x] 6.2 Record Dialogs Show Alert, Show ActionSheet, and Show Prompt as the button, then Alert, Action sheet, or Prompt, then the result or Dismissed

## 7. Timestamp column

- [x] 7.1 Show each row's local time in a right-aligned column that is not indented, and keep that time out of activity, open-session, and root detail
