using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToTestLevel()
    {
        var status = NetworkManager.Singleton.SceneManager.LoadScene("SimpleTest", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {"SimpleTest"} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }
}
