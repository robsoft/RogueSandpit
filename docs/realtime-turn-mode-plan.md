# Optional real-time turn mode — brief plan

1. Expose simulation turn count and add a framework-independent countdown model.
2. Toggle and advance the timer in the MonoGame update loop, submitting `Wait` on expiry.
3. Pause modal/unfocused states and render concise HUD feedback.
4. Add timer, turn-count, and command-line option tests plus runtime verification.

