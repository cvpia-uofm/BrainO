using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Models;
using Assets.Models.Compaerer;
using Assets.Models.Interfaces;
using ModestTree;
using TMPro;
using UnityEngine;
using Zenject;

public class ROIsController : MonoBehaviour
{
    public GameObject Points_obj;
    public GameObject Correlations_obj;
    public GameObject Camera_holder;

    IEnumerable<Transform> Atlas_regions;
    IEnumerable<TMP_Text> ROIs_factors;

    [Inject]
    readonly IGlobal global;

    double factor_low, factor_mid, factor_midlow, factor_high;

    double EPSILON = 0.01f;

    public delegate void OnUpdateROIthr(double low, double mid, double high);
    public static event OnUpdateROIthr UpdateROIthr;
    public static event OnUpdateROIthr UpdateROIthrLgd;

    public delegate void OnFinishPlotROI();
    public static event OnFinishPlotROI Populate_ROI_ListView;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && global.ROIActivated)
        {
            if (!global.DoubleEscape_ROI_Deactivation)
                global.DoubleEscape_ROI_Deactivation = true;
            else
                global.DoubleEscape_ROI_Deactivation = false;
        }   
    }

    void Awake()
    {
        SideMenuController.OnPlotROI += PlotROIs;
        SideMenuController.RestorePoints += RemoveROI_lbls;
        SideMenuController.ApplyThr_ROI += SideMenuController_ApplyThr_ROI;
        CameraController.OnBrainRotate += RotateFactors;
        RegionsController.RestoreROI += PointsController_RestoreROI;
        RegionListController.RestorePreviousStateofRegion += RegionListController_RestorePreviousStateofRegion;
        RegionListController.OnFocus_rOI += RegionListController_OnFocus_rOI;
        SideMenuController.OnLabelActiveROI += SideMenuController_OnLabelActiveROI;
    }

    void SideMenuController_OnLabelActiveROI(bool active)
    {
        var roi_scores = Points_obj.GetComponentsInChildren<Transform>(true).Where(a => a.name.Equals("ROI_factor"));
        foreach(var roi_score in roi_scores)
        {
            roi_score.gameObject.SetActive(active);
        }
    }

    void RegionListController_OnFocus_rOI(ROI sel_rOI, ROI prev_sel_rOi)
    {
        if(prev_sel_rOi != null)
        {
            PointsController_RestoreROI(prev_sel_rOi);
            FocusRegion(sel_rOI);
            return;
        }
        FocusRegion(sel_rOI);
    }


    void PointsController_RestoreROI(ROI rOI)
    {
        var region_roi_obj = Points_obj.transform.Find(rOI.Region.ToUpper());
        ScaleColorROI(rOI, region_roi_obj);
    }

    void RegionListController_RestorePreviousStateofRegion(string region_name)
    {
        if (global.ROIActivated && !global.CorrelationActivated)
        {
            FindandRescaleRegions(region_name);
            return;
        }
        if (global.ROIActivated && global.CorrelationActivated)
        {
            var roi_cors = Correlations_obj.transform.GetComponentsInChildren<Transform>().Where(a => a.name.Contains(region_name));

            foreach(var roi_cor in roi_cors)
            {
                var roi_names = roi_cor.name.Split('_');

                foreach(var roi_name in roi_names)
                {
                    FindandRescaleRegions(roi_name);
                }
            }
        }
    }

    void FindandRescaleRegions(string region_name)
    {
        var region_roi = global.Current_rOIs.Single(a => a.Region.ToLower() == region_name.ToLower());
        var region_roi_obj = Points_obj.transform.Find(region_name.ToUpper());

        ScaleColorROI(region_roi, region_roi_obj);
    }

    void FocusRegion(ROI rOI)
    {
        var obj = Points_obj.transform.Find(rOI.Region.ToUpper()).gameObject;
        obj.transform.localScale = new Vector3(9f, 9f, 9f);
        obj.GetComponent<Renderer>().material.shader = Shader.Find("Custom/Outline");
    }



    #region Apply Threshold ROIs
    IEnumerator SideMenuController_ApplyThr_ROI(double low_thr, double mid_thr, double high_thr)
    {
        if (Math.Abs(low_thr - factor_low) > EPSILON)
        {
            factor_low = low_thr;
            factor_midlow = (factor_mid + factor_low) / 2;
           
            foreach (var roi in global.Current_rOIs)
            {
                foreach (Transform point in Points_obj.transform)
                {
                    if (roi.Region.ToUpper() == point.name)
                    {
                        ScaleColorROI(roi, point);
                        yield return new WaitForSeconds(0.0000001f);
                    }
                    
                }
            }
        }

        if (Math.Abs(mid_thr - factor_mid) > EPSILON)
        {
            factor_mid = mid_thr;
            factor_midlow = (factor_mid + factor_low) / 2;

            foreach (var roi in global.Current_rOIs)
            {
                foreach (Transform point in Points_obj.transform)
                {
                    if (roi.Region.ToUpper() == point.name)
                    {
                        ScaleColorROI(roi, point);
                        yield return new WaitForSeconds(0.0000001f);
                    }
                }
            }
        }

        if (Math.Abs(high_thr - factor_high) > EPSILON)
        {
            factor_high = high_thr;
            foreach (var roi in global.Current_rOIs)
            {
                foreach (Transform point in Points_obj.transform)
                {
                    if (roi.Region.ToUpper() == point.name)
                    {
                        ScaleColorROI(roi, point);
                        yield return new WaitForSeconds(0.0000001f);
                    }
                }
            }
        }
        UpdateROIthrLgd(low_thr, mid_thr, high_thr);
    } 
    #endregion

    void RemoveROI_lbls(string atlas_name, IEnumerable<Region> regions)
    {
        if (ROIs_factors != null && global.ROIActivated)
        {
            foreach (var fac_lbls in ROIs_factors)
            {
                fac_lbls.gameObject.SetActive(false);
            }
        }
    }

    void RotateFactors(float X, float Y)
    {
        if (ROIs_factors != null && global.ROIActivated)
        {
            foreach (var fac_lbls in ROIs_factors)
            {
                fac_lbls.transform.Rotate(-X, Y, 0);

            } 
        }
    }

    void Start()
    {
          
    }

    IEnumerator PlotROIs(IEnumerable<ROI> reg_of_interests, string current_atlas)
    {
        Init_ROI(reg_of_interests);
        CalculateThresholdROI(reg_of_interests);
        foreach (var roi in reg_of_interests)
        {
            var atlas_region = Atlas_regions.SingleOrDefault(a => a.name.ToUpper() == roi.Region.ToUpper());
            if (atlas_region != null)
            {
                var factor = atlas_region.GetComponentsInChildren<TMP_Text>(true).SingleOrDefault(a => a.name == "ROI_factor");
                factor.gameObject.SetActive(true);
                factor.text = roi.Importance_factor;
                ScaleColorROI(roi, atlas_region);
                yield return new WaitForSeconds(0.00001f);

            }
        }
        Populate_ROI_ListView();

        
    }

    void ScaleColorROI(ROI roi, Transform atlas_region)
    {
        if(global.Low_rOI_col != Color.white && global.Mid_rOI_col != Color.white && global.High_rOI_col != Color.white)
        {
            global.Low_rOI_col = Color.white;
            global.Mid_rOI_col = Color.blue;
            global.High_rOI_col = Color.magenta;
        }
        if (ToDouble(roi.Importance_factor) >= factor_low && ToDouble(roi.Importance_factor) < factor_midlow)
        {
            atlas_region.localScale = new Vector3(7f, 7f, 7f);           
            ConfigureColor_ROI(atlas_region.gameObject, global.Low_rOI_col);
        }
        if (ToDouble(roi.Importance_factor) >= factor_midlow && ToDouble(roi.Importance_factor) < factor_mid)
        {
            atlas_region.localScale = new Vector3(9f, 9f, 9f);
            ConfigureColor_ROI(atlas_region.gameObject, global.Mid_rOI_col);
        }
        if (ToDouble(roi.Importance_factor) >= factor_mid && ToDouble(roi.Importance_factor) <= factor_high)
        {
            atlas_region.localScale = new Vector3(10f, 10f, 10f);
            ConfigureColor_ROI(atlas_region.gameObject, global.High_rOI_col);
        }
    }

    void ConfigureColor_ROI(GameObject region, Color color)
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetColor("_Color", color);

        region.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
        region.GetComponent<Renderer>().SetPropertyBlock(props);
    }

    void Init_ROI(IEnumerable<ROI> reg_of_interests)
    {
        global.Current_Active_Regions = new List<Region>();
        Camera_holder.transform.rotation = new Quaternion(0, 0, 0, 0);
        var reg_lbls = Points_obj.GetComponentsInChildren<TMP_Text>(true);
        foreach(var reg_lbl in reg_lbls)
        {
            reg_lbl.transform.rotation = new Quaternion(0, 180, 0, 0);
        }
        Atlas_regions = Points_obj.GetComponentsInChildren<Transform>(true);
        ROIs_factors = Points_obj.GetComponentsInChildren<TMP_Text>(true).Where(a => a.name == "ROI_factor").ToList();
        global.Current_rOIs = reg_of_interests.ToList();
        foreach(var roi_reg in global.Current_rOIs)
        {
            var active_reg = global.Current_Region_list.SingleOrDefault(a => a.Abbreviation.ToUpper() == roi_reg.Region.ToUpper());
            global.Current_Active_Regions.Add(active_reg);
        }
        foreach(Transform region in Points_obj.transform)
        {
            region.gameObject.SetActive(true);
        }
        FocusImportantRegions();
    }

    void FocusImportantRegions()
    {
        List<Region> regs = new List<Region>();
        foreach (var roi in global.Current_rOIs)
        {
            if (global.Current_Region_list.ToList().Exists(a => a.Abbreviation.ToUpper() == roi.Region.ToUpper()))
            {
                regs.Add(global.Current_Region_list.Single(a => a.Abbreviation.ToUpper() == roi.Region.ToUpper()));
            }
        }
        var region_im = global.Current_Region_list.ToList().Except(regs, new Reg_rOI_Comparer()).ToList();
        foreach (var region in region_im)
        {
            var point_unimp = Points_obj.GetComponentsInChildren<Transform>().SingleOrDefault(a => a.name.ToUpper() == region.Abbreviation.ToUpper());

            if (point_unimp != null)
            {
                point_unimp.gameObject.SetActive(false);
            }
        }
    }

    void CalculateThresholdROI(IEnumerable<ROI> reg_of_interests)
    {
        factor_low = ToDouble(reg_of_interests.First().Importance_factor);
        foreach (var roi in reg_of_interests)
        {
            if (ToDouble(roi.Importance_factor) < factor_low)
                factor_low = ToDouble(roi.Importance_factor);
            if (ToDouble(roi.Importance_factor) > factor_high)
                factor_high = ToDouble(roi.Importance_factor);
        }
        factor_mid = (factor_low + factor_high) / 2;
        factor_midlow = (factor_low + factor_mid) / 2;

        if (!global.AnyRegionSelected)
        {
            UpdateROIthr(factor_low, factor_mid, factor_high); 
        }
    }

    double ToDouble(string txt_num)
    {
        return Double.Parse(txt_num);
    }
    
}
