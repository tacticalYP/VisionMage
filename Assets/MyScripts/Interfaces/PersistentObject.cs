using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    void Awake()
    {
        // This keeps the object alive when the scene changes
        DontDestroyOnLoad(gameObject);
    }
}