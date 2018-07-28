using System;
using System.Globalization;
using UnityEngine;
using VRUIControls;

namespace CameraPlus
{
	public class CameraMoverPointer : VRPointer
	{
		private Transform _grabbedCamera;
		public static VRController _grabbingController;
		private Vector3 _grabPos;
		private Quaternion _grabRot;
		private Vector3 _realPos;
		private Quaternion _realRot;
		private const float MinDistance = 0.25f;

		public override void Update()
		{
			base.Update();
			if (vrController != null)
				if (vrController.triggerValue > 0.9f)
				{
					if (_grabbingController != null) return;
					RaycastHit hit;
					if (Physics.Raycast(vrController.position, vrController.forward, out hit, _defaultLaserPointerLength))
					{
						if (hit.transform.name != "CameraCube") return;
						_grabbedCamera = hit.transform;
						_grabbingController = vrController;
						_grabPos = vrController.transform.InverseTransformPoint(_grabbedCamera.position);
						_grabRot = Quaternion.Inverse(vrController.transform.rotation) * _grabbedCamera.rotation;
					}
				}

            if (_grabbingController == null || !(_grabbingController.triggerValue <= 0.9f))
            {
                return;
            }

			SaveToIni();
			_grabbingController = null;
		}

		private void LateUpdate()
		{
            if (_grabbedCamera == null)
            {
                return;
            }

			if (_grabbingController != null)
			{
				var diff = _grabbingController.verticalAxisValue * Time.deltaTime;
				if (_grabPos.magnitude > MinDistance)
				{
					_grabPos -= Vector3.forward * diff;
				}
				else
				{	
					_grabPos -= Vector3.forward * Mathf.Clamp(diff, float.MinValue, 0);
				}
				_realPos = _grabbingController.transform.TransformPoint(_grabPos);
				_realRot = _grabbingController.transform.rotation * _grabRot;
            }

            _grabbedCamera.position = _realPos;
            _grabbedCamera.rotation = _realRot;
        }           

		private void SaveToIni()
		{
			var ini = Plugin.Ini;
			var pos = _grabbedCamera.position;
            Vector3 focusPoint = GetFocusPoint();

            CameraPlusBehaviour.m_3rdPersonCameraLateralFar  = pos.x;
            CameraPlusBehaviour.m_3rdPersonCameraUpperHeight = pos.y;
            CameraPlusBehaviour.m_3rdPersonCameraDistance    = -pos.z;
            CameraPlusBehaviour.m_lookAtPos = focusPoint;

            ini.WriteValue("3rdPersonCameraLateralFar", pos.x.ToString(CultureInfo.InvariantCulture));
            ini.WriteValue("3rdPersonCameraUpperHeight", pos.y.ToString(CultureInfo.InvariantCulture));
            ini.WriteValue("3rdPersonCameraDistance", (-pos.z).ToString(CultureInfo.InvariantCulture));

            ini.WriteValue("lookAtPosX", focusPoint.x.ToString(CultureInfo.InvariantCulture));
            ini.WriteValue("lookAtPosY", focusPoint.y.ToString(CultureInfo.InvariantCulture));
            ini.WriteValue("lookAtPosZ", focusPoint.z.ToString(CultureInfo.InvariantCulture));
        }

        private Vector3 GetFocusPoint()
        {
            Vector3 cameraPos = _grabbedCamera.position;
            Vector3 cameraDir = _grabbedCamera.forward;

            Vector3 tempPos = cameraPos + cameraDir;

            if (Mathf.Abs(cameraDir.x) < 0.0001f) //when looking at a point parallel to the (X = 0) plane
            {
                return CameraPlusBehaviour.m_lookAtPos;
            }

            float cursor = -cameraPos.x / (tempPos.x - cameraPos.x);

            return cameraPos + cursor * cameraDir;
        }
	}
}