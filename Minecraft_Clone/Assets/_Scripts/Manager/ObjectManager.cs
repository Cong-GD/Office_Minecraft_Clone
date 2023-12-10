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
            Initialize();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<ObjectManager>();
            if(_instance == null )
            {
                GameObject gameObject = new GameObject("Object Manager");
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
        Instance.objects.Add(obj);
    }

    [SerializeField ,ReadOnly]
    private List<Object> objects = new();

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
        foreach (Object obj in objects)
        {
            DestroyImmediate(obj);
        }
        Debug.Log(objects.Count + " objects has been destroyed");
        objects.Clear();
    }
}