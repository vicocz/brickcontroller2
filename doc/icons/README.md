# Help icons

SVG copies of the Material Icons (Outlined) glyphs used by the app (`MaterialIconsOutlined-Regular.otf`).

Sources:

- The font comes from [Google Fonts - Material Icons](https://fonts.google.com/icons?icon.set=Material+Icons) (Outlined style).
- The SVG files are the same glyphs taken from the [@material-design-icons/svg](https://www.npmjs.com/package/@material-design-icons/svg) package (`outlined/` folder), because the Google Fonts site can also download single icons as SVG but this package provides all of them under stable names.
- License: Apache License 2.0.

To add an icon, find it in the Material Icons set (Outlined), download its SVG (24 px) from Google Fonts or the package above, and save it as `<Icon value>.svg`.

The file name equals the `Icon` value used in XAML (e.g. `Icon="settings"` -> `settings.svg`).
Reference an icon from a page topic with a Markdown image pointing to `../../../icons/<name>.svg`.

| Icon value | Source icon |
|---|---|
| `copy` | `content_copy` |
| `paste` | `content_paste` |

All other icons use the same name as the source icon.
