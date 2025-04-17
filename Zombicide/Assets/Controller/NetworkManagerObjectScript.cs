using Network;
using UnityEngine;

public class NetworkManagerObjectScript : MonoBehaviour
{
    public static NetworkManagerObjectScript Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
