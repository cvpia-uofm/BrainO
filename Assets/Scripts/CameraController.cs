using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Camera MainCamera;
    public Camera Camera_fig;
    public Text Status_lbl;

    public delegate void RotationAction(float X, float Y);

    public static event RotationAction OnBrainRotate;

    bool ReadyToTakeScreenShot;
    string path_to_save;
    float speed = 200;

    void Awake()
    {
        SideMenuController.TakeFiguere += SideMenuController_TakeFiguere;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Rotate_Camera(0, speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Rotate_Camera(0, -speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Rotate_Camera(speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Rotate_Camera(-speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        
        
    }
    void Rotate_Camera(float X_rot, float Y_rot)
    {
        transform.Rotate(X_rot, Y_rot, 0);
        OnBrainRotate(X_rot, Y_rot);
    }
    void SideMenuController_TakeFiguere(string path)
    {
        
        Camera_fig.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        path_to_save = path;
        ReadyToTakeScreenShot = true;
        Status_lbl.gameObject.SetActive(true);


    }

    void OnPostRender()
    {
        SnapScreenshot();
    }

    void SnapScreenshot()
    {
        if (ReadyToTakeScreenShot)
        {
            ReadyToTakeScreenShot = false;

            RenderTexture renderTexture = Camera_fig.targetTexture;
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] vs = renderResult.EncodeToJPG();

            System.IO.File.WriteAllBytes(path_to_save, vs);

            RenderTexture.ReleaseTemporary(renderTexture);
            Camera_fig.targetTexture = null;
            
        }
    }
}
