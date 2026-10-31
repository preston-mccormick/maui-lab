# Proposal

## Why

The lab already records how the app is used and shows that history on a timeline. This change keeps that baseline and adds the window area, meaningful page and popup rows, button-level actions on Menu Button and Dialogs, and a timestamp column.

## What Changes

- Record the product-usage log: sessions, page visits, foreground time, and local persistence.
- Record the window width and height when a session starts and when that size changes. Record Full screen, Split View, Slide Over, or Windowed only when the rectangle makes the mode clear.
- Ignore Community Toolkit `PopupPage` shell routes so a menu does not become a page visit named like `popupPage45`.
- Title a page row "Navigated to {name} page".
- On Menu Button, nest the button, then Menu, then the chosen item or Dismissed under the page. On Dialogs, nest the button, then Alert, Action sheet, or Prompt, then the result or Dismissed.
- Show each row's local time in a right-aligned column. Activity, open-session, and root details no longer repeat that time.
- Keep the newest-first timeline, live open-page duration, light and dark chrome, and confirmed history reset.

## Capabilities

### New Capabilities

- `usage-log`: Record install, session, page, background, foreground, window area, and button actions, including elapsed and active time, and persist them locally for a later upload.
- `usage-timeline`: Show that log as a themed, newest-first nested timeline, with labeled page rows, nested actions, a right-hand timestamp, a live open visit, and a confirmed history reset.

### Modified Capabilities

- None.

## Impact

- `MauiLab/Usage`, `App`, `AppShell`, `MauiProgram`, the Menu Button and Dialogs experiments, and `MauiLabTests`.
- Local store is SQLite (`sqlite-net-pcl`) at `usage.db` in app data. Existing files gain the new columns. No network upload is in scope.
