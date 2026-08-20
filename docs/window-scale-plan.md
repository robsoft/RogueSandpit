# Window scale option — brief plan

1. Add a framework-independent command-line options parser with a 2× default.
2. Pass the selected scale into `GameWrapper` and apply it only to the backbuffer/window.
3. Keep the native render target and all game/UI coordinates unchanged.
4. Add parser tests, document launch examples, and smoke-test with `--scale 1`.

