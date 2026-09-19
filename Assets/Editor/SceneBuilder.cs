using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using LemonRun.Gameplay;
using LemonRun.Rules;

namespace LemonRun.EditorTools
{
    /// <summary>
    /// Builds the game scene <b>entirely in code</b>, then writes it to disk.
    ///
    /// <para>This is the structuring choice of the template: the scene is an <b>artefact</b>,
    /// regenerated at every build, and not a file edited with the mouse. In exchange, the whole
    /// game can be driven without ever opening the editor -- an agent can change a position, run
    /// the build again in batchmode and look at the result, which a hand-edited <c>.unity</c> file
    /// makes impossible.</para>
    /// </summary>
    /// <remarks>
    /// WARNING: consequence to know about -- <c>Assets/Scenes/Game.unity</c> comes out <b>modified
    /// after every build</b>, because the regeneration renumbers every <c>fileID</c> -- thousands
    /// of diff lines for an identical scene. Discard it (<c>git checkout --</c>) unless
    /// <c>SceneBuilder.cs</c> has changed, in which case the regeneration carries a real
    /// difference. <c>BuildTools.HasLocalChanges</c> already excludes it from the tree cleanliness
    /// check.
    ///
    /// WARNING: do not add anything here that depends on a missing asset: a batchmode build fails
    /// on a null reference without any way to see it in the editor.
    /// </remarks>
    public static class SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("Lemon Run/Regenerate the scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var runnerGo = BuildRunner();
            var runner = runnerGo.GetComponent<Runner>();

            BuildCamera(runnerGo.transform);
            BuildSun();
            BuildRoad(runnerGo.transform);

            var pursuer = BuildPursuer(runner);
            BuildObstacles(runner, pursuer);

            var session = new GameObject("Run Session").AddComponent<RunSession>();
            session.Runner = runner;
            session.Pursuer = pursuer;

            BuildEventSystem();
            BuildStampCanvas(runner, pursuer, session);

            // ---- The game continues here ---------------------------------------------------
            // Add your own building methods (obstacles, fruit, the pursuer, HUD, menus, ...).
            // Keep them short and named after what they place: this list is what gets re-read to
            // know what the scene is made of.
            // ---------------------------------------------------------------------------------

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Scene regenerated: {ScenePath}");
        }

        /// <summary>
        /// Perspective camera placed behind and above the runner: the "seen from behind" framing
        /// the GDD settled on. The values are provisional -- they are set for real once there is
        /// something to follow.
        /// </summary>
        static void BuildCamera(Transform target)
        {
            var go = new GameObject("Main Camera");
            var camera = go.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.10f, 0.18f);
            camera.transform.rotation = Quaternion.Euler(16f, 0f, 0f);
            go.tag = "MainCamera";

            var rig = go.AddComponent<CameraRig>();
            rig.Target = target;
            camera.transform.position = target.position + rig.Offset;

            // The camera's URP data is a separate component: without it, the camera falls back on
            // default values and ignores the renderer.
            go.AddComponent<UniversalAdditionalCameraData>();
        }

        /// <summary>
        /// WARNING: replaces the template's global <c>Light2D</c>, which the Universal (3D)
        /// Renderer ignores completely -- leaving it in place would have lit nothing while looking
        /// present in the scene. A lit mesh with no directional light comes out near-black, with
        /// nothing in the console.
        /// </summary>
        /// <remarks>
        /// The ambient light is set explicitly: with <c>SolidColor</c> clear flags there is no
        /// skybox to light the scene, and the default ambient mode then samples one that does not
        /// exist -- faces turned away from the sun fall to pure black.
        /// </remarks>
        static void BuildSun()
        {
            var go = new GameObject("Sun");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.24f, 0.25f, 0.34f);
        }

        const int TileCount = 8;
        const float TileLength = 40f;
        const float DashSpacing = 5f;
        const float DashLength = 2.2f;

        /// <summary>The runner itself -- a plain cube until there is a lemon to put here.</summary>
        static GameObject BuildRunner()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Runner";
            go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            go.transform.position = new Vector3(0f, 0.4f, 0f);
            Paint(go, Lit(new Color(0.96f, 0.82f, 0.18f)));

            // The collider comes free with the primitive and nothing reads it yet. Left in place,
            // it would silently start catching things the day an obstacle arrives.
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());

            go.AddComponent<Runner>();
            return go;
        }

        /// <summary>
        /// The road: <see cref="TileCount"/> tiles recycled in front of the runner, each carrying
        /// its own lane markings.
        /// </summary>
        /// <remarks>
        /// WARNING: the lane width is baked into this geometry at build time, while the runner
        /// reads it from the tuning at run time. Changing <c>LaneWidth</c> in <c>tuning.json</c>
        /// therefore slides the runner off the painted lanes, with nothing to warn about it: that
        /// one value needs a rebuild, not a tuning pass.
        ///
        /// The dashes are not decoration. On a plain uniform strip, forward motion is invisible:
        /// with nothing passing by, a runner at 12 units per second and one standing still look
        /// exactly the same.
        /// </remarks>
        static void BuildRoad(Transform target)
        {
            var tuning = new RunnerTuning();
            float laneWidth = tuning.LaneWidth;
            float roadWidth = Lanes.Count * laneWidth + 1f;

            var road = new GameObject("Road");
            var treadmill = road.AddComponent<GroundTreadmill>();
            treadmill.Target = target;
            treadmill.TileLength = TileLength;

            var asphalt = Lit(new Color(0.17f, 0.19f, 0.27f));
            var dash = Lit(new Color(0.75f, 0.78f, 0.86f));
            var edge = Lit(new Color(0.34f, 0.37f, 0.48f));

            for (int i = 0; i < TileCount; i++)
            {
                // One tile behind the start, so the road does not begin under the camera.
                var tile = new GameObject($"Tile {i}");
                tile.transform.SetParent(road.transform, false);
                tile.transform.position = new Vector3(0f, 0f, (i - 1) * TileLength);

                Slab(tile.transform, "Asphalt", new Vector3(0f, -0.5f, 0f),
                     new Vector3(roadWidth, 1f, TileLength), asphalt);

                for (int side = -1; side <= 1; side += 2)
                {
                    Slab(tile.transform, "Edge",
                         new Vector3(side * (roadWidth / 2f - 0.2f), 0.01f, 0f),
                         new Vector3(0.25f, 0.02f, TileLength), edge);

                    for (float z = -TileLength / 2f + DashSpacing / 2f; z < TileLength / 2f; z += DashSpacing)
                    {
                        Slab(tile.transform, "Dash",
                             new Vector3(side * laneWidth / 2f, 0.01f, z),
                             new Vector3(0.12f, 0.02f, DashLength), dash);
                    }
                }
            }
        }

        /// <summary>
        /// The obstacle field. It holds no obstacle: it lays them in front of the runner as the
        /// run goes, and takes them back behind.
        /// </summary>
        /// <remarks>
        /// The two materials are built here so that every URP material in the game comes from the
        /// same place (<see cref="Lit"/>), and are handed over rather than looked up at run time.
        ///
        /// Low obstacles are deliberately the colour of a warning and full ones that of a wall:
        /// the answer to each differs (jump or go round), so telling them apart at a distance is
        /// the whole readability of the road.
        /// </remarks>
        static void BuildObstacles(Runner runner, Pursuer pursuer)
        {
            var go = new GameObject("Obstacles");
            var field = go.AddComponent<ObstacleField>();
            field.Runner = runner;
            field.Pursuer = pursuer;
            field.LowMaterial = Lit(new Color(0.90f, 0.45f, 0.15f));
            field.FullMaterial = Lit(new Color(0.72f, 0.20f, 0.30f));

            // Pale green: the only thing on the road that is not a warning, and it must read as
            // such from far enough away to decide whether it is worth leaving the opening for.
            field.FruitMaterial = Lit(new Color(0.55f, 0.95f, 0.35f));
        }

        /// <summary>
        /// The pursuer: bigger than the runner, and darker than anything else on the road.
        /// </summary>
        /// <remarks>
        /// It is drawn inside a band that keeps it in frame whatever the lead (see
        /// <c>Lead.DrawGap</c>), so it must never be mistaken for a measurement: the bar at the
        /// top of the screen is what says how close it really is.
        /// </remarks>
        static Pursuer BuildPursuer(Runner runner)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Pursuer";
            go.transform.localScale = new Vector3(1.6f, 2.2f, 1.2f);
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            Paint(go, Lit(new Color(0.22f, 0.10f, 0.26f)));

            var pursuer = go.AddComponent<Pursuer>();
            pursuer.Runner = runner;
            return pursuer;
        }

        static void Slab(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            Paint(go, material);
        }

        /// <summary>A URP lit material, built from the shader <b>by name</b>.</summary>
        /// <remarks>
        /// WARNING: not left to the default of <c>CreatePrimitive</c>. A primitive carrying a
        /// Built-in pipeline material renders <b>magenta</b> under URP -- a defect that raises
        /// nothing at all, and that only a screenshot reveals.
        ///
        /// Materials created here are serialised INTO the scene, which is itself an artefact: no
        /// asset to manage, and nothing left behind when the scene is regenerated.
        /// </remarks>
        static Material Lit(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("URP Lit shader not found: everything would render magenta.");
                return null;
            }

            return new Material(shader) { color = color };
        }

        static void Paint(GameObject target, Material material)
        {
            if (material == null) return;
            target.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        /// <summary>
        /// WARNING: <c>InputSystemUIInputModule</c> and not <c>StandaloneInputModule</c> -- with
        /// the Input System package active, the old module receives nothing and the UI simply stops
        /// responding.
        /// </summary>
        static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        /// <summary>
        /// The build stamp lives on its <b>own</b> canvas rather than in the HUD: the HUD goes
        /// dark as soon as a menu opens, and menus are exactly where most screenshots are taken.
        /// </summary>
        static void BuildStampCanvas(Runner runner, Pursuer pursuer, RunSession session)
        {
            var canvasGo = new GameObject("Build Stamp Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var labelGo = new GameObject("Stamp");
            labelGo.transform.SetParent(canvasGo.transform, false);

            var text = labelGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.alignment = TextAnchor.LowerLeft;
            text.color = new Color(1f, 1f, 1f, 0.45f);
            text.raycastTarget = false;

            // Bottom LEFT, not bottom right: Unity draws its own "Development Build" watermark in
            // the bottom-right corner of every development build, and the two texts landed exactly
            // on top of each other -- neither of them readable on a capture.
            var rect = labelGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(12f, 8f);
            rect.sizeDelta = new Vector2(300f, 20f);

            labelGo.AddComponent<BuildStampLabel>();

            BuildDebugReadout(canvasGo.transform, runner, pursuer);
            BuildLeadGauge(canvasGo.transform, pursuer);
            session.EndPanel = BuildEndPanel(canvasGo.transform, session);
        }

        /// <summary>The lead bar, top centre -- the only honest reading of the pursuer's distance.</summary>
        static void BuildLeadGauge(Transform canvas, Pursuer pursuer)
        {
            var frame = new GameObject("Lead Gauge");
            frame.transform.SetParent(canvas, false);

            var background = frame.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.12f);
            background.raycastTarget = false;

            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 1f);
            frameRect.anchorMax = new Vector2(0.5f, 1f);
            frameRect.pivot = new Vector2(0.5f, 1f);
            frameRect.anchoredPosition = new Vector2(0f, -40f);
            frameRect.sizeDelta = new Vector2(420f, 16f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(frame.transform, false);

            var fillImage = fill.AddComponent<Image>();
            fillImage.raycastTarget = false;

            // Anchored left and stretched vertically: LeadGauge then drives anchorMax.x alone,
            // so the bar empties from the right without any size to recompute.
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var gauge = frame.AddComponent<LemonRun.UI.LeadGauge>();
            gauge.Pursuer = pursuer;
            gauge.Fill = fillRect;
            gauge.FillImage = fillImage;
        }

        /// <summary>
        /// The end-of-run panel. Built <b>inactive</b>: <c>RunOverLabel</c> fills itself in on
        /// being switched on, which is the moment the score exists.
        /// </summary>
        static GameObject BuildEndPanel(Transform canvas, RunSession session)
        {
            var panel = new GameObject("Run Over");
            panel.transform.SetParent(canvas, false);

            var dim = panel.AddComponent<Image>();
            dim.color = new Color(0.04f, 0.04f, 0.08f, 0.72f);
            dim.raycastTarget = false;

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(panel.transform, false);

            var text = labelGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 40;
            text.lineSpacing = 1.2f;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;

            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(700f, 260f);

            labelGo.AddComponent<LemonRun.UI.RunOverLabel>().Session = session;

            panel.SetActive(false);
            return panel;
        }

        /// <summary>
        /// DEBUG readout, top left. Temporary: it goes the day the real HUD arrives.
        /// </summary>
        /// <remarks>
        /// It rides the stamp canvas rather than the HUD for the same reason the stamp does: it
        /// must survive every screen, since the screens are where captures get taken.
        /// </remarks>
        static void BuildDebugReadout(Transform canvas, Runner runner, Pursuer pursuer)
        {
            var go = new GameObject("Run Debug");
            go.transform.SetParent(canvas, false);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.color = new Color(1f, 1f, 1f, 0.75f);
            text.raycastTarget = false;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(12f, -10f);
            rect.sizeDelta = new Vector2(900f, 24f);

            var readout = go.AddComponent<LemonRun.UI.RunDebugLabel>();
            readout.Runner = runner;
            readout.Pursuer = pursuer;
        }
    }
}
