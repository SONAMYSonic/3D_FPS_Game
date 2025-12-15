using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private GameObject _minimapCamera;
    [SerializeField] private MinimapCamera _minimapCameraScript;

    public void MinimapZoomIn()
    {
        _minimapCameraScript.OffsetY -= 5f;
    }

    public void MinimapZoomOut()
    {
        _minimapCameraScript.OffsetY += 5f;
    }
}
