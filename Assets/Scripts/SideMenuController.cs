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
using System.Linq;
using UnityEngine.EventSystems;

public class SideMenuController : MonoBehaviour
{
    public TMP_Dropdown AtlasDropDown;
    public Button Collapse_btn;
    public InputField Low_range;
    public InputField Mid_range;
    public InputField High_range;
    public InputField Low_ROI;
    public InputField Mid_ROI;
    public InputField High_ROI;
    public Toggle Low_thr_toggle;
    public Toggle Mid_thr_toggle;
    public Toggle High_thr_toggle;
    public Toggle ROI_Score_label;
    public Button Load_Correlation_btn;
    public Button Load_rOI_btn;
    public GameObject Right_Panel_Weight;
    public GameObject Right_Panel_ROI;
    public GameObject Cerebellum;
    public GameObject Brainstem;
    public Camera Main;
    public Camera Auxilary;
    public ColorPicker Settings_Color_Picker;

    public delegate IEnumerator OnPlotAction(IEnumerable<Corelation> corelations, string current_atlas);
    public static event OnPlotAction OnPlotCorrelation;

    public delegate IEnumerator OnROI_Action(IEnumerable<ROI> reg_of_interests, string current_atlas);
    public static event OnROI_Action OnPlotROI;

    public delegate void OnAtlasAction(string atlas_name);
    public static event OnAtlasAction OnChangeAtlas;

    public delegate IEnumerator OnApplyThrAction(double thr_l, double thr_h, bool active);
    public static event OnApplyThrAction ApplyThr_bool;

    public delegate IEnumerator OnApplyThrValueChange(double low_thr, double mid_thr, double high_thr);
    public static event OnApplyThrValueChange ApplyThr_text;
    public static event OnApplyThrValueChange ApplyThr_ROI;

    public delegate void OnFigureAction(string path);
    public static event OnFigureAction TakeFigure;

    public delegate void OnEscapleAction(string atlas_name, IEnumerable<Region> regions);
    public static event OnEscapleAction RestorePoints;

    public delegate void OnLabelActivate(bool active);
    public static event OnLabelActivate OnLabelActive;
    public static event OnLabelActivate OnLabelActiveROI;

    [Inject]
    readonly IAtlas atlas;
    [Inject]
    readonly IGlobal global;

    GameObject Left_Hemph;
    GameObject Right_Hemph;

    string Current_Atlas = Atlas.DSK_Atlas;

    IEnumerable<Corelation> Corelations;
    IEnumerable<ROI> ROIs;

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
    public void BoolAnimatorTopMenu(Animator anim)
    {
        if (anim.GetBool("IsDisplayedTopMenu"))
        {
            anim.SetBool("IsDisplayedTopMenu", false);
            global.Settings_Activated = false;
        }

        else
        {
            anim.SetBool("IsDisplayedTopMenu", true);
            global.Settings_Activated = true;
        }
    }


    #endregion

    void Awake()
    {
        Left_Hemph = GameObject.FindGameObjectWithTag("Left_Hemp");
        Right_Hemph = GameObject.FindGameObjectWithTag("Right_Hemp");

        Corelations = new List<Corelation>();

        AtlasDropDown.AddOptions(new List<string>() { Atlas.DSK_Atlas, Atlas.DTX_Atlas, Atlas.CDK_Atlas, Atlas.A116_Atlas, Atlas.A90_Atlas });

        global.Atlas_Regions_dict_index = new Dictionary<int, IEnumerable<Region>>()
        {
            { 0, atlas.Desikan_Atlas },
            { 1, atlas.Destrieux_Atlas },
            { 2, atlas.Craddock_Atlas },
            { 3, atlas.Aal116_Atlas },
            { 4, atlas.Aal90_Atlas }
        };

        global.Atlas_Regions_value_pairs = new Dictionary<string, IEnumerable<Region>>()
        {
            { Atlas.DSK_Atlas,  atlas.Desikan_Atlas },
            { Atlas.DTX_Atlas, atlas.Destrieux_Atlas },
            { Atlas.CDK_Atlas, atlas.Craddock_Atlas },
            { Atlas.A116_Atlas, atlas.Aal116_Atlas },
            { Atlas.A90_Atlas, atlas.Aal90_Atlas }
        };

        CorrelationController.UpdateWeightThr += UpdateWeightThr;
        ROIsController.UpdateROIthr += ROIsController_UpdateROIthr;

    }

    void ROIsController_UpdateROIthr(double low, double mid, double high)
    {
        Low_ROI.text = low.ToString();
        Mid_ROI.text = mid.ToString();
        High_ROI.text = high.ToString();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            global.CorrelationActivated = false;
            global.ROIActivated = false;
            global.AnyRegionSelected = false;
            Right_Panel_Weight.SetActive(false);
            Right_Panel_ROI.SetActive(false);
            Load_Correlation_btn.interactable = true;
            Load_rOI_btn.interactable = true;
            ROI_Score_label.gameObject.SetActive(false);
            RestorePoints(Current_Atlas, global.Atlas_Regions_dict_index[AtlasDropDown.value]);
        }
        if (IsMouseOver())
        {
            global.MouseOverUI = true;
        }
        else
            global.MouseOverUI = false;

        
    }

    void UpdateWeightThr(double low, double mid, double high)
    {
        Low_range.text = low.ToString();
        Mid_range.text = mid.ToString();
        High_range.text = high.ToString();
    }

    int CountRows(StreamReader reader)
    {
        int count = 0;
        while (!String.IsNullOrWhiteSpace(reader.ReadLine()))
            count++;
        return count;
    }
    public enum FileBrowserOptions {OpenFile, SaveFile};
    public static string[] FileBrowser(ExtensionFilter[] extensions, FileBrowserOptions browserOptions)
    {
        if (browserOptions == FileBrowserOptions.OpenFile)
        {
            var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
            return path;
        }
        if (browserOptions == FileBrowserOptions.SaveFile)
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Save Figure", "", "Brain_O_fig", extensions);
            var path_to_save = new string[] { path };
            return path_to_save;
        }

        return null;
    }


    public void Left_Hemph_Activation(bool active) => Left_Hemph.SetActive(active);

    public void Right_Hemph_Activation(bool active) => Right_Hemph.SetActive(active);

    public void Cerebellum_Activation(bool active) => Cerebellum.SetActive(active);

    public void Brainstem_Activation(bool active) => Brainstem.SetActive(active);

    public void Labels_Activation(bool active) => OnLabelActive(active);

    public void Labels_ROI_Score_Activation(bool active) => OnLabelActiveROI(active);

    public void Load_Correlation()
    {
        var extensions = new[] {
            new ExtensionFilter("CSV Files", "csv"),
        };

        string[] path = FileBrowser(extensions, FileBrowserOptions.OpenFile);

        if (path == null)
            return;

        using (var reader = new StreamReader(path[0]))
        {
            var raw = reader.ReadToEnd();
            string[] data = raw.Split('\n');
            Corelations = MapperFactory<Corelation>.Map_CSV(data, MapperEnums.Inputs.Correlations);

            if ((Corelations as List<Corelation>).Count != 0)
            {
                global.CorrelationActivated = true;

                StartCoroutine(OnPlotCorrelation(Corelations, Current_Atlas));

                Load_rOI_btn.interactable = false;
                Right_Panel_Weight.SetActive(true);
                Low_thr_toggle.isOn = true;
                Mid_thr_toggle.isOn = true;
                High_thr_toggle.isOn = true;
            }

        }
    }

    public void Load_ROI()
    {
        var extensions = new[] {
            new ExtensionFilter("CSV Files", "csv" ),
        };

        string[] path = FileBrowser(extensions, FileBrowserOptions.OpenFile);

        if (path == null)
            return;
        using (var reader = new StreamReader(path[0]))
        {
            var raw = reader.ReadToEnd();
            string[] data = raw.Split('\n');
            ROIs = MapperFactory<ROI>.Map_CSV(data, MapperEnums.Inputs.ROIs);

            ROIs = ROIs.Where(r => !string.IsNullOrWhiteSpace(r.Importance_factor) || !string.IsNullOrWhiteSpace(r.Region)).ToList();


            if ((ROIs as List<ROI>).Count != 0)
            {
                global.ROIActivated = true;

                Load_Correlation_btn.interactable = false;
                StartCoroutine(OnPlotROI(ROIs, Current_Atlas));

                Right_Panel_ROI.SetActive(true);
                ROI_Score_label.gameObject.SetActive(true);
            }

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

        StartCoroutine(OnPlotCorrelation(Corelations, Current_Atlas));
    }

    public void Print_Figure()
    {
        TakeFigure(null);

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
        if (NotEmptyRange(Low_range.text, Mid_range.text, High_range.text))
        {
            var low = Double.Parse(Low_range.text);
            var mid_low = (low + Double.Parse(Mid_range.text)) / 2;

            StartCoroutine(ApplyThr_bool(low, mid_low, active));
        }
        Low_range.interactable = active;
    }

    public void ShowThrPoints_mid(bool active)
    {
        if (NotEmptyRange(Low_range.text, Mid_range.text, High_range.text))
        {
            var mid_low = (Double.Parse(Low_range.text) + Double.Parse(Mid_range.text)) / 2;
            var mid = Double.Parse(Mid_range.text);

            StartCoroutine(ApplyThr_bool(mid_low + 0.001f, mid, active));
        }
        Mid_range.interactable = active;

    }

    public void ShowThrPoints_high(bool active)
    {
        if (NotEmptyRange(Low_range.text, Mid_range.text, High_range.text))
        {
            var mid = Double.Parse(Mid_range.text);
            var high = Double.Parse(High_range.text);

            StartCoroutine(ApplyThr_bool(mid + 0.001f, high, active));
        }
        High_range.interactable = active;
    }

    bool NotEmptyRange(string low_txt, string mid_txt, string high_txt)
    {
        return !String.IsNullOrWhiteSpace(low_txt) && !String.IsNullOrWhiteSpace(mid_txt) && !String.IsNullOrWhiteSpace(high_txt);
    }
    #endregion
    #region Threshold 'Connectivity'
    public void OnValueChange_Thr_low(string thr_txt)
    {
        if (thr_txt == ".")
            Low_range.text = "0.";
        if (NotEmptyRange(Low_range.text, Mid_range.text, High_range.text) && thr_txt != "0." && thr_txt != "0")
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
        if (NotEmptyRange(Low_range.text, Mid_range.text, High_range.text) && thr_txt != "0." && thr_txt != "0")
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
        if (NotEmptyRange(Low_range.text, Mid_range.text, High_range.text) && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(Low_range.text);
            var mid_thr = Double.Parse(Mid_range.text);
            var high_thr = Double.Parse(thr_txt);
            StartCoroutine(ApplyThr_text(low_thr, mid_thr, high_thr));
        }
    }

    #endregion

    #region Threshold 'ROIs'
    public void OnValueChange_Thr_low_ROI(string thr_txt)
    {
        if (thr_txt == ".")
            Low_ROI.text = "0.";
        if (NotEmptyRange(Low_ROI.text, Mid_ROI.text, High_ROI.text) && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(thr_txt);
            var mid_thr = Double.Parse(Mid_ROI.text);
            var high_thr = Double.Parse(High_ROI.text);
            StartCoroutine(ApplyThr_ROI(low_thr, mid_thr, high_thr));
        }
    }
    public void OnValueChange_Thr_mid_ROI(string thr_txt)
    {
        if (thr_txt == ".")
            Mid_ROI.text = "0.";
        if (NotEmptyRange(Low_ROI.text, Mid_ROI.text, High_ROI.text) && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(Low_ROI.text);
            var mid_thr = Double.Parse(thr_txt);
            var high_thr = Double.Parse(High_ROI.text);
            StartCoroutine(ApplyThr_ROI(low_thr, mid_thr, high_thr));
        }
    }
    public void OnValueChange_Thr_high_ROI(string thr_txt)
    {
        if (thr_txt == ".")
            High_ROI.text = "0.";
        if (NotEmptyRange(Low_ROI.text, Mid_ROI.text, High_ROI.text) && thr_txt != "0." && thr_txt != "0")
        {
            var low_thr = Double.Parse(Low_ROI.text);
            var mid_thr = Double.Parse(Mid_ROI.text);
            var high_thr = Double.Parse(thr_txt);
            StartCoroutine(ApplyThr_ROI(low_thr, mid_thr, high_thr));
        }
    }
    #endregion
    bool IsMouseOver() => EventSystem.current.IsPointerOverGameObject();

}