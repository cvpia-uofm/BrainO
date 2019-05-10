using UnityEngine;

public class BrainController : MonoBehaviour
{
    private const float rotateSpeed = 200;

    public delegate void RotationAction(float X, float Y);

    public static event RotationAction OnBrainRotate;

    private void Start()
    {
    }

    private void Update()
    {
        Rotation();
        Scaling();
    }

    private void Scaling()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (!gameObject.transform.localScale.Equals(new Vector3(100, 100, 100)))
            {
                gameObject.transform.localScale += new Vector3(1f, 1f, 1f);
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (!gameObject.transform.localScale.Equals(new Vector3(40, 40, 40)))
            {
                gameObject.transform.localScale -= new Vector3(1f, 1f, 1f);
            }
        }
    }

    private void Rotation()
    {
        if (Input.GetMouseButton(0))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Mathf.Deg2Rad;
            float rotateY = Input.GetAxis("Mouse Y") * rotateSpeed * Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotateX);
            transform.Rotate(Vector3.right, -rotateY);

            OnBrainRotate(rotateX, rotateY);
        }
    }
}