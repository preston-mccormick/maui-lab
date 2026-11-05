# Collection lessons

## One busy UI thread stalls the whole app

FlexLayout and UniformItemsLayout create a view for every item. The old ListView did too when it retained cells, and even recycling a 949-row list was enough to stall this app. A few thousand views on the UI thread make the beachball, the splitter, the pickers, and every other page wait. Flex and Uniform therefore stop at the XKCD survey (949). Both CollectionView pages keep the 3,099-name catalog, because they virtualize. The useful ceiling for Flex and Uniform here is a few hundred items.

Batching 40 items at a time, and waiting until the picker closes, keeps the spinning cursor away and lets the scrollbar move. It does not free the UI thread. On FlexView it also more than doubles the build, because each batch lays the growing list out again. Measured with timings on: the named colors (148) took 2.3 s in one step and 6.6 s in batches, and the XKCD survey (949) took 6.4 s and 14.2 s. The Load in batches switch exists so those two costs can be compared. Prefer batching when someone has to watch the load. Prefer the one-step replace when the only question is how long the work takes.

`Dispatcher.DispatchAsync` from the UI thread runs the work immediately, so a batch loop that uses it never yields. A later turn has to be scheduled with `DispatchDelayed`.

## Uniform Items keeps the first cell

The toolkit measures the first child once and stamps that size on every cell. A later card's own width and height are ignored for placement. On iOS, `IsClippedToBounds` on the cell and on the layout does not keep a larger card inside that cell. The card paints outside it and overlaps its neighbors. That overlap is worth keeping: a short first item lets later cards spill, and a large first item lets later cards sit inside the cell and look smaller. Do not add a clip geometry to force the cut.

## SwipeView follows a mouse, and the swipe must cross the scroll

`SwipeView` is on the item, not on the CollectionView. A swipe along the scroll fights the list, so a vertical list swipes sideways and a horizontal list swipes up and down. Apple's list guidance puts non-destructive actions on the leading edge and Delete on the trailing edge. That split is what people expect on iOS and on Android, so this page uses it on both: Pin and Copy on the left, Delete on the right. A horizontal list has no platform habit for top versus bottom, so Delete stays the far gesture and is a swipe up. Pin and Copy are the swipe down. A mouse drag on Mac Catalyst reveals those same edges. `ListView` context actions did not. The secondary-click menu remains another way in. Replacing that menu while it is closing can leave the card hidden. Change the menu after the click has finished, and set the card's opacity back to 1.

## Shell tab text and page title are different strings

`ShellContent.Title` is the tab label. `Page.Title` is the page title the usage log records. They can differ. Shortening the tab label does not increase the tab font. Shell has no property for that size.
