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
using System.Collections;

public class SideMenuController : MonoBehaviour
{
    public TMP_Dropdown AtlasDropDown;
    public Button Collapse_btn;
    public InputField Low_range;
    public InputField Mid_range;
    public InputField High_range;

    public delegate void OnPlotAction(IEnumerable<Corelation> corelations, string current_atlas);
    public static event OnPlotAction OnPlotCorrelation;

    public delegate void OnROI_Action(IEnumerable<ROI> reg_of_interests, string current_atlas);
    public static event OnROI_Action OnPlotROI;

    public delegate void OnAtlasAction(string atlas_name);
    public static event OnAtlasAction OnChangeAtlas;

    public delegate void OnApplyThrAction(double thr_l, double thr_h, bool active);
    public static event OnApplyThrAction ApplyThr_bool;

    public delegate IEnumerator OnApplyThrValueChange(double low_thr, double mid_thr, double high_thr);
    public static event OnApplyThrValueChange ApplyThr_text;

    [Inject]
    private readonly IAtlas atlas;

    private GameObject Left_Hemph;
    private GameObject Right_Hemph;

    private string Current_Atlas = Atlas.DSK_Atlas;

    private IEnumerable<Corelation> Corelations;
    private IEnumerable<ROI> ROIs;

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

        CorrelationController.UpdateWeightThr += UpdateWeightThr;
    }

    private void UpdateWeightThr(double low, double mid, double high)
    {
        Low_range.text = low.ToString();
        Mid_range.text = mid.ToString();
        High_range.text = high.ToString();
    }

    private int CountRows(StreamReader reader)
    {
        int count = 0;
        while (!String.IsNullOrWhiteSpace(reader.ReadLine()))
            count++;
        return count;
    }

    private string[] FileBrowser()
    {
        var extensions = new[] {
            new ExtensionFilter("CSV Files", "csv" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        return path;
    }


    public void Left_Hemph_Activation(bool active) => Left_Hemph.SetActive(active);

    public void Right_Hemph_Activation(bool active) => Right_Hemph.SetActive(active);

    public void Load_Correlation()
    {
        string[] path = FileBrowser();

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

    public void Load_ROI()
    {
        string[] path = FileBrowser();

        if (path == null)
            return;


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

    #region Threshold Bool
    public void ShowThrPoints_low(bool active)
    {
        if (NotEmptyRange())
        {
            var low = Double.Parse(Low_range.text);
            var mid_low = (low + Double.Parse(Mid_range.text)) / 2;

            ApplyThr_bool(low, mid_low, active);
        }
        Low_range.interactable = active;
    }

    public void ShowThrPoints_mid(bool active)
    {
        if (NotEmptyRange())
        {
            var mid_low = (Double.Parse(Low_range.text) + Double.Parse(Mid_range.text)) / 2;
            var mid = Double.Parse(Mid_range.text);

            ApplyThr_bool(mid_low, mid, active);
        }
        Mid_range.interactable = active;

    }

    public void ShowThrPoints_high(bool active)
    {
        if (NotEmptyRange())
        {
            var mid = Double.Parse(Mid_range.text);
            var high = Double.Parse(High_range.text);

            ApplyThr_bool(mid, high, active);
        }
        High_range.interactable = active;
    }

    private bool NotEmptyRange()
    {
        return !String.IsNullOrWhiteSpace(Low_range.text) && !String.IsNullOrWhiteSpace(Mid_range.text) && !String.IsNullOrWhiteSpace(High_range.text);
    }
    #endregion
    #region Threshold Text
    public void OnValueChange_Thr_low(string thr_txt)
    {
        if (thr_txt == ".")
            Low_range.text = "0.";
        if (NotEmptyRange() && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(thr_txt);
            var mid_thr = Double.Parse(Mid_range.text);
            var high_thr = Double.Parse(High_range.text);
            StartCoroutine(ApplyThr_text(low_thr, mid_thr, high_thr));
        }
    }

    public void OnValueChange_Thr_mid(string thr_txt)
    {
        if (thr_txt == ".")
            Mid_range.text = "0.";
        if (NotEmptyRange() && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(Low_range.text);
            var mid_thr = Double.Parse(thr_txt);
            var high_thr = Double.Parse(High_range.text);
            StartCoroutine(ApplyThr_text(low_thr, mid_thr, high_thr));
        }
    }
    public void OnValueChange_Thr_high(string thr_txt)
    {
        if (thr_txt == ".")
            High_range.text = "0.";
        if (NotEmptyRange() && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(Low_range.text);
            var mid_thr = Double.Parse(Mid_range.text);
            var high_thr = Double.Parse(thr_txt);
            StartCoroutine(ApplyThr_text(low_thr, mid_thr, high_thr));
        }
    }

    #endregion
    


}