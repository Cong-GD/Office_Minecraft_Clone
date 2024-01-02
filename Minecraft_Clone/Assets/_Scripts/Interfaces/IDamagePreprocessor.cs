namespace Minecraft
{
    public interface IDamagePreprocessor
    {
        void PreprocessDamage(ref int damage, ref DamegeType damegeType);
    }
}
