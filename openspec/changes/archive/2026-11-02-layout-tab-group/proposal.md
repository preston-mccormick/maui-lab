# Proposal

## Why

Grid Layout is the only layout you can play with, and it lives under Experiments next to unrelated pages. The lab should treat layouts as one demo: a Layouts group whose pages share a playground, show what each layout can do, and record what was tried.

## What Changes

- Add a flyout group titled Layouts. Move Grid Layout into it and add Stack, Absolute, Flex, Composite, Dock, and Radial as further tabs. Scrolling Layout stays under Experiments. A Collections group stays out of this change. Flex and Uniform may appear there later for a large number of items; this change uses them only as the structure of a small control.
- Give every page the same playground: the layout fills the page, and a docked panel holds collapsible sections for the layout's settings and the selected item. The panel is the toolkit dock layout, docked to the bottom below 800 points and to the right at 800 and above.
- Extend Grid Layout with real children that can be placed in cells. Stack, Absolute, and Flex pages add items (shapes, images, and a small form panel) and expose that layout's settings. Absolute also shows rotation, opacity, and clipping, which do not change layout bounds.
- Add a Composite page that lays one field (label, entry, optional description, optional validation) out three ways: Flex, Uniform, and the existing form-columns layout. The Flex page stays the place to play with Flex as an engine. Add a Dock Layout page on the toolkit dock layout: edge per item (left, top, right, bottom, or none), spacing, and whether the last item fills. Add Radial as the one custom layout, placing children on an arc.
- Add an info control: a circled "i" that shows a tip or a note when tapped. A page setting chooses toast, snackbar, or the kind's own channel.
- Record one usage event per completed gesture on these pages, as a verb and a noun, and show it on the usage timeline. Skip selection, collapsing, and in-progress drags and keystrokes.

## Capabilities

### New Capabilities

- `layout-playground`: The Layouts group, the shared playground, the seven layout pages, the composite field, the toolkit dock page, the radial layout, and the info control.

### Modified Capabilities

- `usage-log`: A layout page records one action per completed gesture, with a verb, a noun, and a short detail, instead of the three-row menu and dialog shape.
- `usage-timeline`: That action appears as one row under the page that was open.

## Impact

- `MauiLab/AppShell.xaml`: a new Layouts `FlyoutItem`; Grid Layout leaves Experiments. The seven pages are top tabs in one `Tab`. Page titles stay as they are.
- `MauiLab/Layouts` owns the pages, the playground, `RadialLayout`, the info control, and the revived `LayoutLevers`. The Composite page uses the existing `FormColumnsLayout` and the toolkit `UniformItemsLayout`. No new NuGet package. The playground and the Dock page use `CommunityToolkit.Maui.Layouts.DockLayout`, already in the referenced toolkit.
- `MauiLab/Usage`: one new event kind and a text column added the same way Width, Height, Mode, and ParentId were. Existing rows are not rewritten.
- `MauiLabTests` gains headless tests for the page models, the radial placement, and the new usage event. The toolkit's own tests cover dock arrangement.
