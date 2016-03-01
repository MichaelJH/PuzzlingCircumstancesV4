using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class levelSelect : MonoBehaviour {
    private int selectedScene;
    private bool animate;
    private GameObject door;
    private int scenePaused;
    private UnityEngine.UI.Image defaultImage;
	
    // Use this for initialization
	void Start () {
        defaultImage = GameObject.Find("Image").GetComponent<UnityEngine.UI.Image>();
        door = GameObject.Find("Door");
        var tracker = GameObject.Find("Tracker").GetComponent<tracker>();
        scenePaused = tracker.lastScene;
    }

    void Update () {
        if (animate == true) {
            ExitAnimation();
        }
        if (scenePaused != 0) {
            //highiligh last level player was in
            var lastLevel = GameObject.Find("Level" + scenePaused);
            lastLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.white;
            //show the image
            selectedScene = scenePaused;
            string sceneName = "Room" + selectedScene;
            defaultImage.sprite = Resources.Load<Sprite>(sceneName);
        }
    }
	
    public void Selected(int sceneNum) {
        //Change previously selected to black
        if (selectedScene != 0) {
            var curLevel = GameObject.Find("Level" + selectedScene);
            curLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(50,50,50,250);
        }
        if (scenePaused != 0) {
            var lastLevel = GameObject.Find("Level" + scenePaused);
            lastLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(50, 50, 50, 250);
            scenePaused = 0;
        }

        selectedScene = sceneNum;
        //Sets the image on the right
        string sceneName = "Room" + sceneNum;
        defaultImage.sprite = Resources.Load<Sprite>(sceneName);

        //Trying to change font
        var newLevel = GameObject.Find("Level" + sceneNum);
        newLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.white;
    }

    public void Clicked() {
        animate = true;
        door.GetComponent<doorScript>().RaiseGate();
    }

    private void ExitAnimation() {
        GameObject player = GameObject.Find("Player");
        var playerScript = player.GetComponent<playerController>();

        //Make player move towards door
        float exitSpeed = playerScript.playerSpeed * 30;
        Vector2 start = player.transform.position;
        Vector2 end = door.transform.position;
        end.y = start.y;

        Vector2 newPos = Vector2.MoveTowards(start, end, exitSpeed * Time.deltaTime);
        player.transform.position = newPos;

        door.GetComponent<doorScript>().TransitionRoomCall(selectedScene);
    }
}
