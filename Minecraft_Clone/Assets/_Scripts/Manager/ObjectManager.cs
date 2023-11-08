using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [ShowNonSerializedField]
    private static ObjectManager _instance;

    private static ObjectManager Instance
    {
        get
        {
            Init();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<ObjectManager>();
            if(_instance == null )
            {
                var gameObject = new GameObject("Object Manager");
                _instance = gameObject.AddComponent<ObjectManager>();
            }
            else
            {
                _instance.DestroyCreatedObjects();
            }
        }
    }

    public static void AddToManagingList(Object obj)
    {
        Instance._objects.Add(obj);
    }

    [SerializeField ,ReadOnly]
    private List<Object> _objects = new();

    private void Awake()
    {
        if(Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        DestroyCreatedObjects();
    }


    [Button]
    public void DestroyCreatedObjects()
    {
        foreach (Object obj in _objects)
        {
            DestroyImmediate(obj);
        }
        Debug.Log(_objects.Count + " objects has been destroyed");
        _objects.Clear();
    }
}