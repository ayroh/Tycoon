using UnityEngine;
public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    static T _instance = null;
    public static T instance
    {
        get
        {
            if (!_instance)
            {
                _instance = Resources.Load<T>(typeof(T).ToString().Replace('.', '/'));
            }
            return _instance;
        }
    }
}