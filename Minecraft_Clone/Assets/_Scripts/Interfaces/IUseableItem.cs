public interface IUseableItem
{
    public object OnStartUse();
    public bool Using(ref float holdedTime, ref object useContext);
    public void OnEndUse(float holdedTime, object useContext);
}