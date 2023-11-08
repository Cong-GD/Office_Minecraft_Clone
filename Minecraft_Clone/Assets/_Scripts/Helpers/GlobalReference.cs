using UnityEngine;

public class GlobalReference<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField]
    private bool immortal;

    private static T _reference;
    public static T Instance 
    {
        get
        {
            if(!IsValidInstance())
            {
                _reference = FindAnyObjectByType<T>();
            }
            return _reference;
        }
    }

    public static bool IsValidInstance()
    {
        return _reference != null;
    }

    protected virtual void Awake()
    {
        if(IsValidInstance() && _reference != this)
        {
            Destroy(gameObject);
            return;
        }
            
        _reference = (T)(MonoBehaviour)this;
        if (immortal)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
