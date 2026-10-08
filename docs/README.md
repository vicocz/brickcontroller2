# BrickController 2 documentation

Help content is shipped with the app (embedded) and also readable here.

## Structure

```
docs/
  devices/<devicetype>/help.md      per device, folder = DeviceType name in lowercase
  pages/<page-key>/help.md          per page, key = view model name without "PageViewModel", kebab-case
  _template/help.md                 starting point for new topics
```

Localized variants (not yet used) are placed next to the default file as `help.<culture>.md`, e.g. `help.de.md`.
The lookup falls back from `help.<culture>.md` to `help.<language>.md` to `help.md`.

## Conventions

- Plain Markdown only: headings, paragraphs, lists, bold/italic, links, tables.
- The first line is a level-1 heading, used as the help page title.
- Cross-links use relative paths, e.g. `[Controller profile](../controller-profile/help.md)`.
- Images are not used yet. When added, they go to an `images/` folder next to the topic file.

## Topics

### Devices

- [PfxBrick](devices/pfxbrick/help.md)

### Pages

- [Creations](pages/creation-list/help.md)
- [Creation](pages/creation/help.md)
- [Controller profile](pages/controller-profile/help.md)
- [Controller action](pages/controller-action/help.md)
