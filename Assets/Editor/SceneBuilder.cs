using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

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

            BuildCamera();
            BuildSun();
            BuildPipelineProbe();
            BuildEventSystem();
            BuildStampCanvas();

            // ---- The game starts here ------------------------------------------------------
            // Add your own building methods (ground, player, HUD, menus, ...). Keep them short and
            // named after what they place: this list is what gets re-read to know what the scene is
            // made of.
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
        static void BuildCamera()
        {
            var go = new GameObject("Main Camera");
            var camera = go.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.10f, 0.18f);
            camera.transform.position = new Vector3(0f, 4.5f, -7f);
            camera.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
            go.tag = "MainCamera";

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

        /// <summary>
        /// TEMPORARY -- a ground strip and a cube, only there to prove that the 3D pipeline really
        /// draws. Remove it the moment the real ground and runner exist.
        /// </summary>
        /// <remarks>
        /// WARNING: the material is built from the URP shader <b>by name</b> rather than left to
        /// the default of <c>CreatePrimitive</c>. A primitive carrying a Built-in pipeline
        /// material renders <b>magenta</b> under URP -- a defect that raises nothing and that only
        /// a screenshot reveals.
        /// </remarks>
        static void BuildPipelineProbe()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Probe Ground (temporary)";
            ground.transform.position = new Vector3(0f, -0.5f, 25f);
            ground.transform.localScale = new Vector3(9f, 1f, 80f);
            Paint(ground, new Color(0.18f, 0.20f, 0.28f));

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Probe Cube (temporary)";
            cube.transform.position = new Vector3(0f, 0.6f, 6f);
            cube.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            Paint(cube, new Color(0.96f, 0.82f, 0.18f));
        }

        static void Paint(GameObject target, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("URP Lit shader not found: the probe would render magenta.");
                return;
            }

            // A material created here is serialised INTO the scene -- no asset to manage, which
            // suits an object whose whole purpose is to be deleted.
            target.GetComponent<MeshRenderer>().sharedMaterial = new Material(shader) { color = color };
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
        static void BuildStampCanvas()
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
        }
    }
}
