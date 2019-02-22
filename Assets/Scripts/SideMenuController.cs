using Assets.Models;
using AutoMapperFactory;
using SFB;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class SideMenuController : MonoBehaviour
{
    private GameObject Left_Hemph;
    private GameObject Right_Hemph;

    private IEnumerable<Corelation> Corelations;
    private void Awake()
    {
        Left_Hemph = GameObject.FindGameObjectWithTag("Left_Hemp");
        Right_Hemph = GameObject.FindGameObjectWithTag("Right_Hemp");
        Corelations = new List<Corelation>();
    }
    public void Left_Hemph_Activation(bool active) => Left_Hemph.SetActive(active);
    public void Right_Hemph_Activation(bool active) => Right_Hemph.SetActive(active);

    public void Load_Corelation()
    {
        var extensions = new[] {
            new ExtensionFilter("CSV Files", "csv" ),
        };
        //var data = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        using (var dialog = new OpenFileDialog())
        {
            if(dialog.ShowDialog() == DialogResult.OK) { }
        }
    }

}
