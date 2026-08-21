# Fullscreen and real-time launch options — brief plan

1. Parse independent fullscreen and real-time flags in `GameOptions`.
2. Pass launch mode into `GameWrapper` and the real-time timer.
3. Configure borderless desktop fullscreen with existing aspect-ratio rendering.
4. Test parsing/composition, update launch docs, and smoke-test both flags together.

