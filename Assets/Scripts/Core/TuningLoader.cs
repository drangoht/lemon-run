using System.IO;
using LemonRun.Rules;
using UnityEngine;

namespace LemonRun
{
    /// <summary>
    /// Reads the tuning from a <c>tuning.json</c> sitting next to the executable, and writes it
    /// with its defaults when the file is missing.
    /// </summary>
    /// <remarks>
    /// The point is the project convention: a value that deserves to be tried several times must
    /// be changeable <b>without recompiling</b>. Shipping the defaults into the file on the first
    /// run matters as much as reading it back -- a knob nobody knows exists is a knob nobody turns.
    ///
    /// WARNING: the file sits beside the binary, inside <c>Build/Windows/</c>, which the build
    /// overwrites. It is a scratch pad for a play session, not a place to keep a setting: what is
    /// worth keeping goes back into <see cref="RunnerTuning"/>, in the commit that measured it.
    /// </remarks>
    public static class TuningLoader
    {
        public const string FileName = "tuning.json";

        public static string FilePath
        {
            get
            {
                var root = Directory.GetParent(Application.dataPath);
                return Path.Combine(root != null ? root.FullName : Application.dataPath, FileName);
            }
        }

        public static RunnerTuning Load()
        {
            var tuning = new RunnerTuning();

            // WebGL has no writable file system beside the "executable": a web build ships with
            // whatever defaults were committed, and that is the intended behaviour.
            if (Application.platform == RuntimePlatform.WebGLPlayer) return tuning;

            try
            {
                if (File.Exists(FilePath))
                {
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(FilePath), tuning);
                    Debug.Log("Tuning read from " + FilePath);
                }
                else
                {
                    File.WriteAllText(FilePath, JsonUtility.ToJson(tuning, true));
                    Debug.Log("Tuning written with its defaults: " + FilePath);
                }
            }
            catch (IOException error)
            {
                Debug.LogWarning("Tuning unusable (" + error.Message + "): defaults kept.");
            }

            return tuning;
        }
    }
}
