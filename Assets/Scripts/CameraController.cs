using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera_fig;

    bool ReadyToTakeScreenShot;

    void Awake()
    {
        SideMenuController.TakeFiguere += SideMenuController_TakeFiguere;
    }

    void SideMenuController_TakeFiguere(string path)
    {
        Camera_fig.targetTexture = RenderTexture.GetTemporary(1000, 1000, 16);
        ReadyToTakeScreenShot = true;
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

            byte[] vs = renderResult.EncodeToPNG();

            System.IO.File.WriteAllBytes(Application.dataPath + "/test.png", vs);

            RenderTexture.ReleaseTemporary(renderTexture);
            Camera_fig.targetTexture = null;
        }
    }


}
