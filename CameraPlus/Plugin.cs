using System;
using System.IO;
using System.Linq;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CameraPlus
{
	public class Plugin : IPlugin
	{
		public static readonly Ini Ini = new Ini(Path.Combine(Environment.CurrentDirectory, "DynamicCamera.cfg"));
		private CameraPlusBehaviour _cameraPlus;
		private bool _init;
		private FileSystemWatcher _iniWatcher;

		public string Name => "DynamicCamera";

		public string Version => "v1.5";

		public void OnApplicationStart()
		{
			if (_init) return;
			_init = true;
			SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
			if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "DynamicCamera.cfg")) || Ini.GetFullText().Contains(','))
			{
				Ini.WriteValue("fov", "90.0");
				Ini.WriteValue("positionSmooth", "10");
				Ini.WriteValue("rotationSmooth", "5");

				Ini.WriteValue("thirdPerson", "True");
                Ini.WriteValue("cameraPreview", "True");
                Ini.WriteValue("moveCameraInGame", "True");
                Ini.WriteValue("avoidWalls", "True");

                Ini.WriteValue("3rdPersonCameraDistance", "0.8");
                Ini.WriteValue("3rdPersonCameraUpperHeight", "1.6");
                Ini.WriteValue("3rdPersonCameraLowerHeight", "0.4");
                Ini.WriteValue("3rdPersonCameraLateralNear", "0.4");
                Ini.WriteValue("3rdPersonCameraLateralFar", "1");
                Ini.WriteValue("3rdPersonCameraForwardPrediction", "1");
                Ini.WriteValue("3rdPersonCameraSpeed", "6");

                Ini.WriteValue("lookAtPosX", "0");
                Ini.WriteValue("lookAtPosY", "1");
                Ini.WriteValue("lookAtPosZ", "10");

                Ini.WriteValue("useSway", "True");
                Ini.WriteValue("maxSway", "0.15");
                Ini.WriteValue("swaySpeed", "0.01");

                Ini.Save();
			}
			else
			{
                bool needSave = false;
				if (Ini.GetValue("thirdPerson", "", "missing") == "missing")
				{
					Ini.WriteValue("thirdPerson", "True");
                    needSave = true;
				}
                if (Ini.GetValue("cameraPreview", "", "missing") == "missing")
                {
                    Ini.WriteValue("cameraPreview", "True");
                    Ini.WriteValue("moveCameraInGame", "True");
                    needSave = true;
                }
                if(Ini.GetValue("avoidWalls", "", "missing") == "missing")
                {
                    Ini.WriteValue("avoidWalls", "True");
                    Ini.WriteValue("lookAtPosX", "0");
                    Ini.WriteValue("lookAtPosY", "1");
                    Ini.WriteValue("lookAtPosZ", "10");
                    needSave = true;
                }
                if(Ini.GetValue("useSway", "", "missing") == "missing")
                {
                    Ini.WriteValue("useSway", "True");
                    Ini.WriteValue("maxSway", "0.15");
                    Ini.WriteValue("swaySpeed", "0.01");
                    needSave = true;
                }

                if (needSave)
                {
                    Ini.Save();
                }
			}

			_iniWatcher = new FileSystemWatcher(Environment.CurrentDirectory)
			{
				NotifyFilter = NotifyFilters.LastWrite,
                Filter = "DynamicCamera.cfg",
                EnableRaisingEvents = true
			};
			_iniWatcher.Changed += IniWatcherOnChanged;
		}

		public void OnApplicationQuit()
		{
			SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
			_iniWatcher.Changed -= IniWatcherOnChanged;
			Ini.Save();
		}

		public void OnLevelWasLoaded(int level)
		{
		}

		public void OnLevelWasInitialized(int level)
		{
		}

		public void OnUpdate()
		{
		}

		public void OnFixedUpdate()
		{
		}

		private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
		{
            if (scene.buildIndex < 1)
            {
                return;
            }

            if (_cameraPlus != null)
            {
                Object.Destroy(_cameraPlus.gameObject);
            }

			var mainCamera = Object.FindObjectsOfType<Camera>().FirstOrDefault(x => x.CompareTag("MainCamera"));
            if (mainCamera == null)
            {
                return;
            }

			GameObject gameObj = new GameObject("CameraPlus");
			CameraPlusBehaviour.MainCamera = mainCamera;
			_cameraPlus = gameObj.AddComponent<CameraPlusBehaviour>();
		}

		private void IniWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
		{
			if (_cameraPlus == null) return;
			Ini.Load();
			_cameraPlus.ReadIni();
		}
	}
}