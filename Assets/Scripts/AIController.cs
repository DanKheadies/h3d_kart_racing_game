using UnityEngine;

public class AIController : MonoBehaviour
{
    public Circuit circuit;

    Drive drive;
    GameObject tracker;
    Vector3 target;

    public float accelSensitivity = 0.3f;
    public float brakingSensitivity = 1.1f;
    public float steeringSensitivity = 0.01f;

    float lastTimeMoving = 0;
    float lookAhead = 10;
    float totalDistanceToTarget;
    int currentTrackerWP = 0;
    int currentWP = 0;

    // Start is called before the first frame update
    void Start()
    {
        drive = GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, drive.rb.gameObject.transform.position);

        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = drive.rb.gameObject.transform.position;
        tracker.transform.rotation = drive.rb.gameObject.transform.rotation;

        GetComponent<Ghost>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!RaceMonitor.racing)
        {
            lastTimeMoving = Time.time;
            drive.Go(0, 0, 0);
            return;
        }

        ProgressTracker();
        Vector3 localTarget;
        float targetAngle;

        if (drive.rb.velocity.magnitude > 1)
            lastTimeMoving = Time.time;

        if (Time.time > lastTimeMoving + 4) // should be 1 second longer than flip car (which is 3 atm)
        {
            drive.rb.gameObject.transform.position = 
                circuit.waypoints[currentTrackerWP].transform.position + 
                Vector3.up * 2 + // puts car ~2m up
                new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)); 
            tracker.transform.position = circuit.waypoints[currentTrackerWP].transform.position;
            drive.rb.gameObject.layer = 8;
            GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 3);
        }

        if (Time.time < drive.rb.GetComponent<AvoidDetector>().avoidTime)
            localTarget = tracker.transform.right * drive.rb.GetComponent<AvoidDetector>().avoidPath;
        else
            localTarget = drive.rb.gameObject.transform.InverseTransformPoint(tracker.transform.position);

        targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(drive.currentSpeed);
        float speedFactor = drive.currentSpeed / drive.maxSpeed;
        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0, 90);
        float cornerFactor = corner / 90;

        float brake = 0;
        if (corner > 10 && speedFactor > 0.25f)
            brake = Mathf.Lerp(0, 1 + speedFactor * brakingSensitivity, cornerFactor);

        float accel = 1f;
        if (corner > 20 && speedFactor > 0.25f)
            accel = Mathf.Lerp(0, 1 * accelSensitivity, 1 - cornerFactor);

        drive.Go(accel, steer, brake);

        drive.CalculateEngineSound();
        drive.CheckForSkid();
    }

    void ResetLayer()
    {
        drive.rb.gameObject.layer = 0;
        GetComponent<Ghost>().enabled = false;
    }

    void ProgressTracker()
    {
        //Debug.DrawLine(drive.rb.gameObject.transform.position, tracker.transform.position);

        if (Vector3.Distance(drive.rb.gameObject.transform.position, tracker.transform.position) > lookAhead)
            return;

        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1f); // speed of tracker

        if (Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWP].transform.position) < 1)
        {
            currentTrackerWP++;
            if (currentTrackerWP >= circuit.waypoints.Length)
                currentTrackerWP = 0;
        }
    }
}
