using Assets.Models;
using Assets.Models.Correlation_Generator;
using Assets.Models.Interfaces;
using AutoMapperFactory;
using ExcelFactory;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
public class SideMenuController : MonoBehaviour
{
    public TMP_Dropdown AtlasDropDown;
    public Button Collapse_btn;

    public delegate void OnPlotAction(IEnumerable<Corelation> corelations, string current_atlas);

    public static event OnPlotAction OnPlotCorrelation;

    public delegate void OnAtlasAction(string atlas_name);

    public static event OnAtlasAction OnChangeAtlas;

    [Inject]
    private readonly IAtlas atlas;

    private GameObject Left_Hemph;
    private GameObject Right_Hemph;

    private string Current_Atlas = Atlas.DSK_Atlas;

    private IEnumerable<Corelation> Corelations;

    #region Animator
    public void BoolAnimator(Animator anim)
    {
        if (anim.GetBool("IsDisplayed"))
        {
            anim.SetBool("IsDisplayed", false);
            Collapse_btn.GetComponentInChildren<Text>().text = ">";
        }

        else
        {
            anim.SetBool("IsDisplayed", true);
            Collapse_btn.GetComponentInChildren<Text>().text = "<";
        }
    }
  

    #endregion

    private void Awake()
    {
        Left_Hemph = GameObject.FindGameObjectWithTag("Left_Hemp");
        Right_Hemph = GameObject.FindGameObjectWithTag("Right_Hemp");

        Corelations = new List<Corelation>();

        AtlasDropDown.AddOptions(new List<string>() { Atlas.DSK_Atlas, Atlas.DTX_Atlas, Atlas.CDK_Atlas, Atlas.A116_Atlas, Atlas.A90_Atlas });
    }

    private int CountRows(StreamReader reader)
    {
        int count = 0;
        while (!String.IsNullOrWhiteSpace(reader.ReadLine()))
            count++;
        return count;
    }

    public void Left_Hemph_Activation(bool active) => Left_Hemph.SetActive(active);

    public void Right_Hemph_Activation(bool active) => Right_Hemph.SetActive(active);

    public void Load_Correlation()
    {
        var extensions = new[] {
            new ExtensionFilter("CSV Files", "csv" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (path == null)
            return;

        using (var reader = new StreamReader(path[0]))
        {
            var raw = reader.ReadToEnd();
            string[] data = raw.Split('\n');
            Corelations = MapperFactory<Corelation>.Map_CSV(data, MapperEnums.Inputs.Correlations);

            if ((Corelations as List<Corelation>).Count != 0)
                OnPlotCorrelation(Corelations, Current_Atlas);
        }
    }

    public void Test_Correlation()
    {
        switch (AtlasDropDown.value)
        {
            case 0:
                Corelations = CorrelationGenerator.GenerateRandomCorrelation(atlas.Desikan_Atlas);
                break;

            case 1:
                Corelations = CorrelationGenerator.GenerateRandomCorrelation(atlas.Destrieux_Atlas);
                break;

            case 2:
                Corelations = CorrelationGenerator.GenerateRandomCorrelation(atlas.Craddock_Atlas);
                break;

            case 3:
                Corelations = CorrelationGenerator.GenerateRandomCorrelation(atlas.Aal116_Atlas);
                break;

            case 4:
                Corelations = CorrelationGenerator.GenerateRandomCorrelation(atlas.Aal90_Atlas);
                break;
        }

        OnPlotCorrelation(Corelations, Current_Atlas);
    }

    public void OnAtlasDropDownValueChange(int index)
    {
        switch (index)
        {
            case 0:
                OnChangeAtlas(Atlas.DSK_Atlas);
                Current_Atlas = Atlas.DSK_Atlas;
                break;

            case 1:
                OnChangeAtlas(Atlas.DTX_Atlas);
                Current_Atlas = Atlas.DTX_Atlas;
                break;

            case 2:
                OnChangeAtlas(Atlas.CDK_Atlas);
                Current_Atlas = Atlas.CDK_Atlas;
                break;

            case 3:
                OnChangeAtlas(Atlas.A116_Atlas);
                Current_Atlas = Atlas.A116_Atlas;
                break;

            case 4:
                OnChangeAtlas(Atlas.A90_Atlas);
                Current_Atlas = Atlas.A90_Atlas;
                break;
        }
    }
}