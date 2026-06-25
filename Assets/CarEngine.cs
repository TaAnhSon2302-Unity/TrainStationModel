using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    [Header("Path")]
    public Transform path;
    public List<Transform> nodes = new List<Transform>();
    public int currentNode = 0;

    [Header("Steering")]
    public float maxSteerAngle = 30f;
    public float lookAheadDistance = 5f;

    [Header("Speed")]
    public float maxMotorTorque = 80f;
    public float maxSpeed = 100f;
    public float currentSpeed;

    [Header("Waypoint")]
    public float waypointRadius = 3f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Sensors")]
    public float sensorLength = 3f;
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
    public float sideSensorPosition = 0.5f;
    public float frontSensorAngle = 30f;
    private bool avoiding = false;
    private float lastAvoidDirection = 1f;
    private float lastAvoidMultiplier = 0f;

    [Header("Avoidance")]
    public float avoidTime = 1.5f;
    private float avoidTimer = 0f;

    void Start()
    {
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes.Clear();
        foreach (Transform t in pathTransforms)
        {
            if (t != path)
                nodes.Add(t);
        }
    }

    private void FixedUpdate()
    {
        if (nodes.Count < 2) return;
        Sensor();
        CheckWaypointDistance();
        ApplySteer();
        Drive();
    }

    private void ApplySteer()
    {
        if (avoiding) return;

        Transform current = nodes[currentNode];
        Transform next = nodes[(currentNode + 1) % nodes.Count];

        Vector3 closestPoint = ClosestPointOnLine(current.position, next.position, transform.position);
        Vector3 pathDirection = (next.position - current.position).normalized;
        Vector3 targetPoint = closestPoint + pathDirection * lookAheadDistance;
        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);

        float steer = (localTarget.x / localTarget.magnitude) * maxSteerAngle;
        steer = Mathf.Clamp(steer, -maxSteerAngle, maxSteerAngle);

        frontLeftWheelCollider.steerAngle = steer;
        frontRightWheelCollider.steerAngle = steer;
    }

    private void Drive()
    {
        currentSpeed = 2f * Mathf.PI * frontLeftWheelCollider.radius * frontLeftWheelCollider.rpm * 60f / 1000f;

        float steerAmount = Mathf.Abs(frontLeftWheelCollider.steerAngle);
        float torqueFactor = Mathf.Lerp(1f, 0.4f, steerAmount / maxSteerAngle);
        float torque = maxMotorTorque * torqueFactor;

        if (currentSpeed < maxSpeed)
        {
            rearLeftWheelCollider.motorTorque = torque;
            rearRightWheelCollider.motorTorque = torque;
        }
        else
        {
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
        }
    }

    private void CheckWaypointDistance()
    {
        int nextNode = (currentNode + 1) % nodes.Count;
        float distance = Vector3.Distance(transform.position, nodes[nextNode].position);
        if (distance < waypointRadius)
            currentNode = nextNode;
    }

    private Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    private void Sensor()
    {
        RaycastHit hit;

        bool detectedObstacle = false;
        float avoidMultiplier = 0f;

        Vector3 sensorStartPos =
            transform.position +
            transform.forward * frontSensorPosition.z +
            transform.right * frontSensorPosition.x +
            transform.up * frontSensorPosition.y;

        Vector3 rightSensorPos = sensorStartPos + transform.right * sideSensorPosition;
        Vector3 leftSensorPos = sensorStartPos - transform.right * sideSensorPosition;

        // ==========================
        // FRONT RIGHT
        // ==========================
        if (Physics.Raycast(rightSensorPos, transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(rightSensorPos, hit.point, Color.red);
            if (hit.collider.CompareTag("Obstacle"))
            {
                detectedObstacle = true;
                avoidMultiplier = -1f;
                lastAvoidDirection = -1f;
            }
        }
        else
        {
            Debug.DrawLine(rightSensorPos, rightSensorPos + transform.forward * sensorLength, Color.green);

            Vector3 rightAngleDir = Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward;

            if (Physics.Raycast(rightSensorPos, rightAngleDir, out hit, sensorLength))
            {
                Debug.DrawLine(rightSensorPos, hit.point, Color.yellow);
                if (hit.collider.CompareTag("Obstacle"))
                {
                    detectedObstacle = true;
                    avoidMultiplier = -0.5f;
                    lastAvoidDirection = -1f;
                }
            }
            else
            {
                Debug.DrawLine(rightSensorPos, rightSensorPos + rightAngleDir * sensorLength, Color.green);
            }
        }

        // ==========================
        // FRONT LEFT
        // ==========================
        if (Physics.Raycast(leftSensorPos, transform.forward, out hit, sensorLength))
        {
            Debug.DrawLine(leftSensorPos, hit.point, Color.red);
            if (hit.collider.CompareTag("Obstacle"))
            {
                detectedObstacle = true;
                avoidMultiplier = 1f;
                lastAvoidDirection = 1f;
            }
        }
        else
        {
            Debug.DrawLine(leftSensorPos, leftSensorPos + transform.forward * sensorLength, Color.green);

            Vector3 leftAngleDir = Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward;

            if (Physics.Raycast(leftSensorPos, leftAngleDir, out hit, sensorLength))
            {
                Debug.DrawLine(leftSensorPos, hit.point, Color.yellow);
                if (hit.collider.CompareTag("Obstacle"))
                {
                    detectedObstacle = true;
                    avoidMultiplier = 0.5f;
                    lastAvoidDirection = 1f;
                }
            }
            else
            {
                Debug.DrawLine(leftSensorPos, leftSensorPos + leftAngleDir * sensorLength, Color.green);
            }
        }

        // ==========================
        // FRONT CENTER
        // ==========================
        if (!detectedObstacle)
        {
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
            {
                Debug.DrawLine(sensorStartPos, hit.point, Color.red);
                if (hit.collider.CompareTag("Obstacle"))
                {
                    detectedObstacle = true;
                    avoidMultiplier = lastAvoidDirection;
                }
            }
            else
            {
                Debug.DrawLine(sensorStartPos, sensorStartPos + transform.forward * sensorLength, Color.green);
            }
        }

        // ==========================
        // APPLY AVOIDANCE
        // ==========================
        if (detectedObstacle)
        {
            avoiding = true;
            avoidTimer = avoidTime;
            lastAvoidMultiplier = avoidMultiplier;
        }
        else if (avoidTimer > 0f)
        {
            avoidTimer -= Time.fixedDeltaTime;
            avoiding = avoidTimer > 0f;
            avoidMultiplier = lastAvoidMultiplier; // giữ hướng né cũ
        }
        else
        {
            avoiding = false;
        }

        if (avoiding)
        {
            float steer = maxSteerAngle * avoidMultiplier;
            steer = Mathf.Clamp(steer, -maxSteerAngle, maxSteerAngle);
            frontLeftWheelCollider.steerAngle = steer;
            frontRightWheelCollider.steerAngle = steer;
        }
    }
}