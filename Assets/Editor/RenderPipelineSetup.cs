using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LemonRun.EditorTools
{
    /// <summary>
    /// Enables the Universal Render Pipeline on <b>every</b> quality level.
    ///
    /// <para>The Built-in Render Pipeline has been deprecated since Unity 6. Lemon Run renders
    /// through the <b>Universal (3D) Renderer</b>: the game is seen from behind, in perspective
    /// (GDD section 7), so the template's 2D Renderer no longer applies.</para>
    /// </summary>
    /// <remarks>
    /// WARNING: Unity stores the active pipeline in <c>QualitySettings</c> <b>level by level</b>:
    /// filling in only <c>GraphicsSettings.defaultRenderPipeline</c> leaves the other levels on
    /// Built-in, and the game switches pipeline as soon as the player changes quality -- with no
    /// error.
    ///
    /// WARNING: under the 2D Renderer, a mesh is simply <b>never drawn</b>, and nothing is
    /// logged: the game shows the camera background and reads as "empty scene" rather than as a
    /// rendering fault. That is why the renderer is asserted here at every build instead of being
    /// set once by hand.
    /// </remarks>
    public static class RenderPipelineSetup
    {
        public const string PipelineAssetPath = "Assets/Settings/UniversalRP.asset";
        public const string RendererAssetPath = "Assets/Settings/UniversalRenderer.asset";
        public const string GlobalSettingsPath = "Assets/Settings/UniversalRenderPipelineGlobalSettings.asset";

        [MenuItem("Lemon Run/Enable the URP pipeline")]
        public static void Apply()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                Debug.LogError("URP pipeline not found: " + PipelineAssetPath);
                return;
            }

            EnsureUniversalRenderer(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;

            int previousLevel = QualitySettings.GetQualityLevel();
            int levelCount = QualitySettings.names.Length;
            for (int level = 0; level < levelCount; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(previousLevel, false);

            // The global settings carry, among other things, the default volume profile. Without
            // this explicit assignment, the editor would fabricate one at the first launch.
            // UniversalRenderPipelineGlobalSettings is internal: we go through the base class.
            var globalSettings = AssetDatabase.LoadAssetAtPath<RenderPipelineGlobalSettings>(GlobalSettingsPath);
            if (globalSettings != null)
            {
                EditorGraphicsSettings.SetRenderPipelineGlobalSettingsAsset<UniversalRenderPipeline>(globalSettings);
            }
            else
            {
                Debug.LogWarning("URP global settings not found: " + GlobalSettingsPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"URP active on {levelCount} quality level(s): {PipelineAssetPath}");
        }

        /// <summary>
        /// Makes the pipeline render through the Universal (3D) Renderer, creating the asset on
        /// the first run.
        /// </summary>
        /// <remarks>
        /// The renderer list has no public setter: <c>SerializedObject</c> is the only supported
        /// way into <c>m_RendererDataList</c>. Doing it at every build rather than once by hand
        /// keeps the switch reproducible on a fresh clone, where <c>Assets/Settings</c> is all
        /// that survives.
        /// </remarks>
        static void EnsureUniversalRenderer(RenderPipelineAsset pipeline)
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererAssetPath);
                Debug.Log("Universal (3D) Renderer created: " + RendererAssetPath);
            }

            var serialized = new SerializedObject(pipeline);
            var list = serialized.FindProperty("m_RendererDataList");
            if (list == null)
            {
                Debug.LogError("m_RendererDataList not found on " + PipelineAssetPath);
                return;
            }

            if (list.arraySize != 1 ||
                list.GetArrayElementAtIndex(0).objectReferenceValue != renderer)
            {
                list.arraySize = 1;
                list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            }

            var defaultIndex = serialized.FindProperty("m_DefaultRendererIndex");
            if (defaultIndex != null) defaultIndex.intValue = 0;

            serialized.ApplyModifiedProperties();
        }
    }
}
