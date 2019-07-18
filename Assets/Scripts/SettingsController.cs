using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public Text Selection_lbl;

    public void Change_Background_Color() => Selection_lbl.text = "Background Color";
    public void Change_Lbl_Color() => Selection_lbl.text = "Label Color";
    public void Change_RH_reg_Color() => Selection_lbl.text = "RH Region Color";
    public void Change_LH_reg_Color() => Selection_lbl.text = "LH Region Color";
    public void Change_Mesh_Color() => Selection_lbl.text = "Mesh Color";
    
}
