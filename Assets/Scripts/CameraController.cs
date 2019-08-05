﻿using Assets.Models.Interfaces;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static SideMenuController;

public class CameraController : MonoBehaviour
{
    public Camera MainCamera;
    public Text Status_lbl;
    public Canvas TitleCanvas;
    public Canvas ListViewCanvas;
    public Canvas SideMenuCanvas;
    public RectTransform ROI_ListView_Panel;
    public RectTransform CameraCtrlPanel;
    public RectTransform FigureCtrlPanel;
    public GameObject Points_obj_holder;

    Dictionary<string, bool> activeView = new Dictionary<string, bool>()
    {
        { "left", false },
        { "right", false},
        { "top", false},
        { "back", false},
        { "default", true}
    };

    public delegate void RotationAction(float X, float Y);

    public static event RotationAction OnBrainRotate;

    bool ReadyToTakeScreenShot;
    string path_to_save;
    float speed = 200;

    [Inject]
    readonly IGlobal global;

    void Awake()
    {
        SideMenuController.TakeFigure += SideMenuController_TakeFigure;
    }

    void Start()
    {
        global.Back_col = MainCamera.backgroundColor;
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
            CamaeraDefaultRotation();
            ActivateView("default");
            Cancel();
        }
    }

    void CamaeraDefaultRotation()
    {
        transform.rotation = new Quaternion(0, 0, 0, 0);
        OnBrainRotate(0, 0);
    }

    void Rotate_Camera(float X_rot, float Y_rot)
    {
        transform.Rotate(X_rot, Y_rot, 0);
        OnBrainRotate(X_rot, Y_rot);
    }

    void SideMenuController_TakeFigure(string path)
    {
        //Camera_fig.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        //path_to_save = path;
        //ReadyToTakeScreenShot = true;
        //Status_lbl.gameObject.SetActive(true);
        TitleCanvas.gameObject.SetActive(false);
        ListViewCanvas.gameObject.SetActive(false);
        CameraCtrlPanel.gameObject.SetActive(false);
        SideMenuCanvas.gameObject.SetActive(false);
        FigureCtrlPanel.gameObject.SetActive(true);

        if (global.ROIActivated)
        {
            ROI_ListView_Panel.gameObject.SetActive(true);
        }
    }

    void OnPostRender()
    {
        //SnapScreenshot();
    }

    public void SnapScreenshot()
    {
        //if (ReadyToTakeScreenShot)
        //{
        //    ReadyToTakeScreenShot = false;

        //    RenderTexture renderTexture = Camera_fig.targetTexture;
        //    Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        //    Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
        //    renderResult.ReadPixels(rect, 0, 0);

        //    byte[] vs = renderResult.EncodeToJPG();

        //    System.IO.File.WriteAllBytes(path_to_save, vs);

        //    RenderTexture.ReleaseTemporary(renderTexture);
        //    Camera_fig.targetTexture = null;

        //}
        var extensions = new[] {
            new ExtensionFilter("JPEG Files", "jpg" ),
        };
        var path = FileBrowser(extensions, FileBrowserOptions.SaveFile);
        if (string.IsNullOrWhiteSpace(path[0]))
            return;
        ScreenCapture.CaptureScreenshot(path[0]);
    }

    public void Cancel()
    {
        TitleCanvas.gameObject.SetActive(true);
        ListViewCanvas.gameObject.SetActive(true);
        CameraCtrlPanel.gameObject.SetActive(true);
        SideMenuCanvas.gameObject.SetActive(true);
        FigureCtrlPanel.gameObject.SetActive(false);
        ROI_ListView_Panel.gameObject.SetActive(false);
    }

    public void LeftHemphView()
    {
        if (!activeView["left"])
        {
            CamaeraDefaultRotation();
            Rotate_Camera(0, -109f);
            ActivateView("left");

            Toggle_Region_obj("R", false);
            Toggle_Region_obj("L", true);
        }
    }
    public void RightHemphView()
    {
        if (!activeView["right"])
        {
            CamaeraDefaultRotation();
            Rotate_Camera(0, 63f);
            ActivateView("right");

            Toggle_Region_obj("L", false);
            Toggle_Region_obj("R", true);
        }
    }

    void DeActivateViews(string view)
    {
        foreach (var kvp in activeView.ToArray())
        {
            if (kvp.Key != view)
            {
                activeView[kvp.Key] = false;
            }
        }
    }
    void ActivateView(string view)
    {
        activeView[view] = true;
        DeActivateViews(view);
    }

    void Toggle_Region_obj(string hemph, bool state)
    {
        var reg_hemph = Points_obj_holder.GetComponentsInChildren<Transform>(true).Where(a => a.name.StartsWith(hemph, StringComparison.CurrentCultureIgnoreCase));
        
        foreach(Transform reg in reg_hemph)
        {
            reg.gameObject.SetActive(state);
        }
    }
}
