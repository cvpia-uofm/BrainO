using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Models.Interfaces;
using Zenject;
using System;

public class SettingsController : MonoBehaviour
{
    public Text Selection_lbl;
    public ColorPicker ColorPicker;

    [Inject]
    readonly IGlobal global;

    public void Change_Background_Color() { Selection_lbl.text = "Background Color";  show_Color_picker(); set_Color_to_Color_picker(global.Back_col); }
    public void Change_Lbl_Color() { Selection_lbl.text = "Label Color"; show_Color_picker(); set_Color_to_Color_picker(global.Lbl_col); }
    public void Change_RH_reg_Color() { Selection_lbl.text = "RH Region Color"; show_Color_picker(); set_Color_to_Color_picker(global.RH_reg_col); }
    public void Change_LH_reg_Color() { Selection_lbl.text = "LH Region Color"; show_Color_picker(); set_Color_to_Color_picker(global.LH_reg_col); }
    public void Change_Mesh_Color() { Selection_lbl.text = "Mesh Color"; show_Color_picker(); set_Color_to_Color_picker(global.Mesh_col); }

    void Start()
    {
        ColorPicker.onValueChanged.AddListener(color => 
        {

        });
    }

    void Update()
    {
        if(SelectedItem == null)
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
