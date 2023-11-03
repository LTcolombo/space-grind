using UnityEngine;
using UnityEngine.Events;

public class CollideEventProxy : MonoBehaviour
{
    [SerializeField] private UnityEvent OnCollide;
    private float _initializationTime;

    void Start()
    {
        _initializationTime = Time.timeSinceLevelLoad;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.timeSinceLevelLoad - _initializationTime > 0.5f)
            OnCollide?.Invoke();
    }
}