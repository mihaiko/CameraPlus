using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRUIControls;

namespace CameraPlus
{
	public class CameraPlusBehaviour : MonoBehaviour
	{
		public static Camera MainCamera;
		public static float FOV;
		public static float PosSmooth;
		public static float RotSmooth;

		public static bool ThirdPerson
		{
			get { return _thirdPerson; }
			set
			{
				_thirdPerson = value;
				_cameraCube.gameObject.SetActive(_thirdPerson);
			}
		}
        public static bool m_MoveCameraInGame = true;
        public static bool m_UsePreviewCamera = true;
        public static bool m_AvoidWalls = true;

        private static bool _thirdPerson;
		private static RenderTexture _renderTexture;
		private static Material _previewMaterial;
		private static Camera _cameraComp;
		private static Camera _previewCam;
		private static Transform _cameraCube;
		private const int Width = 256;

        public static float m_3rdPersonCameraDistance;
        public static float m_3rdPersonCameraUpperHeight;
        public static float m_3rdPersonCameraLowerHeight;
        public static float m_3rdPersonCameraLateralNear;
        public static float m_3rdPersonCameraLateralFar;
        public static float m_3rdPersonCameraForwardPrediction;
        public static float m_3rdPersonCameraSpeed;
        public static Vector3 m_lookAtPos = Vector3.zero;

        private Vector3 currentPosition = Vector3.zero;
        private Vector3 wantedPosition = Vector3.zero;
        private Vector3 potentialPosition = Vector3.zero;

        private void Awake()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
			SceneManagerOnActiveSceneChanged(new Scene(), new Scene());

			GameObject newCameraObject = Instantiate(MainCamera.gameObject);
            newCameraObject.SetActive(false);
            newCameraObject.name = "Camera Plus";
            newCameraObject.tag = "Untagged";

            while (newCameraObject.transform.childCount > 0)
            {
                DestroyImmediate(newCameraObject.transform.GetChild(0).gameObject);
            }

			DestroyImmediate(newCameraObject.GetComponent("CameraRenderCallbacksManager"));
			DestroyImmediate(newCameraObject.GetComponent("AudioListener"));
			DestroyImmediate(newCameraObject.GetComponent("MeshCollider"));

			if (SteamVRCompatibility.IsAvailable)
			{
				DestroyImmediate(newCameraObject.GetComponent(SteamVRCompatibility.SteamVRCamera));
				DestroyImmediate(newCameraObject.GetComponent(SteamVRCompatibility.SteamVRFade));
			}

            _cameraComp = newCameraObject.GetComponent<Camera>();
            _cameraComp.stereoTargetEye = StereoTargetEyeMask.None;
            _cameraComp.targetTexture = null;
            _cameraComp.depth += 100;

            newCameraObject.SetActive(true);

			Transform cameraTransform = MainCamera.transform;
			transform.position = cameraTransform.position;
			transform.rotation = cameraTransform.rotation;

            newCameraObject.transform.parent = transform;
            newCameraObject.transform.localPosition = Vector3.zero;
            newCameraObject.transform.localRotation = Quaternion.identity;
            newCameraObject.transform.localScale = Vector3.one;

			var cameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cameraCube.SetActive(ThirdPerson);
			_cameraCube = cameraCube.transform;
			_cameraCube.localScale = new Vector3(0.15f, 0.15f, 0.22f);
			_cameraCube.name = "CameraCube";

            ReadIni();

            if (m_UsePreviewCamera)
            {
                _previewCam = Instantiate(_cameraComp.gameObject, _cameraCube).GetComponent<Camera>();

                if (_renderTexture == null && _previewMaterial == null)
                {
                    _renderTexture = new RenderTexture(Width, (int)(Width / _cameraComp.aspect), 24);
                    _previewMaterial = new Material(Shader.Find("Hidden/BlitCopyWithDepth"));
                    _previewMaterial.SetTexture("_MainTex", _renderTexture);
                }

                _previewCam.targetTexture = _renderTexture;

                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                DestroyImmediate(quad.GetComponent<Collider>());
                quad.GetComponent<MeshRenderer>().material = _previewMaterial;
                quad.transform.parent = _cameraCube;
                quad.transform.localPosition = new Vector3(-1f * ((_cameraComp.aspect - 1) / 2 + 1), 0, 0.22f);
                quad.transform.localEulerAngles = new Vector3(0, 180, 0);
                quad.transform.localScale = new Vector3(-1 * _cameraComp.aspect, 1, 1);
            }

            if (m_MoveCameraInGame)
            {
                var pointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();
                if (pointer == null) return;
                ReflectionUtil.CopyComponent(pointer, typeof(CameraMoverPointer), pointer.gameObject);
                DestroyImmediate(pointer);
            }
        }

		private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
		{
		}

		public void ReadIni()
		{
			FOV = Convert.ToSingle(Plugin.Ini.GetValue("fov", "", "90"), CultureInfo.InvariantCulture);
			PosSmooth = Convert.ToSingle(Plugin.Ini.GetValue("positionSmooth", "", "10"), CultureInfo.InvariantCulture);
			RotSmooth = Convert.ToSingle(Plugin.Ini.GetValue("rotationSmooth", "", "5"), CultureInfo.InvariantCulture);

			ThirdPerson = Convert.ToBoolean(Plugin.Ini.GetValue("thirdPerson", "", "True"), CultureInfo.InvariantCulture);
            m_MoveCameraInGame = Convert.ToBoolean(Plugin.Ini.GetValue("moveCameraInGame", "", "True"), CultureInfo.InvariantCulture);
            m_UsePreviewCamera = Convert.ToBoolean(Plugin.Ini.GetValue("cameraPreview", "", "True"), CultureInfo.InvariantCulture);
            m_AvoidWalls = Convert.ToBoolean(Plugin.Ini.GetValue("avoidWalls", "", "True"), CultureInfo.InvariantCulture);

            m_3rdPersonCameraDistance = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraDistance", "", "0.9"), CultureInfo.InvariantCulture);
            m_3rdPersonCameraUpperHeight = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraUpperHeight", "", "1.7"), CultureInfo.InvariantCulture);
            m_3rdPersonCameraLowerHeight = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraLowerHeight", "", "0.5"), CultureInfo.InvariantCulture);
            m_3rdPersonCameraLateralNear = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraLateralNear", "", "0.35"), CultureInfo.InvariantCulture);
            m_3rdPersonCameraLateralFar = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraLateralFar", "", "0.9"), CultureInfo.InvariantCulture);
            m_3rdPersonCameraForwardPrediction = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraForwardPrediction", "", "1"), CultureInfo.InvariantCulture);
            m_3rdPersonCameraSpeed = Convert.ToSingle(Plugin.Ini.GetValue("3rdPersonCameraSpeed", "", "4"), CultureInfo.InvariantCulture);

            m_lookAtPos = new Vector3(Convert.ToSingle(Plugin.Ini.GetValue("lookAtPosX", "", "0"), CultureInfo.InvariantCulture),
                Convert.ToSingle(Plugin.Ini.GetValue("lookAtPosY", "", "1"), CultureInfo.InvariantCulture),
                Convert.ToSingle(Plugin.Ini.GetValue("lookAtPosZ", "", "10"), CultureInfo.InvariantCulture));

            SetFOV();
		}

		private void LateUpdate()
		{
            if(CameraMoverPointer._grabbingController != null)
            {
                return;
            }

			var camera = MainCamera.transform;

			if (ThirdPerson)
            {
                Compute3RdPersonCamera();
                return;
			}

			transform.position = Vector3.Lerp(transform.position, camera.position, PosSmooth * Time.deltaTime);
			transform.rotation = Quaternion.Slerp(transform.rotation, camera.rotation, RotSmooth * Time.deltaTime);
		}

        private void Compute3RdPersonCamera()
        {
            Vector3 upperOuterRight = new Vector3(m_3rdPersonCameraLateralFar, m_3rdPersonCameraUpperHeight, -m_3rdPersonCameraDistance);

            if(m_AvoidWalls)
            {
                Vector3 upperOuterLeft = new Vector3(-m_3rdPersonCameraLateralFar, m_3rdPersonCameraUpperHeight, -m_3rdPersonCameraDistance);
                Vector3 upperInnerRight = new Vector3(m_3rdPersonCameraLateralNear, m_3rdPersonCameraUpperHeight, -m_3rdPersonCameraDistance);
                Vector3 upperInnerLeft = new Vector3(-m_3rdPersonCameraLateralNear, m_3rdPersonCameraUpperHeight, -m_3rdPersonCameraDistance);
                Vector3 lowerOuterRight = new Vector3(m_3rdPersonCameraLateralFar, m_3rdPersonCameraLowerHeight, -m_3rdPersonCameraDistance);
                Vector3 lowerOuterLeft = new Vector3(-m_3rdPersonCameraLateralFar, m_3rdPersonCameraLowerHeight, -m_3rdPersonCameraDistance);
                Vector3 lowerInnerRight = new Vector3(m_3rdPersonCameraLateralNear, m_3rdPersonCameraLowerHeight, -m_3rdPersonCameraDistance);
                Vector3 lowerInnerLeft = new Vector3(-m_3rdPersonCameraLateralNear, m_3rdPersonCameraLowerHeight, -m_3rdPersonCameraDistance);

                //position
                if (currentPosition == Vector3.zero)
                {
                    currentPosition = upperOuterRight;
                    wantedPosition = upperOuterRight;
                }

                potentialPosition = Vector3.zero;

                if (IsPointAvailable(upperOuterRight))
                {
                    wantedPosition = upperOuterRight;
                }
                else if (IsPointAvailable(upperInnerRight))
                {
                    wantedPosition = upperInnerRight;
                }
                else if (IsPointAvailable(upperOuterLeft))
                {
                    wantedPosition = upperOuterLeft;
                }
                else if (IsPointAvailable(upperInnerLeft))
                {
                    wantedPosition = upperInnerLeft;
                }
                else if (IsPointAvailable(lowerOuterRight))
                {
                    wantedPosition = lowerOuterRight;
                }
                else if (IsPointAvailable(lowerOuterLeft))
                {
                    wantedPosition = lowerOuterLeft;
                }
                else if (IsPointAvailable(lowerInnerRight))
                {
                    wantedPosition = lowerInnerRight;
                }
                else if (IsPointAvailable(lowerInnerLeft))
                {
                    wantedPosition = lowerInnerLeft;
                }
                else if (potentialPosition != Vector3.zero)
                {
                    wantedPosition = potentialPosition;
                }

                if (wantedPosition != currentPosition)
                {
                    currentPosition = GetNewPos(currentPosition, wantedPosition);

                    if (Vector3.Distance(currentPosition, wantedPosition) < 0.001f)
                    {
                        currentPosition = wantedPosition;
                    }
                }
            }
            else
            {
                currentPosition = upperOuterRight;
            }

            transform.position = currentPosition;
            _cameraCube.position = currentPosition;

            //rotation
            transform.LookAt(m_lookAtPos);
            _cameraCube.LookAt(m_lookAtPos);
            Quaternion newRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f); //prohibit camera roll
            transform.rotation = newRotation;
            _cameraCube.rotation = newRotation;
        }

        private bool IsPointAvailable(Vector3 position)
        {
            RaycastHit hit;
            int layerMask = 1 << 11;
            Vector3 playerHead = MainCamera.transform.position;
            Vector3 direction = (position - playerHead).normalized;
            Vector3 startPos = playerHead + Vector3.forward * m_3rdPersonCameraForwardPrediction;
            float distance = (position - playerHead).magnitude;

            if (position.y == m_3rdPersonCameraLowerHeight)
            {
                playerHead.y = m_3rdPersonCameraLowerHeight;
            }

            if (Physics.Raycast(startPos, direction, out hit, distance, layerMask))
            {
                return false;
            }
            else if (potentialPosition == Vector3.zero)
            {
                potentialPosition = position;
            }

            if (Physics.Raycast(playerHead, direction, out hit, distance, layerMask))
            {
                return false;
            }

            return true;
        }

        private Vector3 GetNewPos(Vector3 currentPos, Vector3 wantedPos)
        {
            float cursor = Time.deltaTime * m_3rdPersonCameraSpeed;
            return (currentPos * (1 - cursor) + wantedPos * cursor);
        }

        private void SetFOV()
		{
			if (_cameraComp == null) return;
			var fov = (float) (57.2957801818848 *
			                   (2.0 * Mathf.Atan(Mathf.Tan((float) (FOV * (Math.PI / 180.0) * 0.5)) /
			                                     MainCamera.aspect)));
            _cameraComp.fieldOfView = fov;
			if (_previewCam == null) return;
			_previewCam.fieldOfView = fov;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				ThirdPerson = !ThirdPerson;
				if (!ThirdPerson)
				{
					transform.position = MainCamera.transform.position;
					transform.rotation = MainCamera.transform.rotation;
				}

				Plugin.Ini.WriteValue("thirdPerson", ThirdPerson.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}