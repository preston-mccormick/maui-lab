# Design

## Context

See proposal.md for why this change exists. The catalog, the samples, and the cycling behavior are the `form-controls` spec.

Shell items are `ShellContent` data templates. Experiments is the only `FlyoutItem`; Home, Colors, Geolocation, and Usage are top-level. Pages use a parameterless constructor and set their own `BindingContext` (Colors, Geolocation, Grid Layout). View models are `CommunityToolkit.Mvvm` `ObservableObject`s. `CommunityToolkit.Maui` 15.0.1 is already referenced and `UseMauiCommunityToolkit` runs on OS targets. Implicit styles in `Resources/Styles/Styles.xaml` already theme Entry, Editor, SearchBar, Picker, DatePicker, TimePicker, CheckBox, Switch, RadioButton, Slider, and Button. Stepper has no implicit style. `panel` is the existing bordered group style. There is no error color in `Colors.xaml`. In MAUI 11, `DatePicker.Date`, `MaximumDate`, and `TimePicker.Time` are nullable.

## Goals / Non-Goals

**Goals:**

- One scrollable page whose groups match the spec, using the toolkit types already in the project.
- Every value the form shows or edits is a property on a form view model, bound with compiled bindings. The page is a reference for a bound form.
- Several form view models with different data, and a pinned bar that swaps which one the form is bound to, so the rebind of the whole form can be watched.
- Pure column-count, sample, and cycling logic the test project can call without a device.
- Validation that fails on the samples the spec names, then clears as the text becomes valid.

**Non-Goals:**

- A measured rebind time on screen. The cost is the layout pass after the binding context changes, which a stopwatch around the property set would not include.
- Restyling the app's implicit control styles.
- `TableView`, `Frame`, `TabbedPage`, or a new package for in-page tabs.
- Toolkit pieces that are not form controls or are already shown elsewhere: Popup (Dialogs), AvatarView, DrawingView, DockLayout, StateContainer, SemanticOrderView.
- Toolkit behaviors that do not change what the field displays: `SetFocusOnEntryCompletedBehavior`, `UserStoppedTypingBehavior`, `MaxLengthReachedBehavior`.
- Persisting edits. Samples are rebuilt each time the page is constructed.

## Decisions

### Top-level shell item, after Colors

`AppShell` gets a `ShellContent` titled Form Controls between Colors and the Experiments `FlyoutItem`. Everything lives in `MauiLab/FormControls`, namespace `MauiLab.FormControls`, the same folder-per-screen pattern as Colors and Dialogs.

### Three types: gallery, form, samples

- `FormControlsViewModel` is the form. One property per bound value, all `[ObservableProperty]`. It has a `PressCount` and a `PressCommand` that increments it, so the button group has something bound to show.
- `FormGallery` holds `Samples` (the five forms, in spec order), `Current`, `CurrentIndex`, `Position` (`"1 of 5 · Empty"`), `NextCommand`, `PreviousCommand`, and the two expander states `IsEntryExpanded` (true) and `IsEditorExpanded` (false). Next and previous wrap by modular index. Expander state lives here, not on the form, so cycling does not collapse a group.
- `FormSamples.Create()` builds the five forms. Each has a `Name`: Empty, Short, Long, Valid, Invalid.

The page's `BindingContext` is the gallery. The pinned bar binds to the gallery. The scrolling form is one `VerticalStackLayout` whose `BindingContext` is bound to `Current`, so swapping `Current` rebinds the whole form in one assignment. That assignment is the rebind under test. Expander `IsExpanded` binds back up to the gallery through the page: `{Binding IsEntryExpanded, Source={RelativeSource AncestorType={x:Type FormGallery}}}`.

The gallery is constructed in the page constructor, like Colors, and not registered in DI. It has no services.

### Sample data

Dates and times are nullable so Empty can leave them blank. Text is `string?`. The Long text is a fixed ~300-character paragraph constant.

| Field | Empty | Short | Long | Valid | Invalid |
|---|---|---|---|---|---|
| Name, read-only, disabled, notes, growing notes, disabled notes, search, disabled search | null | 2–3 characters | the long paragraph | a sentence | a sentence |
| Password | null | `pw` | long | `correct horse` | `x` |
| Required | null | `a` | long | `Present` | `"   "` |
| Email | null | `a@b.co` | long local part `@example.com` | `ada@example.com` | `not-an-email` |
| Number | null | `5` | `10` | `7` | `0` |
| Exact | null | `yes` | `yes` | `yes` | `Yes` |
| Digit | null | `a1` | long with digits | `abc123` | `abcdef` |
| URL | null | `https://a.co` | long path on `https://example.com/` | `https://example.com` | `not a url` |
| Letters | null | `ab` | long letters only | `letters` | `abc123` |
| Phone | null | `(555) 1` | `(555) 123-4567` | `(555) 123-4567` | `(555) 12` |
| Choice | null | Red | Blue | Green | null |
| Selected choice, disabled choice | null | Red | Blue | Green | Red |
| Date, constrained date, disabled date | null | today − 1 | today − 3650 | today − 30 | today − 7 |
| Time 12, time 24, disabled time | null | 09:05 | 23:59 | 14:30 | 00:00 |
| Box 1, box 2, disabled box | false, false, false | true, false, false | true, true, true | false, true, true | false, false, true |
| Switch 1, switch 2, disabled switch | same pattern as boxes | | | | |
| Radio choice | null | One | Two | Two | One |
| Slider, disabled slider | 0, 0 | 10, 10 | 100, 100 | 40, 60 | 0, 100 |
| Stepper, disabled stepper | 0, 0 | 1, 1 | 10, 10 | 2, 5 | 0, 10 |
| Stars, hearts, read-only rating | 0 | 1 | 5 | 4, 3, 5 | 0, 0, 2 |
| Press count | 0 | 0 | 0 | 0 | 0 |

The constrained date's `MaximumDate` binds to `Today` on the form, set to `DateTime.Today` when the sample is created. Picker items Red, Green, Blue and radio values One, Two, Three are constants on the form.

### Border groups, Expander for Entry and Editor

Each control type is one full-width group in a vertical stack inside a `ScrollView`. The sample bar sits above the `ScrollView` in a two-row `Grid`, so it never scrolls.

Entry and Editor use `Expander`. The header is the control-type label (Medium) plus a disclosure mark. `IsExpanded` binds two-way to the gallery. `HandleHeaderTapped` stays at its default.

Every other group is a `Border` using `panel` for the stroke and corner, with margin overridden on the page so stacked groups are not separated by the style's 24-point margin. The heading inside the border is the same Medium label. These groups do not collapse.

The variant caption is a page style: 12-point gray text. Slider, stepper, and press-count captions bind to the form with `StringFormat`, so the caption and the live value are the same label.

### Equal columns from the variant area's width

`FormColumnCount.ForWidth` returns 1 when the width is missing, non-finite, or below 640; 2 from 640 up to but not including 960; 3 at 960 and wider. It references no MAUI types, so `MauiLabTests` (plain `net11.0`) can call it.

Each group's variants sit in a `FormColumnsLayout`, a small `Layout` in the FormControls folder. Its manager takes the column count from `ForWidth` of the width constraint it is measured with, so the spec measures the variant area, not the window, and no `SizeChanged` round trip is needed. Columns are equal width. Each row is as tall as its tallest child, so a validation message or a growing editor pushes the next row down.

The toolkit's `UniformItemsLayout` was tried first and rejected. Its manager measures only the first child, once, and gives every cell that size, so anything taller than the first variant overlaps the row below. A `FlexLayout` basis is harder to pin to 640 and 960. Rebuilding a `Grid` would mean reassigning every child's column when the count changes.

### Validation and the mask live on Entry

Flags on every validating behavior are `ValidateOnAttaching` and `ValidateOnValueChanged`. `ForceMakeValidWhenFocused` is left off so focusing a field does not hide the error.

The page defines an invalid style and a valid style. Invalid sets `TextColor` to `#B00020`. Valid restores the theme text color. The message under a field is red, 12 points, and binds `IsVisible` to that behavior's `IsNotValid` through `x:Reference`. That is the toolkit's documented pattern and does not depend on the behavior having a binding context. When `Current` changes, the field's `Text` changes, `ValidateOnValueChanged` re-runs, and the message follows. The combined field has two messages, one per child behavior.

| Field | Toolkit type | Rule |
|---|---|---|
| Required | `TextValidationBehavior` | `MinimumLength` 1, trim |
| Email | `EmailValidationBehavior` | toolkit email pattern; `Keyboard.Email` |
| Number | `NumericValidationBehavior` | minimum 1, maximum 10; `Keyboard.Numeric` |
| Exact | `RequiredStringValidationBehavior` | `RequiredString` `yes`, exact match |
| Digit | `CharactersValidationBehavior` | at least one `Digit` |
| URL | `UriValidationBehavior` | absolute URI |
| Letters | `MultiValidationBehavior` | minimum length 1, and `^[A-Za-z]*$`; the Empty sample's null text fails both rules, "abc123" fails only the letters rule |
| Phone | `MaskedBehavior` | mask `(XXX) XXX-XXXX`, unmasked character `X`; `Keyboard.Telephone` |

### The rest of the catalog

Captions name the variant. Values that differ from a control's default:

- Password entry: `IsPassword`, clear button while editing.
- Read-only entry: `IsReadOnly`. Disabled entry: `IsEnabled` false.
- Editor: placeholder "Notes"; the growing editor uses `AutoSize` `TextChanges`.
- SearchBar placeholder "Search".
- Open picker title "Choose one".
- Dates display format `d`. Times `h:mm tt` and `HH:mm`.
- Radio buttons share one `GroupName`; `RadioButtonGroup.SelectedValue` binds two-way to the form's radio choice on the containing layout. The third is disabled. They are one variant cell.
- Slider 0–100. Stepper 0–10, increment 1.
- Text button and image button bind `Command` to `PressCommand`. Image source is `chemistry_flask.png`, which the home page already uses. The disabled button has `IsEnabled` false and no command.
- RatingView: `MaximumRating` 5. Shapes Star and Heart. The third has `IsReadOnly` set. Fill and empty colors come from `Primary` and `Gray`.

### Code-behind stays small

The page constructs the gallery and sets it as `BindingContext`. Nothing else. No service registration. Shell creates the page with `ContentTemplate`, which needs the parameterless constructor the other pages already use.

## Risks / Trade-offs

- [A behavior's `IsValid` can be stale after a rebind if the new sample has the same text] → Same text means the same validity, so the stale value is also the correct one.
- [Two-way `RadioButtonGroup.SelectedValue` with a null sample value leaves every button unchecked] → That is the Empty sample's specified state.
- [Equal cells make a short checkbox as tall as its tallest sibling in that row] → Groups contain one control type, so the mismatch stays inside that type.
- [Clear button and the search cancel glyph differ by platform] → The spec requires clearable and query behavior, not a glyph.
- [Email and telephone keyboards do little on Mac Catalyst] → The caption names the keyboard. The visible contract is the validation message and the phone mask.
- [Expander inside a scroll view can measure its closed height late] → Editor starts collapsed and is not inside a `CollectionView`. The default header handler is the one the toolkit documents for scrollable parents.
- [An invalid style can stick after the value becomes valid] → Each behavior sets both `InvalidStyle` and `ValidStyle`.
- [Swapping `BindingContext` on a large form is the slow path being demonstrated] → That is the point of the bar. Nothing is virtualized on purpose.

## Migration Plan

No stored data. Rollback is removing the Form Controls `ShellContent`, the `MauiLab/FormControls` folder, and the FormControls tests.
