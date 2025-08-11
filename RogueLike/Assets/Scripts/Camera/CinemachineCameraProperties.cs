using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraProperties : MonoBehaviour
{
    private CinemachineBrain _brain;

    void Awake()
    {
        _brain = GetComponent<CinemachineBrain>();

        if (_brain.IgnoreTimeScale != true)
        {
            _brain.IgnoreTimeScale = true;
        }
        else
            Debug.Log("Ignore Time scale Property Already set to Active");


        _brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
    }

    void LateUpdate()
    {
        if (_brain.ActiveVirtualCamera is CinemachineVirtualCameraBase vcam)
        {
            vcam.InternalUpdateCameraState(Vector3.up , Time.unscaledDeltaTime);
        }
    }  

}
