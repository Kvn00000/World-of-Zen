using UnityEngine;
using UnityEngine.SceneManagement; // 用于加载场景

public class DoorTrigger : MonoBehaviour
{
    public string interiorSceneName; // 室内场景的名称

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has triggered the door!"); // 调试用
            EnterInterior();
        }
        else
        {
            Debug.Log("Something else entered the trigger: " + other.name); // 调试用
        }
    }

    private void EnterInterior()
    {
        // 加载室内场景
        SceneManager.LoadScene(interiorSceneName);
    }
}
