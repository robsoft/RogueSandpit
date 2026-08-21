# Prototype sprite atlas plan

1. Correct the atlas map in the presentation roadmap and retain only editable sources under `ArtSource/`.
2. Add the exported PNG to `Content.mgcb`.
3. Add a small named atlas-region abstraction and load the texture once.
4. Update `MapRenderer` to layer atlas terrain, features, effects, items, and actors while preserving primitive fallbacks.
5. Begin the map sprite batch with point sampling.
6. Add focused tests for atlas-region coordinates where practical.
7. Build, run the full test suite, and smoke-test the application at the default scale.
