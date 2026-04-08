using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TriggerOnEnable : MonoBehaviour
{
    [SerializeField] float delay = 0f;
    [SerializeField] bool cancelOnDisable = false;
    public UnityEvent events;

    IEnumerator triggerCallbackCoroutine_;
    private void OnEnable()
    {
        triggerCallbackCoroutine_ = TriggerCallbackCoroutine();
        StartCoroutine(triggerCallbackCoroutine_);
    }
    IEnumerator TriggerCallbackCoroutine()
    {
        yield return new WaitForSeconds(delay);
        TriggerCallback();
    }
    void TriggerCallback()
    {
        events?.Invoke();
    }
    private void OnDisable()
    {
        if (!cancelOnDisable || triggerCallbackCoroutine_ == null) return;
        StopCoroutine(triggerCallbackCoroutine_);
    }
}
