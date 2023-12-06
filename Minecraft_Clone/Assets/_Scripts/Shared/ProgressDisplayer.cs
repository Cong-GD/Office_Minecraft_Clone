using UnityEngine;

public abstract class ProgressDisplayer : MonoBehaviour
{
    public abstract void Enable();
    public abstract void Disable();
    public abstract void SetValue(float value);
}
