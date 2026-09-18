using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartCampaign()
    {
        SceneManager.LoadScene("Mission01");
    }

    public void OpenAircraftSelection()
    {
        SceneManager.LoadScene("AircraftSelection");
    }

    public void OpenMissionSelection()
    {
        SceneManager.LoadScene("MissionSelection");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
