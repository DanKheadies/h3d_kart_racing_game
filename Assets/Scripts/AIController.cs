using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Circuit circuit;

    Drive drive;
    GameObject tracker;
    Vector3 target;
    //Vector3 nextTarget;

    public float accelSensitivity = 0.3f;
    public float brakingSensitivity = 1.1f;
    public float steeringSensitivity = 0.01f;

    //bool isJump = false;
    float lastTimeMoving = 0;
    float lookAhead = 10;
    float totalDistanceToTarget;
    int currentTrackerWP = 0;
    int currentWP = 0;

    // Start is called before the first frame update
    void Start()
    {
        drive = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        //nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, drive.rb.gameObject.transform.position);

        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = drive.rb.gameObject.transform.position;
        tracker.transform.rotation = drive.rb.gameObject.transform.rotation;

        this.GetComponent<Ghost>().enabled = false;
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

        //Vector3 nextLocalTarget = drive.rb.gameObject.transform.InverseTransformPoint(nextTarget);
        //float distanceToTarget = Vector3.Distance(target, drive.rb.gameObject.transform.position);

        //float nextTargetAngle = Mathf.Atan2(nextLocalTarget.x, nextLocalTarget.z) * Mathf.Rad2Deg;

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
            this.GetComponent<Ghost>().enabled = true;
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

        //float distanceFactor = distanceToTarget / totalDistanceToTarget;
        //float speedFactor = drive.currentSpeed / drive.maxSpeed;

        //float accel = Mathf.Lerp(accelSensitivity, 1, distanceFactor);
        //float brake = Mathf.Lerp(
        //    (-1 - Mathf.Abs(nextTargetAngle)) * brakingSensitivity, 
        //    1 + speedFactor, 
        //    1 - distanceFactor
        //);

        //if (Mathf.Abs(nextTargetAngle) > 20)
        //{
        //    brake += 0.8f;
        //    accel -= 0.8f;
        //}

        //if (isJump)
        //{
        //    accel = 1;
        //    brake = 0;
        //}

        drive.Go(accel, steer, brake);

        // Threshold; how close the car will get to that location of the waypoint
        // Make larger if car circles waypoint w/ making contact, e.g. 2 -> 5
        //if (distanceToTarget < 4) 
        //{
        //    currentWP++;
        //    if (currentWP >= circuit.waypoints.Length)
        //        currentWP = 0;

        //    target = circuit.waypoints[currentWP].transform.position;
        //    if (currentWP == circuit.waypoints.Length - 1)
        //        nextTarget = circuit.waypoints[0].transform.position;
        //    else
        //        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        //    totalDistanceToTarget = Vector3.Distance(target, drive.rb.gameObject.transform.position);

        //    if (drive.rb.gameObject.transform.InverseTransformPoint(target).y > 5)
        //        isJump = true;
        //    else 
        //        isJump = false;
        //}

        drive.CalculateEngineSound();
        drive.CheckForSkid();
    }

    void ResetLayer()
    {
        drive.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
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
