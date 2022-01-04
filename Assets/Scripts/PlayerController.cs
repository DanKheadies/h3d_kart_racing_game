using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Drive drive;
    Quaternion lastRotation;
    Vector3 lastPosition;

    float lastTimeMoving = 0;

    // Start is called before the first frame update
    void Start()
    {
        drive = GetComponent<Drive>();
        GetComponent<Ghost>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");

        if (drive.rb.velocity.magnitude > 1 || !RaceMonitor.racing)
            lastTimeMoving = Time.time;

        RaycastHit hit;
        if (Physics.Raycast(drive.rb.gameObject.transform.position, -Vector3.up, out hit, 10))
        {
            if (hit.collider.gameObject.tag == "road")
            {
                lastPosition = drive.rb.gameObject.transform.position;
                lastRotation = drive.rb.gameObject.transform.rotation;
            }
        }

        if (Time.time > lastTimeMoving + 4)
        {
            drive.rb.gameObject.transform.position = lastPosition;
            drive.rb.gameObject.transform.rotation = lastRotation;
            drive.rb.gameObject.layer = 8;
            GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 3);
        }

        if (!RaceMonitor.racing) 
            a = 0;

        drive.Go(a, s, b);
        drive.CheckForSkid();
        drive.CalculateEngineSound();
    }

    void ResetLayer()
    {
        drive.rb.gameObject.layer = 0;
        GetComponent<Ghost>().enabled = false;
    }
}
