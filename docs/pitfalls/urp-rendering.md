# Pitfalls -- Rendering (URP 3D)


**WARNING: Unity stores the active pipeline in `QualitySettings`, LEVEL BY LEVEL.** [inherited]
Filling in only `GraphicsSettings.defaultRenderPipeline` leaves the other levels on Built-in: the
game changes pipeline as soon as the player changes quality. `RenderPipelineSetup.Apply()` loops over
every level -- that is why.

**WARNING: under the 2D Renderer a mesh is simply NEVER DRAWN, and nothing says so.** The template
shipped with `Renderer2D`; Lemon Run is seen from behind, in perspective. Left on the 2D renderer,
the camera background fills the window and the scene reads as *empty* -- not as broken. No error, no
warning, nothing in the player log. `RenderPipelineSetup.EnsureUniversalRenderer()` therefore
asserts the Universal (3D) Renderer **at every build**, rather than trusting a setting made once by
hand which a fresh clone would not carry.

**WARNING: the renderer list has no public setter.** `m_RendererDataList` is reachable only through
`SerializedObject`. Assigning `GraphicsSettings.defaultRenderPipeline` is *not* enough: the pipeline
asset keeps pointing at whichever renderer it was built with.

**WARNING: with `SolidColor` clear flags there is no skybox -- but the default ambient mode samples
one anyway.** The faces turned away from the directional light then fall to pure black and read as a
hole in the geometry rather than as lighting. `SceneBuilder.BuildSun()` sets
`RenderSettings.ambientMode = Flat` and an explicit `ambientLight`.

**WARNING: a `Light2D` is ignored outright by the 3D renderer.** It stays visible in the hierarchy,
it looks like lighting, and it lights nothing. *Was true under the 2D Renderer, and no longer
applies here: a sprite with no global `Light2D` was rendered black -- the opposite failure.*

**Guarded against, not suffered here: `GameObject.CreatePrimitive` and the magenta material.** A
primitive carrying a Built-in pipeline material renders magenta under URP, with nothing logged.
`SceneBuilder.Paint()` builds the material from `Shader.Find("Universal Render Pipeline/Lit")`
instead of trusting the default, so the case has not been observed on this project -- the entry is
here to keep it that way.
