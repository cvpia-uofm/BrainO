using Assets.Models.Interfaces;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    readonly float rotateSpeed = 20;

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
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
            {
                float rotateX = Input.GetAxis("Mouse X") * rotateSpeed;
                float rotateY = Input.GetAxis("Mouse Y") * rotateSpeed;

                transform.position = new Vector3(transform.localPosition.x + rotateX, transform.localPosition.y + rotateY, 70f);
            }



        }
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
            CameraDefaultPosition();
            if (global.ROIActivated && !global.DoubleEscape_ROI_Deactivation)
            {
                Toggle_Region_obj("L", true);
                Toggle_Region_obj("R", true);
                return;
            }
            ActivateView("default");
            Cancel();
        }
    }

    void CameraDefaultPosition()
    {
        transform.localPosition = new Vector3(30f, 70f, 70f);
    }

    void CamaeraDefaultRotation()
    {
        transform.rotation = new Quaternion(0, 0, 0, 0);
        var lbls = Points_obj_holder.GetComponentsInChildren<TMP_Text>(true);
        foreach(var lbl in lbls)
        {
            lbl.transform.localRotation = new Quaternion(0, 180, 0, 0);
        }
        
    }

    void Rotate_Camera(float X_rot, float Y_rot)
    {
        transform.Rotate(X_rot, Y_rot, 0);
        OnBrainRotate(X_rot, Y_rot);
    }

    void SideMenuController_TakeFigure(string path)
    {
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


    public void SnapScreenshot()
    {
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
        if (!global.CorrelationActivated)
        {
            var reg_hemph = Points_obj_holder.GetComponentsInChildren<Transform>(true).
                    Where(a => a.name.StartsWith(hemph, StringComparison.CurrentCultureIgnoreCase) && a.name != "ROI_factor");
            if (!global.ROIActivated)
            {
                foreach (Transform reg in reg_hemph)
                {
                    reg.gameObject.SetActive(state);
                }
            }
            else
            {
                foreach (var roi in global.Current_rOIs)
                {
                    Transform reg_roi = reg_hemph.SingleOrDefault(a => a.name.Equals(roi.Region, StringComparison.CurrentCultureIgnoreCase));
                    if (reg_roi != null)
                    {
                        reg_roi.gameObject.SetActive(state);
                    }
                }
            } 
        }
    }
}
