public static class ClassCache<T> where T : class, new()
{
    [System.ThreadStatic]
    private static T _cached;

    public static T Get()
    {
        if(_cached == null)
        {
            return new T();
        }
        var instance = _cached;
        _cached = null;
        return instance;
    }

    public static void Release(T instance)
    {
        _cached = instance;
    }
}