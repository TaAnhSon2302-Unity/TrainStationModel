using UnityEngine;

public class TrainScript : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    public float speed = 10f;

    private Quaternion originalRotation;
    private float fixedY;

    private void Start()
    {
        originalRotation = transform.rotation;
        fixedY = transform.position.y;
    }

    private void Update()
    {
        Vector3 direction = endPoint.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.position += direction * speed * Time.deltaTime;

        // Khóa Y
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        if (Vector3.Distance(transform.position, endPoint.position) < 1f)
        {
            ResetTrain();
        }
    }

    private void ResetTrain()
    {
        transform.position = startPoint.position;
        transform.rotation = originalRotation;
    }
}