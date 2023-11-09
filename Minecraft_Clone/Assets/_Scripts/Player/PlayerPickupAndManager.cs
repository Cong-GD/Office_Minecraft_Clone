using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using UnityEngine;

public class PlayerPickupAndManager : MonoBehaviour
{
    [SerializeField]
    private float pickupAllowTime = 0.5f;


    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<FreeMinecraftObject>(out var obj))
        {
            if (Time.time < obj.ActivatedTime + pickupAllowTime)
                return;

            obj.AddToIventory();
        }
        else
        {
            Debug.LogWarning($"Unexpected trigger {other.name}");
        }
    }
}