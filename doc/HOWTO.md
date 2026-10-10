# Writing documentation

How the help content in `doc/` is organized and how to add or translate topics.

Help content is shipped with the app (embedded) and also readable on GitHub. Each language has its own complete folder; English is the default and the fallback.

## Structure

```
doc/
  <lang>/README.md                       app overview and main pages of the language
  <lang>/devices/README.md               list of supported devices
  <lang>/devices/<devicetype>/README.md  per device, folder = DeviceType name in lowercase
  <lang>/pages/<page-key>/README.md      per page, key = view model name without "PageViewModel", kebab-case
  _template/                             starting points for new topics (not embedded)
```

Only `README.md` files inside `devices/` and `pages/` folders are embedded into the app. Index files, this file and templates are not.

## Language lookup

The app looks for a topic in the folder of the UI culture (e.g. `de-AT`), then in the language folder (`de`), then in `en`.
A topic missing in a language is therefore shown in English. A translation uses the same relative path as the English topic.

## Conventions

- Plain Markdown only: headings, paragraphs, lists, bold/italic, links, tables.
- The first line is a level-1 heading, used as the help page title.
- Cross-links use relative paths, e.g. `[Controller profile](../controller-profile/README.md)`.
- App images are referenced from `BrickController2/UI/Images` using relative paths (`../../../../BrickController2/BrickController2/UI/Images/<name>.png` from a device topic). Only `.png` files from that folder are inlined in the app. Images in other locations (e.g. next to the topic) are not embedded and do not show in the app help.

## Adding a topic

1. Copy `_template/device/README.md` or `_template/page/README.md` to the right folder under `en/`.
2. Fill in the content; verify facts against the code.
3. Add a device topic to `en/devices/README.md`. Add a main page to `en/README.md`.
4. To translate, create the same path under `de/` or `hu/` and add it to that language index.

Authoring rules are in `.github/instructions/documentation.instructions.md`.
