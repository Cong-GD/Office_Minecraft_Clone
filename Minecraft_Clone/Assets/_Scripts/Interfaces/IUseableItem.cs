public interface IUseableItem
{
    public object OnStartUse();
    public bool Using(ref float holdedTime, ref object usingContext);
    public void OnEndUse(float holdedTime, object usingContext);
}