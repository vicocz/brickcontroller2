# Sequence editor

A sequence is a reusable series of control points. When played, the output value moves through the control points one after another, which lets you automate a motion such as opening a gate or driving a pattern.

## Sequence properties

- **Name** - tap the name at the top to rename the sequence.
- **Loop** - when enabled, the sequence restarts from the first control point after the last one finishes.
- **Interpolate** - when enabled, the value changes smoothly between control points. When disabled, the value jumps directly to the next control point's value.

## Control points

Each control point has:

- **Value (%)** - the output value, from -100 to 100 in steps of 5. Negative values drive the channel in the opposite direction.
- **Duration (ms)** - how long this control point lasts. Tap the duration to change it.

Use the floating ![](../../../icons/add.svg) button (**Add a new control point**) to add a control point. Use the ![](../../../icons/clear.svg) button on a row to delete it.

## Toolbar

- ![](../../../icons/share.svg) **Share** - share the sequence as text.
- ![](../../../icons/upload_file.svg) **Share the sequence as file** - share the sequence as a file.
- ![](../../../icons/copy.svg) **Copy the sequence to clipboard** (overflow menu) - copy the sequence to the clipboard.
- ![](../../../icons/file_upload.svg) **Export the sequence** (overflow menu) - export the sequence to a file.
- ![](../../../icons/done_outline.svg) **Apply changes** - save the sequence and close the editor.
- ![](../../../icons/help_outline.svg) **Help** (overflow menu) - show this page.
