using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : MonoBehaviour
{
    public static PlayerSpawnHandler Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("1");
        if (scene.name == "BossRoom")
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject spawn = GameObject.Find("PlayerSpawnPoint");
            Debug.Log($"{player==null} {spawn==null}");

            if (player != null && spawn != null)
            {
                Debug.Log("3");
                player.transform.position = spawn.transform.position;
                player.GetComponent<PlayerHealth>().respawnPoint = spawn.transform;
            }
        }
    }
}