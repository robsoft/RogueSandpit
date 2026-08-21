# Fullscreen and real-time launch options — brief spec

- `--fullscreen` requests borderless fullscreen at the current desktop resolution while preserving the native 800×600 aspect ratio.
- If absent, the existing integer `--scale` window sizing remains unchanged; fullscreen takes precedence over initial scale dimensions.
- `--realtime` starts timed-turn mode enabled. F12 still toggles it normally.
- Both flags compose with each other and with `--turn-seconds`.

