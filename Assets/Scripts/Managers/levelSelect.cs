using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelSelect : MonoBehaviour {
    private int selectedScene; //Keep track of which scene is currently selected
    private bool animate;
    private GameObject door;
    private int scenePaused;
    private Image defaultImage;
    private int numScenes;

    private Color selectedColor;

    //For scrolling
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    private Vector2 origin;
    private int quadrant;


    // Use this for initialization
    void Start () {
        defaultImage = GameObject.Find("Image").GetComponent<Image>();
        door = GameObject.Find("Door");
        var tracker = GameObject.Find("Tracker").GetComponent<tracker>();
        scenePaused = tracker.lastScene;
        numScenes = 18;
        if (scenePaused == 0) {
            selectedScene = 1;
            SelectScene();
        }
        origin = contentPanel.anchoredPosition;
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
            selectedColor = level.GetComponentInChildren<Text>().color;
            level.GetComponentInChildren<Text>().color = Color.white;
            defaultImage.sprite = Resources.Load<Sprite>(sceneName);
        }

        SnapTo(level.GetComponent<RectTransform>());
    }

    private void UnSelectScene() {
        if (selectedScene != 0) {
            var level = GameObject.Find("Level" + selectedScene);
            level.GetComponentInChildren<Text>().color = selectedColor;
        }
        if (scenePaused != 0) {
            var lastLevel = GameObject.Find("Level" + scenePaused);
            lastLevel.GetComponentInChildren<Text>().color = selectedColor;
            scenePaused = 0;
        }
    }

    //Heavily modified from user maksymiuk on StackOverflow 
    //at http://stackoverflow.com/questions/30766020/how-to-scroll-to-a-specific-element-in-scrollrect-with-unity-ui
    public void SnapTo(RectTransform target) {
        float offset = 50f;
        Vector2 sizeOfRect = scrollRect.GetComponent<RectTransform>().sizeDelta;
        Vector2 sizeOfContent = contentPanel.GetComponent<RectTransform>().sizeDelta;
        float halfView = sizeOfRect.y / 2.0f;
        float fourth = sizeOfContent.y / 4;
        Canvas.ForceUpdateCanvases();

        Vector2 move = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);


        // Debug.Log(move.y)

        Vector2 newPos = new Vector2(0, 0);
        if (move.y < halfView) {
            newPos = origin;
        } else if (move.y < sizeOfRect.y) {
            newPos = new Vector2(contentPanel.anchoredPosition.x, origin.y + halfView - offset);
        } else if (move.y < (sizeOfRect.y + halfView)) {
            //    Debug.Log("Third else " + (origin.y+2*halfView - offset));
            newPos = new Vector2(contentPanel.anchoredPosition.x, origin.y + 2 * fourth - offset*2);
        }
            //} else if ((move.y < sizeOfRect.y*2)) {
            //    Debug.Log("moving here!");
            //    float newY = origin.y + sizeOfContent.y - halfView;
            //    Debug.Log("New y" + newY);
            //    newPos = new Vector2(contentPanel.anchoredPosition.x, newY );
            //}
            Debug.Log("moving to " + newPos);
            contentPanel.anchoredPosition = newPos;


            //        Debug.Log("scroll: " + scroll);
            //        Debug.Log("Amount to move " + move.y);
            //        if ((Mathf.Abs(move.y) > scroll)) {
            //// Debug.Log("Scrolling " + halfScroll);
            //            Canvas.ForceUpdateCanvases();
            //            scroll += 200f;
            //            Vector2 newPos = new Vector2(contentPanel.anchoredPosition.x, move.y + offset);
            //            contentPanel.anchoredPosition = newPos;
            //        }

            //        if (scroll > sizeOfRect.y) {
            //            scroll = 200f;
            //        }
        }

    int MyMod(int a, int b) {
        int rem = a % b;
        if (rem < 0) {
            rem = b + rem;
        }
        return rem;
    }
}
