using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class levelSelect : MonoBehaviour {
    public int selectedScene;

    private UnityEngine.UI.Image defaultImage;
	
    // Use this for initialization
	void Start () {
        defaultImage = GameObject.Find("Image").GetComponent<UnityEngine.UI.Image>();
    }
	

    public void Selected(int sceneNum) {
        //Change previously selected to black
        if (selectedScene != 0) {
            var curLevel = GameObject.Find("Level" + selectedScene);
            curLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(50,50,50,250);
        }

        selectedScene = sceneNum;
        //Sets the image on the right
        string sceneName = "Room" + sceneNum;
        defaultImage.sprite = Resources.Load<Sprite>(sceneName);

        //Set label
        //var label = GameObject.Find("Label").GetComponent<UnityEngine.UI.Text>();
        //label.text = "Room " + sceneNum;

        //Trying to change font
        var newLevel = GameObject.Find("Level" + sceneNum);
        newLevel.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.white;
    }


    public void Clicked() {
        SceneManager.LoadScene(selectedScene);
        var manager = GameObject.Find("roomManager").GetComponent<roomManagement>();
        manager.RestartLevel();
    }
}
