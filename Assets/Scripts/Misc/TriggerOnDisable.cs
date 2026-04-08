using UnityEngine;
using UnityEngine.Events;

public class TriggerOnDisable : MonoBehaviour
{
    public UnityEvent events;
    private void OnDisable()
    {
        events?.Invoke();
    }
}
