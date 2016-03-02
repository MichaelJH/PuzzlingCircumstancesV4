using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class levelSelect : MonoBehaviour {
    private int selectedScene; //Keep track of which scene is currently selected
    private bool animate;
    private GameObject door;
    private int scenePaused;
    private UnityEngine.UI.Image defaultImage;
    private int numScenes;

    // Use this for initialization
    void Start () {
        defaultImage = GameObject.Find("Image").GetComponent<UnityEngine.UI.Image>();
        door = GameObject.Find("Door");
        var tracker = GameObject.Find("Tracker").GetComponent<tracker>();
        scenePaused = tracker.lastScene;
        numScenes = 18;
        if (scenePaused == 0) {
            selectedScene = 1;
            SelectScene();
        }
    }

    void Update () {
        if (animate == true) {
            ExitAnimation();
        }
        if (scenePaused != 0) {
            selectedScene = scenePaused;
            SelectScene();
        }

        //Navigate with arrow keys
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            UnSelectScene();
            selectedScene = MyMod(selectedScene, numScenes)+1;
            SelectScene();
        } else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            UnSelectScene();
            if (selectedScene - 1 == 0) {
                selectedScene = numScenes;
            } else {
                selectedScene = MyMod(selectedScene - 1, numScenes);
            }
            SelectScene();
        } else if (Input.GetKeyDown(KeyCode.Return)) {
            Clicked();
        }
    }
	
    public void Selected(int sceneNum) {
        //Change previously selected to original color
        UnSelectScene();
        //new scene
        selectedScene = sceneNum;
        SelectScene();
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

    private void SelectScene() {
        var level = GameObject.Find("Level" + selectedScene);
        string sceneName = "Room" + selectedScene;

        if (level != null) {
            level.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.white;
            defaultImage.sprite = Resources.Load<Sprite>(sceneName);
        }
    }

    private void UnSelectScene() {
        if (selectedScene != 0) {
            var level = GameObject.Find("Level" + selectedScene);
            level.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(50, 50, 50, 250);
        }
        if (scenePaused != 0) {
            var lastLevel = GameObject.Find("Level" + scenePaused);
            lastLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(50, 50, 50, 250);
            scenePaused = 0;
        }
    }

    int MyMod(int a, int b) {
        int rem = a % b;
        if (rem < 0) {
            rem = b + rem;
        }
        return rem;
    }
}
