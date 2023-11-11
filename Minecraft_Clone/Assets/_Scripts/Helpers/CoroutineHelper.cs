using System.Collections;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    
    private static CoroutineHelper Instance
    {
        get
        {
            if( _instance == null )
            {
                _instance = new GameObject("Coroutine Helper").AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public static void Start(IEnumerator routine, out Coroutine coroutine)
    {
        coroutine = Instance.StartCoroutine(routine);
    }

    public static Coroutine Start(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }

    public static void Stop(Coroutine coroutine)
    {
        if(coroutine != null)
        {
            Instance.StopCoroutine(coroutine);
        }
    }
}