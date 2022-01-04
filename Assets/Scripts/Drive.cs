using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour
{
    public AudioSource highAccel;
    public AudioSource skidSound;
    public GameObject brakeLight;
    public GameObject playerNamePrefab;
    public GameObject[] wheels;
    public ParticleSystem smokePrefab;
    public Renderer carMesh;
    public Rigidbody rb;
    public Transform skidTrailPrefab;
    public WheelCollider[] wheelColis;

    ParticleSystem[] skidSmoke = new ParticleSystem[4];
    Transform[] skidTrails = new Transform[4];

    public bool isGreen;
    public bool isOrange;
    public bool isPurple;
    public bool isYellow;
    public float currentSpeed { get { return rb.velocity.magnitude * gearLength; } }
    public float gearLength = 3;
    public float highPitch = 6f;
    public float lowPitch = 1f;
    public float maxBrakeTorque = 500;
    public float maxSpeed = 200;
    public float maxSteerAngle = 30;
    public float torque = 200;
    public int numGears = 5;

    float currentGearPerc;
    float rpm;
    int currentGear = 1;
    string[] aiNames = { "Dominus", "Cavax", "Goblin", "Orion", "Fletchner"};

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }

        brakeLight.SetActive(false);

        GameObject playerName = Instantiate(playerNamePrefab);
        playerName.GetComponent<NameUIController>().target = rb.gameObject.transform;

        if (GetComponent<AIController>().enabled)
            playerName.GetComponent<Text>().text = aiNames[Random.Range(0, aiNames.Length)];
        else 
            playerName.GetComponent<Text>().text = "Darrow";

        playerName.GetComponent<NameUIController>().carRend = carMesh;
    }

    public void CalculateEngineSound()
    {
        float gearPercentage = (1 / (float)numGears);
        float targetGearFactor = Mathf.InverseLerp(
            gearPercentage * currentGear, 
            gearPercentage * (currentGear + 1), 
            Mathf.Abs(currentSpeed / maxSpeed)
        );

        currentGearPerc = Mathf.Lerp(
            currentGearPerc, 
            targetGearFactor, 
            Time.deltaTime * 5f // Quicker b/t gears: increase float
        );

        var gearNumFactor = currentGear / (float)numGears;
        rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
        float downGearMax = (1 / (float)numGears) * currentGear;

        if (currentGear > 0 && speedPercentage < downGearMax)
            currentGear--;
        if (speedPercentage > upperGearMax && currentGear < (numGears - 1))
            currentGear++;

        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
        highAccel.pitch = Mathf.Min(highPitch, pitch) * 0.25f; // drops the sound down a quarter

        // To reduce world sound of engine (so other cars don't sound as loud)
        // Decrease the (3D Sound Settings) Min and Max Distance for that Audio Source
        // Create a smaller ranger, i.e. 50 - 200 -> 50 - 100
    }

    public void StartSkidTrail(int i)
    {
        if (skidTrails[i] == null)
            skidTrails[i] = Instantiate(skidTrailPrefab);

        skidTrails[i].parent = wheelColis[i].transform;
        skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0);
        skidTrails[i].localPosition = -Vector3.up * wheelColis[i].radius;
    }

    public void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null) return;
        Transform holder = skidTrails[i];
        skidTrails[i] = null;
        holder.parent = null;
        holder.rotation = Quaternion.Euler(90, 0, 0);
        Destroy(holder.gameObject, 30);
    }

    public void CheckForSkid()
    {
        int numSkidding = 0;
        for (int i = 0; i< 4; i++)
        {
            WheelHit wheelHit;
            wheelColis[i].GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.forwardSlip) >= 0.4f ||
                Mathf.Abs(wheelHit.sidewaysSlip) >= 0.4f)
            {
                numSkidding++;
                if (!skidSound.isPlaying)
                    skidSound.Play();

                skidSmoke[i].transform.position = wheelColis[i].transform.position - wheelColis[i].transform.up * wheelColis[i].radius;
                skidSmoke[i].Emit(1);
            }
        }

        if (numSkidding == 0 && skidSound.isPlaying)
            skidSound.Stop();
    }

    public void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        if (brake > 0)
            brakeLight.SetActive(true);
        else
            brakeLight.SetActive(false);

        float thrustTorque = 0;
        if (currentSpeed < maxSpeed)
            thrustTorque = accel * torque;

        for (int i = 0; i < 4; i++)
        {
            wheelColis[i].motorTorque = thrustTorque;

            if (i < 2)
                wheelColis[i].steerAngle = steer;
            else
                wheelColis[i].brakeTorque = brake;

            Quaternion quat;
            Vector3 position;
            wheelColis[i].GetWorldPose(out position, out quat);
            wheels[i].transform.position = position;
            wheels[i].transform.rotation = quat;
        }
    }
}
