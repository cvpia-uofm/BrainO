using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Camera Camera_fig;
    public Text Status_lbl;

    bool ReadyToTakeScreenShot;
    string path_to_save;

    void Awake()
    {
        SideMenuController.TakeFiguere += SideMenuController_TakeFiguere;
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
