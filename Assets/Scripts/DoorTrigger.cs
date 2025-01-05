using UnityEngine;
using UnityEngine.SceneManagement; // 用于加载场景

public class DoorTrigger : MonoBehaviour
{
    public string interiorSceneName; // 室内场景的名称
    public string exteriorSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has triggered the door!"); // 调试用
            ToggleScene();
        }
        else
        {
            Debug.Log("Something else entered the trigger: " + other.name); // 调试用
        }
    }
    
    private void ToggleScene()
    {
        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == exteriorSceneName)
        {
            Debug.Log("Switching to interior scene: " + interiorSceneName);
            SceneManager.LoadScene(interiorSceneName);
        }
        else if (currentSceneName == interiorSceneName)
        {
            Debug.Log("Switching to exterior scene: " + exteriorSceneName);
            SceneManager.LoadScene(exteriorSceneName);
        }
        else
        {
            Debug.LogError("Current scene name does not match known scenes!");
        }
    }
}
