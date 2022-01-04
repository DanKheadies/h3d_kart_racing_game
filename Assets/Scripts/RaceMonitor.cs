using System.Collections;
using UnityEngine;

public class RaceMonitor : MonoBehaviour
{
    public GameObject[] countdownItems;

    public static bool racing = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject g in countdownItems)
            g.SetActive(false);

        StartCoroutine(PlayCountdown());
    }

    IEnumerator PlayCountdown()
    {
        yield return new WaitForSeconds(2);
        foreach(GameObject g in countdownItems)
        {
            g.SetActive(true);
            yield return new WaitForSeconds(1);
            g.SetActive(false);
        }
        racing = true;
    }
}
