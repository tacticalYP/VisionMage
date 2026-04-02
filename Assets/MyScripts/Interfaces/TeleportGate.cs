using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportGate : MonoBehaviour
{
    [Header("Scene Settings")]
    public string bossSceneName = "BossRoom";

    private bool isTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            TeleportToBoss();
        }
    }

    void TeleportToBoss()
    {
        SceneLoader.Instance.LoadScene(bossSceneName);
    }
}