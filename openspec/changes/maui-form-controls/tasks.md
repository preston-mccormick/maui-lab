# Tasks

## 1. Logic and tests

- [x] 1.1 Add `FormColumnCount.ForWidth` per design.md and `FormColumnCountTests` for 639, 640, 959, 960, a non-finite width, 0, and a negative width. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter FormColumnCountTests` passes.
- [x] 1.2 Add `FormControlsViewModel` with every bound property from the design's sample table, `PressCount`, and `PressCommand`. Add `FormSamples.Create()` with the five samples and `FormSamplesTests` covering the sample order, Empty nulls, Invalid values, Valid values, Long slider and ratings, and that `PressCommand` increments only its own form. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter FormSamplesTests` passes.
- [x] 1.3 Add `FormGallery` with `Samples`, `Current`, `CurrentIndex`, `Position`, next and previous commands that wrap, and the two expander states (Entry true, Editor false). Add `FormGalleryTests` for opening on Empty with "1 of 5 · Empty", next from Invalid to Empty, previous from Empty to Invalid with "5 of 5 · Invalid", an edit on Short surviving a round trip, and expander state unchanged by cycling. Verify `dotnet test MauiLabTests/MauiLabTests.csproj --filter FormGalleryTests` passes.

## 2. Page and flyout

- [x] 2.1 Add `MauiLab/FormControls/FormControlsPage` titled Form Controls with a two-row grid: the sample bar (previous, position label, next, bound to the gallery) above a vertical `ScrollView` whose content binds its `BindingContext` to `Current`. Set the gallery as the page's `BindingContext` in the constructor. Add a top-level `ShellContent` titled Form Controls between Colors and the Experiments `FlyoutItem`. Verify the item is a direct child of `Shell`, the page title is Form Controls, and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 3. Groups and columns

- [x] 3.1 Add the groups in the spec order. Entry and Editor are expanders whose `IsExpanded` binds two-way to the gallery, with a Medium heading and a disclosure mark. Every other group is a `panel` border with the margin override from design.md and the same heading. Each group gets a `FormColumnsLayout`, whose manager picks the column count from `FormColumnCount.ForWidth` of its width constraint and sizes each row to its tallest child. Verify the order, the two initial expander states, and that `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 4. Entry

- [x] 4.1 Add the Entry variants bound to the form: placeholder text, password with clear-while-editing, read-only, disabled, and the phone mask `(XXX) XXX-XXXX`. Verify each variant has a 12-point caption and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.
- [x] 4.2 Add the validating Entry fields from design.md, bound to the form, using `ValidateOnAttaching` and `ValidateOnValueChanged`, both valid and invalid styles, and a message whose visibility binds to the behavior's `IsNotValid` by `x:Reference`. The combined field uses `^[A-Za-z]*$`. Verify the messages shown for the Empty, Invalid, and Valid samples match the form-controls spec and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 5. Remaining controls

- [x] 5.1 Add Editor, SearchBar, Picker, DatePicker, and TimePicker variants bound to the form, including the constrained date's `MaximumDate` bound to `Today`. Verify the placeholders, formats, disabled fields, the "Choose one" title on Empty, and that `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.
- [x] 5.2 Add CheckBox, Switch, RadioButton, Slider, Stepper, and Button variants bound to the form. `RadioButtonGroup.SelectedValue` binds two-way to the radio choice. Slider, stepper, and press-count captions bind to the live values. Text and image buttons bind `PressCommand`; the image is `chemistry_flask.png`. Verify the Valid sample's box and switch states, the disabled controls, and that `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.
- [x] 5.3 Add the RatingView group bound to the form: star, heart, and read-only, each with five shapes, using `Primary` and `Gray`. Verify the Long sample shows 5 on all three, the read-only rating sets `IsReadOnly`, and `dotnet build MauiLab/MauiLab.csproj -f net11.0` succeeds.

## 6. Integration

- [ ] 6.1 Build for Mac Catalyst and run the app. Verify the bar stays visible while scrolling, cycling through all five samples updates every group, and `dotnet test MauiLabTests/MauiLabTests.csproj` passes.
