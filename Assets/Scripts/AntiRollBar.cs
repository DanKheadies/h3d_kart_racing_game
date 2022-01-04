using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public GameObject centerOfMass;
    public WheelCollider wheelLFront;
    public WheelCollider wheelRFront;
    public WheelCollider wheelLBack;
    public WheelCollider wheelRBack;

    Rigidbody rb;

    public float antiRoll = 5000;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GroundWheels(wheelLFront, wheelRFront);
        GroundWheels(wheelLBack, wheelRBack);
    }

    void GroundWheels(WheelCollider WL, WheelCollider WR) 
    {
        WheelHit hit;
        float travelL = 1;
        float travelR = 1;

        bool groundedL = WL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WL.transform.InverseTransformPoint(hit.point).y - WL.radius) / WL.suspensionDistance;

        bool groundedR = WR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-WR.transform.InverseTransformPoint(hit.point).y - WR.radius) / WR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            rb.AddForceAtPosition(WL.transform.up * -antiRollForce, WL.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(WR.transform.up * antiRollForce, WR.transform.position);
    }
}
