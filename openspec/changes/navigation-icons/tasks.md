# Tasks

## 1. Glyph files

- [x] 1.1 Add the twenty-three Material Symbols Rounded SVGs under `MauiLab/Resources/Images/nav`, at 24px with a solid black fill, and include that folder as `MauiImage` with `BaseSize="24,24"`. The names are `science`, `palette`, `contacts`, `dashboard`, `dataset`, `dynamic_form`, `location_on`, `timeline`, `grid_on`, `open_with`, `view_sidebar`, `more_horiz`, `view_agenda`, `flex_direction`, `layers`, `radar`, `grid_view`, `view_list`, `flex_wrap`, `view_module`, `edit_note`, `picture_in_picture`, and `more_vert`. Verify each file has a black fill, none use a hyphen, and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 2. Shell

- [x] 2.1 Set `FlyoutIcon` on Home, Colors, Contacts, Layouts, Collections, Forms, Geolocation, and Usage, and set `Icon` on every bottom tab and on the four More Layouts top tabs, using the names in the shell-navigation spec. Verify the group names `dashboard`, `dataset`, and `dynamic_form` are not used as a tab icon, and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 3. Colors

- [x] 3.1 Set `Shell.TabBarUnselectedColor` to Gray in light and LighterGray in dark, and set light-mode `Shell.UnselectedColor` to Gray. Leave `Shell.TabBarForegroundColor`, `Shell.TabBarTitleColor`, and `Shell.ForegroundColor` as they are. Verify those three setters are unchanged and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 4. Dark flyout

- [x] 4.1 Add a white copy of each flyout glyph, named `science_dark` and so on, and point `FlyoutIcon` at the black file in light mode and the white file in dark mode. Leave tab `Icon` values on the black files. Verify the white files use a white fill, the eight flyout rows use `AppThemeBinding`, and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.
