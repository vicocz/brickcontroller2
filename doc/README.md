# BrickController 2 documentation

Help content is shipped with the app (embedded) and also readable here.

## Structure

```
doc/
  devices/<devicetype>/README.md      per device, folder = DeviceType name in lowercase
  pages/<page-key>/README.md          per page, key = view model name without "PageViewModel", kebab-case
  _template/README.md                 starting point for new topics
```

Localized variants (not yet used) are placed next to the default file as `README.<culture>.md`, e.g. `README.de.md`.
The lookup falls back from `README.<culture>.md` to `README.<language>.md` to `README.md`.

## Conventions

- Plain Markdown only: headings, paragraphs, lists, bold/italic, links, tables.
- The first line is a level-1 heading, used as the help page title.
- Cross-links use relative paths, e.g. `[Controller profile](../controller-profile/README.md)`.
- Images are not used yet. When added, they go to an `images/` folder next to the topic file.

## Topics

### Devices

- [Circuit Cubes](devices/circuitcubes/README.md)
- [PFxBrick](devices/pfxbrick/README.md)

### Pages

- [Creations](pages/creation-list/README.md)
- [Creation](pages/creation/README.md)
- [Controller profile](pages/controller-profile/README.md)
- [Controller action](pages/controller-action/README.md)
- [Devices](pages/device-list/README.md)
- [Sequences](pages/sequence-list/README.md)
- [Sequence editor](pages/sequence-editor/README.md)
