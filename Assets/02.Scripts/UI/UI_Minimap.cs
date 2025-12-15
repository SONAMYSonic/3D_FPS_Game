using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private MinimapCamera _minimapCameraScript;
    [SerializeField] private float _zoom = 5f;

    public void MinimapZoomIn()
    {
        _minimapCameraScript.OffsetY -= _zoom;
    }

    public void MinimapZoomOut()
    {
        _minimapCameraScript.OffsetY += _zoom;
    }
}
