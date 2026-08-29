# Fonts shipped with K-BOT

Drop `Inter-Regular.ttf` and `Inter-SemiBold.ttf` here.

Everything in this folder with a `.ttf` or `.otf` extension is copied next to the executable
and registered at startup by `FontLoader` (`KBot.Theming`), on **both** GDI paths — the
`PrivateFontCollection` that `Graphics.DrawString` resolves against, and
`AddFontMemResourceEx` for `TextRenderer.DrawText`. Registering only one leaves roughly half
the application silently on the fallback font.

The `Modern` scheme names `Inter`. Inter is under the SIL Open Font License, so it may be
redistributed with the application, and it carries `ș` and `ț` with a comma below rather than
a cedilla — which is what Romanian actually wants.

An empty folder is not an error. `FontLoader` logs through `GlobalErrorLog` and the scheme
falls back to whatever GDI picks; the application starts normally either way.

Source: https://github.com/rsms/inter/releases (the static `.ttf` files, not the variable font).
