using UnityEngine;
using UnityEngine.UI;

public class NameUIController : MonoBehaviour
{
    public Renderer carRend;
    public Text playerName;
    public Transform target;

    CanvasGroup canvasGroup;

    int showName = -1;

    // White (text), White (small outline), Black (big outline)
    // Color (text), Black (small outline), White (big outline)

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(GameObject.Find("GUI").GetComponent<Transform>(), false);
        playerName = GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (PlayerPrefs.HasKey("showName"))
            showName = PlayerPrefs.GetInt("showName");

        canvasGroup.alpha = 0;

        if (target.parent.GetComponent<Drive>().isGreen)
            playerName.color = new Color(4f / 255f, 142f / 255f, 0f / 255f, 255f / 255f);
        else if (target.parent.GetComponent<Drive>().isOrange)
            playerName.color = new Color(204f / 255f, 95f / 255f, 0f / 255f, 255f / 255f);
        else if (target.parent.GetComponent<Drive>().isPurple)
            playerName.color = new Color(72f / 255f, 0f / 255f, 204f / 255f, 255f / 255f);
        else if (target.parent.GetComponent<Drive>().isYellow)
            playerName.color = new Color(173f / 255f, 160f / 255f, 1f / 255f, 255f / 255f);
        else
            playerName.color = Color.black;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            UpdateName();
    }

    private void LateUpdate()
    {
        if (carRend == null || !RaceMonitor.racing) 
            return;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool carInView = GeometryUtility.TestPlanesAABB(planes, carRend.bounds);
        canvasGroup.alpha = carInView && showName == 1 ? 1 : 0;
        transform.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.2f);
    }

    void UpdateName()
    {
        showName *= -1;
        PlayerPrefs.SetInt("showName", showName);

        if (showName == 1)
            canvasGroup.alpha = 1;
        else
            canvasGroup.alpha = 0;
    }
}
