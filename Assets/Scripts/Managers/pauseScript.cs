using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class pauseScript : MonoBehaviour {
    public bool pause;
    private GameObject player;

	// Use this for initialization
	void Start () {
        gameObject.SetActive(false);
        player = GameObject.Find("Player");
        pause = false;
    }

    void Update() {
        if (pause) {
            Time.timeScale = 0;  //pause game
            player.GetComponent<playerController>().move = false;
            player.GetComponent<PortalScript>().paused = true;
        }
    }

    public void ContinueGame() {
        gameObject.SetActive(false);
        Time.timeScale = 1;
        pause = false;
        player.GetComponent<playerController>().move = true;
        player.GetComponent<PortalScript>().paused = false;
    }

    public void RestartLevel() {
        Time.timeScale = 1;
        pause = false;
        gameObject.SetActive(false);
        var manager = GameObject.Find("roomManager").GetComponent<roomManagement>();
        manager.RestartLevel();
    }

    public void ExitGame() {
        Time.timeScale = 1;
        pause = false;
        SceneManager.LoadScene("endGame");
        Application.Quit();
    }

    public void SelectLevel() {
        Time.timeScale = 1;
        pause = false;
        SceneManager.LoadScene("listSceneSelection");
    }
}
