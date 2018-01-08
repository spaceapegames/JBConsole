using UnityEngine;
using System.Collections;

public interface ICoroutineRunner
{
    Coroutine StartCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
    void StopCoroutine(IEnumerator routine);
    void StopAllCoroutines();
}

public class UnityCoroutineRunner : IMonoBehaviour, ICoroutineRunner
{
    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
