---
applyTo: "doc/**/*.md"
---

# Documentation instructions

Help content in `doc/` is embedded in the app and also read on GitHub. Keep it simple and accurate.

## Structure

- Device topics: `doc/devices/<devicetype>/README.md`; folder = DeviceType name in lowercase.
- Page topics: `doc/pages/<page-key>/README.md`; key = view model name without `PageViewModel`, kebab-case.
- Start new topics from `doc/_template/device/README.md` or `doc/_template/page/README.md`.
- Localized variants: `README.<culture>.md` next to the default file (fallback: culture, language, `README.md`).
- Register every new topic in the Topics list of `doc/README.md`.

## Format

- Plain Markdown only: headings, paragraphs, lists, bold/italic, links, tables. No HTML.
- First line is a single level-1 heading; it is the help page title.
- Cross-links use relative paths, e.g. `[Controller action](../../pages/controller-action/README.md)`.
- Images live in the app's `UI/Images` folder or an `images/` folder next to the topic; use relative paths and alt text.
- Use `-100 % to +100 %` style for ranges (space before `%`).
- Refer to UI elements in **bold**. Use present tense and second person, short sentences.

## Device topic section order

1. Title and short description (connection type, what it drives)
2. Image
3. `## Channels` (table: Channels, Purpose, Output)
4. `## Getting started` (numbered steps: power on, scan, add, assign channels)
5. Device-specific sections (sound, scripts, macros, etc.)
6. `## Settings` (name, effect, default)
7. `## Notes`
8. `## See also` (device-list, controller-action, vendor docs)

## Accuracy

- Verify channel counts, ranges, settings and defaults against the device implementation in code before documenting.
- Document only what the app does; do not describe vendor features the app does not support.
- Do not invent behavior; leave out unknown details.
