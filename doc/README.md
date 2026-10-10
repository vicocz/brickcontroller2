# BrickController 2 documentation

Help content is shipped with the app (embedded) and also readable here. Each language has its own complete folder; English is the default and the fallback.

## Languages

- [English](en/README.md)
- [Deutsch](de/README.md)
- [Magyar](hu/README.md)

## Structure

```
doc/
  <lang>/README.md                       index of the language
  <lang>/devices/<devicetype>/README.md  per device, folder = DeviceType name in lowercase
  <lang>/pages/<page-key>/README.md      per page, key = view model name without "PageViewModel", kebab-case
  _template/                             starting points for new topics (not embedded)
```

Only `README.md` files inside `devices/` and `pages/` folders are embedded into the app. Index files and templates are not.

## Language lookup

The app looks for a topic in the folder of the UI culture (e.g. `de-AT`), then in the language folder (`de`), then in `en`.
A topic missing in a language is therefore shown in English. A translation uses the same relative path as the English topic.

## Conventions

- Plain Markdown only: headings, paragraphs, lists, bold/italic, links, tables.
- The first line is a level-1 heading, used as the help page title.
- Cross-links use relative paths, e.g. `[Controller profile](../controller-profile/README.md)`.
- App images are referenced from `BrickController2/UI/Images` using relative paths (`../../../../BrickController2/BrickController2/UI/Images/<name>.png` from a device topic). Only `.png` files from that folder are inlined in the app. Images in other locations (e.g. next to the topic) are not embedded and do not show in the app help.

New topics start from `_template/device/README.md` or `_template/page/README.md`. Authoring rules are in `.github/instructions/documentation.instructions.md`.
