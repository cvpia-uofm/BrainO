using Assets.Models.Interfaces;
using UnityEngine;
using Zenject;

public class BrainController : MonoBehaviour
{
    const float rotateSpeed = 200;

    public delegate void RotationAction(float X, float Y);

    public static event RotationAction OnBrainRotate;

    public GameObject Cerebellum;
    public GameObject Brainstem;
    public GameObject Left_Hemp;
    public GameObject Right_Hemp;

    [Inject]
    readonly IGlobal global;

    void Start()
    {
        global.Mesh_col = Left_Hemp.GetComponent<Renderer>().material.color;
    }

    void Update()
    {
        //Rotation();
        Scaling(); 
    }

    void Scaling()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && Input.GetKey(KeyCode.LeftControl))
        {
            if (!gameObject.transform.localScale.Equals(new Vector3(100, 100, 100)))
            {
                gameObject.transform.localScale += new Vector3(1f, 1f, 1f);
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f && Input.GetKey(KeyCode.LeftControl))
        {
            if (!gameObject.transform.localScale.Equals(new Vector3(0, 0, 0)))
            {
                gameObject.transform.localScale -= new Vector3(1f, 1f, 1f);
            }
        }
    }

    void Rotation()
    {
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Mathf.Deg2Rad;
            float rotateY = Input.GetAxis("Mouse Y") * rotateSpeed * Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotateX);
            transform.Rotate(Vector3.right, -rotateY);

            OnBrainRotate(rotateX, rotateY);
        }

        

        
    }
}