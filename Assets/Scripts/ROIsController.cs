using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Func_Area_Model;
using Assets.Models;
using TMPro;
using UnityEngine;

public class ROIsController : MonoBehaviour
{
    IEnumerable<Transform> Atlas_regions;
    IEnumerable<TMP_Text> ROIs_factors;

    double factor_low, factor_mid, factor_midlow, factor_high;
    private void Awake()
    {
        SideMenuController.OnPlotROI += PlotROIs;
    }

    private void Start()
    {
        
    }

    private void PlotROIs(IEnumerable<ROI> reg_of_interests, string current_atlas)
    {
        Init_ROI();
        CalculateThresholdROI(reg_of_interests);
        foreach (var roi in reg_of_interests)
        {
            var atlas_region = Atlas_regions.SingleOrDefault(a => a.name == roi.Region);

            if (atlas_region != null)
            {
                var factor = atlas_region.GetComponentsInChildren<TMP_Text>(true).SingleOrDefault(a => a.name == "ROI_factor");
                factor.gameObject.SetActive(true);
                factor.text = roi.Importance_factor + "%";
                ScaleColorROI(roi, atlas_region);

            }
        }
    }

    private void ScaleColorROI(ROI roi, Transform atlas_region)
    {
        if (ToDouble(roi.Importance_factor) >= factor_low && ToDouble(roi.Importance_factor) <= factor_midlow)
        {
            atlas_region.localScale = new Vector3(3.5f, 3.5f, 3.5f);
            ConfigureColor_ROI(atlas_region.gameObject, Color.white);
        }
        if (ToDouble(roi.Importance_factor) > factor_midlow && ToDouble(roi.Importance_factor) < factor_mid)
        {
            atlas_region.localScale = new Vector3(4.5f, 4.5f, 4.5f);
            ConfigureColor_ROI(atlas_region.gameObject, Color.blue);
        }
        if (ToDouble(roi.Importance_factor) >= factor_mid && ToDouble(roi.Importance_factor) <= factor_high)
        {
            atlas_region.localScale = new Vector3(7f, 7f, 7f);
            ConfigureColor_ROI(atlas_region.gameObject, Color.magenta);
        }
    }

    void ConfigureColor_ROI(GameObject region, Color color)
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetColor("_Color", color);
        region.GetComponent<Renderer>().SetPropertyBlock(props);
    }

    private void Init_ROI()
    {
        Atlas_regions = GameObject.Find("Points").GetComponentsInChildren<Transform>(true);
        ROIs_factors = new List<TMP_Text>();
    }

    private void CalculateThresholdROI(IEnumerable<ROI> reg_of_interests)
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
    }

    double ToDouble(string txt_num)
    {
        return Double.Parse(txt_num);
    }
    
}
