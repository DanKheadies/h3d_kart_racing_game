using UnityEngine;

public class HUDController : MonoBehaviour
{
    public GameObject miniMapCanvas;
    public GameObject mirrorCanvas;

    int showMiniMap = -1;
    int showMirror = -1;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CanvasGroup>().alpha = 0;

        if (PlayerPrefs.HasKey("showMiniMap"))
            showMiniMap = PlayerPrefs.GetInt("showMiniMap");

        if (PlayerPrefs.HasKey("showMirror"))
            showMirror = PlayerPrefs.GetInt("showMirror");

        Invoke("EnableHUD", 6);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            UpdateMiniMap();

        if (Input.GetKeyDown(KeyCode.R))
            UpdateMirror();
    }

    void UpdateMiniMap()
    {
        showMiniMap *= -1;
        PlayerPrefs.SetInt("showMiniMap", showMiniMap);

        if (showMiniMap == 1)
            miniMapCanvas.GetComponent<CanvasGroup>().alpha = 1;
        else
            miniMapCanvas.GetComponent<CanvasGroup>().alpha = 0;
    }

    void UpdateMirror()
    {
        showMirror *= -1;
        PlayerPrefs.SetInt("showMirror", showMirror);

        if (showMirror == 1)
            mirrorCanvas.GetComponent<CanvasGroup>().alpha = 1;
        else
            mirrorCanvas.GetComponent<CanvasGroup>().alpha = 0;
    }

    void EnableHUD()
    {
        GetComponent<CanvasGroup>().alpha = 1;
    }
}
