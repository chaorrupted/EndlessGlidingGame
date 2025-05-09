using UnityEngine;

public class RestartButton : MonoBehaviour
{
    [SerializeField] private RocketmanController rocketman;
    [SerializeField] private GameObject parentCanvas;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject launcher;
    [SerializeField] private EndlessLevelGenerator levelGenerator;

    public void RestartGame()
    {
        print("restarting game ....");
       
        rocketman.Restart();
        levelGenerator.Restart();
        
        mainCamera.GetComponent<Follower>().ResetCamera();
        launcher.SetActive(true);
        launcher.GetComponent<Launcher>().Reset();
        parentCanvas.SetActive(false);
    }
}
