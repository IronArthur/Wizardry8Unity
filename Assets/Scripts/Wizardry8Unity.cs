#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WizardryConnect.Utility;

namespace WizardryConnect
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif

    public class Wizardry8Unity : MonoBehaviour
    {
        #region Fields

        bool isReady = false;
        bool isPathValidated = false;
        ContentReader reader;

        #endregion


        #region Public Fields

        // General
        public string Wizardry8Path;
        public string Item3DImporter_ModelName = "BOOKFOUR.ITM";
        public int Item3DImporter_ModelNameID = 0;

        #endregion

        #region Class Properties

        public bool IsReady
        {
            get { return isReady; }
        }

        public bool IsPathValidated
        {
            get { return isPathValidated; }
        }

        public ContentReader ContentReader
        {
            get { return reader; }
        }

        #endregion

        #region Singleton

        static Wizardry8Unity instance = null;
        public static Wizardry8Unity Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!FindWizardry8Unity(out instance))
                    {
                        GameObject go = new GameObject();
                        go.name = "Wizardry8Unity";
                        instance = go.AddComponent<Wizardry8Unity>();
                    }
                }
                return instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return (instance != null);
            }
        }

        #endregion

        #region Unity

        void Awake()
        {
            instance = null;
            SetupSingleton();
            SetupW8Path();
            SetupContentReaders();
        }

        void Start()
        {

        }

        void Update()
        {
#if UNITY_EDITOR
            // Check ready every update in editor as code changes can de-instantiate local objects
            if (!isReady) SetupW8Path();
            if (reader == null) SetupContentReaders();
#endif
        }

        #endregion

        #region Editor-Only Methods

#if UNITY_EDITOR
        public void EditorUpdate()
        {
            // Try to get ready
            if (!isReady)
            {
                SetupSingleton();
                SetupW8Path();
                SetupContentReaders();
            }

            // Check content ready every update in editor as code changes can de-instantiate local objects
            if (reader == null)
            {
                SetupContentReaders();
            }
        }

        /// <summary>
        /// Setup path and content readers again.
        /// Used by editor when setting new W8Path.
        /// </summary>
        public void EditorResetW8Path()
        {
           // Settings.LoadSettings();
            SetupW8Path();
            SetupContentReaders(true);
        }

        /// <summary>
        /// Clear W8 path in editor.
        /// Used when you wish to decouple from W8 for certain builds.
        /// </summary>
        public void EditorClearW8Path()
        {
            Wizardry8Path = string.Empty;
            EditorResetW8Path();
        }
#endif

        #endregion

        #region Startup and Shutdown

        /// <summary>
        /// Sets new W8 path and sets up Wizardry8Unity.
        /// </summary>
        /// <param name="W8Path">New W8 path. Must be valid.</param>
        public void ChangeW8Path(string W8Path)
        {
            Wizardry8Path = W8Path;
            SetupW8Path();
            SetupContentReaders(true);
        }

        private void SetupW8Path()
        {
            // Clear path validated flag
            isPathValidated = false;

#if !UNITY_EDITOR
            // When starting a build, always clear stored path
            if (Application.isPlaying)
            {
                W8Path = string.Empty;
            }
#endif

            // Allow implementor to set own W8 path (e.g. from custom settings file)
            RaiseOnSetW8SourceEvent();

#if UNITY_EDITOR
            // Check editor singleton path is valid
            if (ValidateW8Path(Wizardry8Path))
            {
                isReady = true;
                isPathValidated = true;
                LogMessage("W8 path validated.", true);
                return;
            }
#endif


            bool found = false;
            string path = TestW8Exists("W8");
            if (!string.IsNullOrEmpty(path))
            {
                LogMessage("Trying Wizardry8 path " + path, true);
                if (Directory.Exists(path))
                    found = true;
                else
                    LogMessage("Wizardry8 path not found.", true);
            }

            // Otherwise, look for W8 folder in Application.dataPath at runtime
            if (Application.isPlaying && !found)
            {
                path = TestW8Exists(Application.dataPath);
                if (!string.IsNullOrEmpty(path))
                    found = true;
            }

            // Did we find a path?
            if (found)
            {
                // If it appears valid set this is as our path
                LogMessage(string.Format("Testing W8 path at '{0}'.", path), true);
                if (ValidateW8Path(path))
                {
                    Wizardry8Path = path;
                    isReady = true;
                    isPathValidated = true;
                    LogMessage(string.Format("Found valid W8 path at '{0}'.", path), true);
                    //Generate log file
                   // GenerateDiagLog.PrintInfo(Settings.MyWizardry8Path);
                    return;
                }
            } else
            {
                LogMessage(string.Format("Could not find W8 path. Try setting MyWizardry8Path in settings.ini."), true);
            }

            // No path was found but we can try to carry on without one
            // Many features will not work without a valid path
            isReady = true;

            // Singleton is now ready
            RaiseOnReadyEvent();
        }

        private void SetupContentReaders(bool force = false)
        {
            if (reader == null || force)
            {
                // Ensure content readers available even when path not valid
                if (isPathValidated)
                {
                    Wizardry8Unity.LogMessage(string.Format("Setting up content readers with W8 path '{0}'.", Wizardry8Path));
                    reader = new ContentReader(Wizardry8Path);
                } else
                {
                    Wizardry8Unity.LogMessage(string.Format("Setting up content readers without W8 path. Not all features will be available."));
                    reader = new ContentReader(string.Empty);
                }
            }
        }

        #endregion

        #region Public Static Methods

        public static void LogMessage(string message, bool showInEditor = false)
        {
            if (showInEditor || Application.isPlaying) Debug.Log(string.Format("DFTFU {0}: {1}", VersionInfo.Wizardry8UnityVersion, message));
        }

        public static bool FindWizardry8Unity(out Wizardry8Unity dfUnityOut)
        {
            dfUnityOut = GameObject.FindObjectOfType(typeof(Wizardry8Unity)) as Wizardry8Unity;
            if (dfUnityOut == null)
            {
                LogMessage("Could not locate Wizardry8Unity GameObject instance in scene!", true);
                return false;
            }

            return true;
        }

        public static string TestW8Exists(string parent)
        {
            // Accept either upper or lower case
            string pathLower = Path.Combine(parent, "w8");
            string pathUpper = Path.Combine(parent, "W8");

            if (Directory.Exists(pathLower))
                return pathLower;
            else if (Directory.Exists(pathUpper))
                return pathUpper;
            else
                return string.Empty;
        }

        public static bool ValidateW8Path(string path)
        {
            W8Validator.ValidationResults results;
            W8Validator.ValidateW8Folder(path, out results);

            return results.AppearsValid;
        }

        #endregion

        #region Private Methods

        private void SetupSingleton()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                if (Application.isPlaying)
                {
                    LogMessage("Multiple Wizardry8Unity instances detected in scene!", true);
                    Destroy(gameObject);
                }
            }
        }

        #endregion

        #region Event Handlers
        // OnReady
        public delegate void OnReadyEventHandler();
        public static event OnReadyEventHandler OnReady;
        protected virtual void RaiseOnReadyEvent()
        {
            if (OnReady != null)
                OnReady();
        }

        // OnSetW8Source
        public delegate void OnSetW8SourceEventHandler();
        public static event OnSetW8SourceEventHandler OnSetW8Source;
        protected virtual void RaiseOnSetW8SourceEvent()
        {
            if (OnSetW8Source != null)
                OnSetW8Source();
        }
        #endregion
    }
}

