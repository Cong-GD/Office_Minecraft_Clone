using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using ObjectPooling;
using System.Collections.Generic;
using UnityEngine;


public class PickupManager : GlobalReference<PickupManager>
{
    [SerializeField]
    private ObjectPool freeObjectPool;

    [SerializeField]
    private float pickupAllowTime = 0.5f;

    [SerializeField]
    private float objectRotateSpeed;

    [SerializeField]
    private float itemSuckRange;

    [SerializeField] 
    private float itemSuckSpeed;

    [SerializeField]
    private float pickupRange;

    [SerializeField]
    private float itemLifeTime;

    [SerializeField]
    private float maxDistanceFormPlayer;

    [SerializeField]
    private Rigidbody playerBody;

    private HashSet<FreeMinecraftObject> _activefreeObjects = new();

    private Queue<FreeMinecraftObject> _returningObject = new();

    private Vector3 _playerPosition;

    private void FixedUpdate()
    {
        foreach (var freeObject in _activefreeObjects)
        {
            if (ItemLifeTimePass(freeObject))
                continue;

            _playerPosition = playerBody.position + new Vector3(0, 1.5f, 0);
            float distanceToPlayer = (freeObject.transform.position - _playerPosition).magnitude;

            if (MaxDistancePass(freeObject, distanceToPlayer))
                continue;

            if (ItemSuckPass(freeObject, distanceToPlayer)) 
                continue;

            if (ItemPickupPass(freeObject, distanceToPlayer))
                continue;

            freeObject.Rotate(objectRotateSpeed * Time.fixedDeltaTime);
        }

        while(_returningObject.TryDequeue(out var freeObject))
        {
            _activefreeObjects.Remove(freeObject);
            freeObject.ReturnToPool();
        }
    }

    public void ThrowItem(ItemPacked item, Vector3 position, Vector3 force)
    {
        if (item.IsEmpty())
            return;

        var instance = (FreeMinecraftObject)freeObjectPool.Get();
        _activefreeObjects.Add(instance);
        instance.Init(item, position, force);
    }

    private bool ItemLifeTimePass(FreeMinecraftObject freeMinecraftObject)
    {
        if(Time.time > freeMinecraftObject.ActivatedTime + itemLifeTime)
        {
            _returningObject.Enqueue(freeMinecraftObject);
            return true;
        }
        return false;
    }

    private bool MaxDistancePass(FreeMinecraftObject freeMinecraftObject, float distanceToPlayer)
    {
        if(distanceToPlayer > maxDistanceFormPlayer)
        {
            _returningObject.Enqueue(freeMinecraftObject);
            return true;
        }
        return false;
    }

    private bool ItemSuckPass(FreeMinecraftObject freeMinecraftObject, float distanceToPlayer)
    {
        if(distanceToPlayer < itemSuckRange && IsPickUpAble(freeMinecraftObject))
        {
            var direction = (_playerPosition - freeMinecraftObject.transform.position).normalized;
            freeMinecraftObject.MovePosition(itemSuckSpeed * Time.fixedDeltaTime * direction);
        }
        return false;
    }

    private bool ItemPickupPass(FreeMinecraftObject freeMinecraftObject, float distanceToPlayer)
    {
        if(distanceToPlayer < pickupRange && IsPickUpAble(freeMinecraftObject) && freeMinecraftObject.AddToIventory())
        {
            _returningObject.Enqueue(freeMinecraftObject);
            return true;
        }
        return false;
    }

    private bool IsPickUpAble(FreeMinecraftObject freeMinecraftObject)
    {
        return Time.time > freeMinecraftObject.ActivatedTime + pickupAllowTime;
    }
}