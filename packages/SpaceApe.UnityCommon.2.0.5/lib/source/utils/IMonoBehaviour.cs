using UnityEngine;

public abstract class IMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake() { }
    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void LateUpdate() { }

    protected virtual void OnDrawGizmos() { }

    protected virtual void OnDestroy() { }

    protected virtual void OnCollisionEnter() { }
    protected virtual void OnCollisionEnter(Collision collision) { }

    protected virtual void OnTriggerEnter2D(Collider2D other) { }
}
