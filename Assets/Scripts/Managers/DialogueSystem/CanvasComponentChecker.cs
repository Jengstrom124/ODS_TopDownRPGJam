using UnityEngine;

public class CanvasComponentChecker : MonoBehaviour
{
    private void OnDisable()
    {
        CheckForCanvas();
    }
    public void CheckForCanvas()
    {
        if (GetComponent<Canvas>() == null) return;
        Destroy(GetComponent<Canvas>());
    }
}
