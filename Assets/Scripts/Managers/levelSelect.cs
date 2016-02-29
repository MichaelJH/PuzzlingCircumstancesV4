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
        selectedScene = sceneNum;
        //Sets the image on the right
        string sceneName = "Room" + sceneNum;
        defaultImage.sprite = Resources.Load<Sprite>(sceneName);

        //Set label
        var label = GameObject.Find("Label").GetComponent<UnityEngine.UI.Text>();
        label.text = "Room " + sceneNum;
    }


    public void Clicked() {
        SceneManager.LoadScene(selectedScene);
        var manager = GameObject.Find("roomManager").GetComponent<roomManagement>();
        manager.RestartLevel();
    }
}
