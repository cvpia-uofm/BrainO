using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Models.Interfaces;
using Zenject;
using System;
using TMPro;
using System.Linq;

public class SettingsController : MonoBehaviour
{
    const string Back_col_str = "Background Color";
    const string Lbl_col_str = "Label Color";
    const string RH_col_str = "RH Region Color";
    const string LH_col_str = "LH Region Color";
    const string Mesh_col_str = "Mesh Color";

    public Text Selection_lbl;
    public ColorPicker ColorPicker;
    public Camera MainCamera;
    public GameObject Points;
    public GameObject Cerebellum;
    public GameObject Brainstem;
    public GameObject Left_Hemp;
    public GameObject Right_Hemp;

    [Inject]
    readonly IGlobal global;

    public void Change_Background_Color() { Selection_lbl.text = Back_col_str; show_Color_picker(); set_Color_to_Color_picker(global.Back_col); }
    public void Change_Lbl_Color() { Selection_lbl.text = Lbl_col_str; show_Color_picker(); set_Color_to_Color_picker(global.Lbl_col); }
    public void Change_RH_reg_Color() { Selection_lbl.text = RH_col_str; show_Color_picker(); set_Color_to_Color_picker(global.RH_reg_col); }
    public void Change_LH_reg_Color() { Selection_lbl.text = LH_col_str; show_Color_picker(); set_Color_to_Color_picker(global.LH_reg_col); }
    public void Change_Mesh_Color() { Selection_lbl.text = Mesh_col_str; show_Color_picker(); set_Color_to_Color_picker(global.Mesh_col); }

    void Start()
    {
        ColorPicker.onValueChanged.AddListener(color =>
        {
            if (global.Settings_Activated)
            {
                switch (Selection_lbl.text)
                {
                    case Back_col_str:
                        MainCamera.backgroundColor = ColorPicker.CurrentColor;
                        global.Back_col = ColorPicker.CurrentColor;
                        break;
                    case Lbl_col_str:
                        var reg_lbls = Points.GetComponentsInChildren<TMP_Text>();
                        foreach (var reg_lbl in reg_lbls)
                        {
                            reg_lbl.color = ColorPicker.CurrentColor;
                        }
                        break;
                    case RH_col_str:
                        var regs_rh = Points.GetComponentsInChildren<Transform>().Where(a => a.name.StartsWith("R", StringComparison.CurrentCultureIgnoreCase));
                        foreach(var reg_rh in regs_rh)
                        {
                            MaterialPropertyBlock props_rh = new MaterialPropertyBlock();
                            props_rh.SetColor("_Color", ColorPicker.CurrentColor);
                            reg_rh.gameObject.GetComponent<Renderer>().SetPropertyBlock(props_rh);
                            global.RH_reg_col = ColorPicker.CurrentColor;
                        }
                        break;
                    case LH_col_str:
                        var regs_lh = Points.GetComponentsInChildren<Transform>().Where(a => a.name.StartsWith("L", StringComparison.CurrentCultureIgnoreCase));
                        foreach (var reg_lh in regs_lh)
                        {
                            MaterialPropertyBlock props_lh = new MaterialPropertyBlock();
                            props_lh.SetColor("_Color", ColorPicker.CurrentColor);
                            reg_lh.gameObject.GetComponent<Renderer>().SetPropertyBlock(props_lh);
                            global.LH_reg_col = ColorPicker.CurrentColor;
                        }
                        break;
                    case Mesh_col_str:
                        MaterialPropertyBlock props_mesh = new MaterialPropertyBlock();
                        props_mesh.SetColor("_Color", ColorPicker.CurrentColor);
                        Cerebellum.GetComponent<Renderer>().SetPropertyBlock(props_mesh);
                        Brainstem.GetComponent<Renderer>().SetPropertyBlock(props_mesh);
                        Left_Hemp.GetComponent<Renderer>().SetPropertyBlock(props_mesh);
                        Right_Hemp.GetComponent<Renderer>().SetPropertyBlock(props_mesh);
                        break;

                } 
            }
        });
    }

    void Update()
    {
        if (SelectedItem == null)
        {
            ColorPicker.gameObject.SetActive(false);
            Selection_lbl.text = "";
        }
    }
    void show_Color_picker()
    {
        if ((ColorPicker.gameObject != null) && !ColorPicker.gameObject.activeSelf)
        {
            ColorPicker.gameObject.SetActive(true);
        }
    }

    void set_Color_to_Color_picker(Color color)
    {
        ColorPicker.CurrentColor = color;
    }

    GameObject SelectedItem => EventSystem.current.currentSelectedGameObject;

}
