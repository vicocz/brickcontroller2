---
applyTo: "doc/**/*.md"
---

# Documentation instructions

Help content in `doc/` is embedded in the app and also read on GitHub. Keep it simple and accurate.

## Structure

- Each language has its own complete tree `doc/<lang>/` (`en` is the default and the fallback; `de`, `hu`).
- Device topics: `doc/<lang>/devices/<devicetype>/README.md`; folder = DeviceType name in lowercase.
- Page topics: `doc/<lang>/pages/<page-key>/README.md`; key = view model name without `PageViewModel`, kebab-case.
- Start new topics from `doc/_template/device/README.md` or `doc/_template/page/README.md` (templates are not embedded).
- Write new topics in `en` first. A translation uses the same relative path under `doc/<lang>/`; lookup falls back per topic (culture, language, `en`).
- Register every new topic in `doc/en/README.md` (and in the index of the translated language).

## Format

- Plain Markdown only: headings, paragraphs, lists, bold/italic, links, tables. No HTML.
- First line is a single level-1 heading; it is the help page title.
- Cross-links use relative paths, e.g. `[Controller action](../../pages/controller-action/README.md)`.
- Images of the app are referenced from the `UI/Images` folder with relative paths, e.g. `../../../../BrickController2/BrickController2/UI/Images/<name>.png` from a device topic; always add alt text. Only `.png` files from that folder are shown in the app help; images in other locations are not embedded. Check that the file exists.
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
