using UnityEngine;
using Unity.Cinemachine; 

public class CameraHandler : MonoBehaviour
{
    public CinemachineCamera vcam1;
    public CinemachineCamera vcam2;

    public void SwitchToCamera2()
    {
        vcam1.Priority = 10;
        vcam2.Priority = 20; 
    }

    public void SwitchToCamera1()
    {
        vcam1.Priority = 20;
        vcam2.Priority = 10;
    }
}