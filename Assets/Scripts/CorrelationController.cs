﻿using Assets.Models;
using Assets.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

public class CorrelationController : MonoBehaviour
{
    const double epsilon = 0.0001f;
    string Current_Atlas;
    List<GameObject> activePoints;
    double low, mid, high, mid_low;

    [Inject]
    readonly IGlobal global;

    public GameObject Region_obj;

    public delegate void OnActivateAction();
    public static event OnActivateAction ActivateAllPoints;

    public delegate void OnWeightThrUpdateAction(double low, double mid, double high);
    public static event OnWeightThrUpdateAction UpdateWeightThr;

    public delegate IEnumerator OnRegionPathAction(IEnumerable<Region> regions);
    public static event OnRegionPathAction OnPathAction;

    public delegate void OnUpdateRegionList(IEnumerable<Region> atlas_regions);
    public static event OnUpdateRegionList Update_Regionlist;

    void Awake()
    {
        SideMenuController.OnPlotCorrelation += PlotCorrelations;
        SideMenuController.OnChangeAtlas += Trigger_Existing_Correlation;
        SideMenuController.ApplyThr_bool += ApplyThr_bool;
        SideMenuController.ApplyThr_text += ApplyThr_text;
        SideMenuController.RestorePoints += ResetCorrelations;
        RegionListController.OnRegionSelected += RegionListController_OnRegionSelected;
        RegionListController.RestorePreviousStateofRegion += RegionListController_RestorePreviousStateofRegion;
    }

    void RegionListController_RestorePreviousStateofRegion(string region_name)
    {
        if (global.CorrelationActivated)
        {            
            var region = Region_obj.transform.Find(region_name.ToUpper());
            var rbs = region.GetComponents<FixedJoint>();

            foreach (var rb in rbs)
            {
                var cb = rb.connectedBody.gameObject;

                if (cb.activeInHierarchy)
                {
                    foreach (var cor in global.Current_Correlations)
                    {
                        if (cb.name == cor.PointX + "_" + cor.PointY)
                            ConfigureEdgeWeight(cor, cb, Vector3.zero);
                    }
                }
            }
        }
    }
    void Start()
    {
        
    }

    IEnumerator RegionListController_OnRegionSelected(string region_name)
    {
        if (global.CorrelationActivated)
        {
            SetEdgeWeightstoDefault();
            if (!string.IsNullOrWhiteSpace(region_name) && global.CorrelationActivated)
            {
                List<Region> region_path = new List<Region>();
                foreach (Transform cor in gameObject.GetComponentsInChildren<Transform>(false))
                {
                    if (cor.name.Contains(region_name))
                    {
                        cor.GetComponent<Renderer>().material.shader = Shader.Find("Custom/Outline");

                        region_path = CollectActiveRegions(global.Current_Correlations.Single(a => a.PointX + "_" + a.PointY == cor.name), region_path).ToList();
                    }
                }
                yield return StartCoroutine(OnPathAction(region_path));
            }
        }
    }

    void SetEdgeWeightstoDefault()
    {
        foreach (var cor in global.Current_Correlations)
        {
            foreach (Transform con in transform)
            {
                if (string.Concat(cor.PointX, "_", cor.PointY) == con.name)
                {
                    ConfigureEdgeWeight(cor, con.gameObject, Vector3.zero);
                    break;

                }
            }
        }
    }

    void ResetCorrelations(string atlas_name, IEnumerable<Region> regions)
    {
        RemoveExistingCorrelation();
    }

    #region Threshold Events
    IEnumerator ApplyThr_bool(double thr_l, double thr_h, bool active)
    {
        var in_active_cor = global.Current_Correlations.Where(a => a.Weight >= thr_l && a.Weight <= thr_h);
        foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            foreach (var cor in in_active_cor)
            {
                if (child.name == cor.PointX + "_" + cor.PointY)
                {
                    child.gameObject.SetActive(active);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }

    }

    IEnumerator ApplyThr_text(double low_thr, double mid_thr, double high_thr)
    {
        if (gameObject.GetComponentsInChildren<Transform>(true).First() != null && gameObject.GetComponentsInChildren<Transform>(true) != null)
        {
            if (Math.Abs(low_thr - low) > epsilon)
            {
                if (low < low_thr)
                {
                    StartCoroutine(ApplyThr_bool(low, low_thr - 0.001f, false));
                }
                else
                    StartCoroutine(ApplyThr_bool(low_thr, low, true));

                low = low_thr;
                mid_low = (mid + low) / 2;

            }
            if (Math.Abs(mid_thr - mid) > epsilon)
            {
                mid = mid_thr;
                mid_low = (mid + low) / 2;

            }
            if (Math.Abs(high_thr - high) > epsilon)
            {
                if (high_thr < high)
                {
                    StartCoroutine(ApplyThr_bool(high_thr + 0.001f, high, false));
                }
                else
                    StartCoroutine(ApplyThr_bool(high, high_thr, true));
                high = high_thr;
            }
            var childs = gameObject.GetComponentsInChildren<Transform>(true).ToList();
            foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                foreach (var cor in global.Current_Correlations)
                {
                    if (child.name == cor.PointX + "_" + cor.PointY)
                    {
                        var scale = child.transform.localScale;
                        scale = ConfigureEdgeWeight(cor, child.gameObject, scale);

                        child.transform.localScale = scale;
                        yield return new WaitForSeconds(0.01f);
                    }
                }
            } 
        }
    }
    #endregion

    void Update()
    {
    }

    #region Configure Correlation
    IEnumerator PlotCorrelations(IEnumerable<Corelation> corelations, string current_atlas)
    {
        InitPlot(corelations, current_atlas);

        FindThresholdMarkers(corelations);

        foreach (var relation in corelations)
        {
            CollectActiveRegions(relation, global.Current_Active_Regions);
            Load_Init_Prefab_Edge(relation, out GameObject pointX, out GameObject pointY,
                out Vector3 region_start, out Vector3 region_end, out GameObject edge);

            Configure_Transformation(relation, edge, region_start, region_end);

            Configure_RigidBody_Constraints(pointX, pointY, edge);
            yield return new WaitForSeconds(0.000000001f);
        }

        ShowOnlyActivePoints(current_atlas);
        Update_Regionlist(global.Current_Active_Regions);


    }

    IList<Region> CollectActiveRegions(Corelation relation, IList<Region> regions_storage)
    {
        var pointX = global.Current_Region_list.SingleOrDefault(a => a.Abbreviation == relation.PointX);
        var pointY = global.Current_Region_list.SingleOrDefault(a => a.Abbreviation == relation.PointY);

        if (!regions_storage.Contains(pointX))
            regions_storage.Add(pointX);
        if (!regions_storage.Contains(pointY))
            regions_storage.Add(pointY);

        return regions_storage;
    }

    #region Threshold
    void FindThresholdMarkers(IEnumerable<Corelation> corelations)
    {
        low = corelations.First().Weight;
        foreach (var relation in corelations)
        {
            low = FindThresholdMarkers_low(relation.Weight);
            high = FindThresholdMarkers_high(relation.Weight);
        }
        FindThresholdMarkers_mid();
        UpdateWeightThr(low, mid, high);
    }

    void FindThresholdMarkers_mid()
    {
        mid = (high + low) / 2;
        mid_low = (mid + low) / 2;
    }

    double FindThresholdMarkers_high(double weight)
    {
        if (weight > high)
            return weight;
        return high;
    }

    double FindThresholdMarkers_low(double weight)
    {
        if (weight < low)
            return weight;
        return low;
    }
    #endregion

    void InitPlot(IEnumerable<Corelation> corelations, string current_atlas)
    {
        RemoveExistingCorrelation();
        ActivateAllPoints();

        transform.parent.position = Vector3.zero;
        transform.parent.rotation = Quaternion.identity;
        transform.parent.localScale = new Vector3(40, 40, 40);

        var points_abr = GameObject.Find("Points").GetComponentsInChildren<TMP_Text>().ToList();

        foreach (var point in points_abr)
        {
            point.transform.rotation = new Quaternion(0, 180, 0, 0);
        }

        Current_Atlas = current_atlas;
        global.Current_Correlations = corelations;

        activePoints = new List<GameObject>();
        global.Current_Active_Regions = new List<Region>();
    }

    void Load_Init_Prefab_Edge(Corelation relation, out GameObject pointX, out GameObject pointY, out Vector3 region_start, out Vector3 region_end, out GameObject edge)
    {
        Find_Gather_Points(relation, out pointX, out pointY);

        region_start = pointX.transform.position;
        region_end = pointY.transform.position;
        edge = Instantiate(Resources.LoadAsync("Edge").asset as GameObject);
        edge.name = string.Concat(relation.PointX, "_", relation.PointY);
        edge.transform.parent = transform;
    }

    void Configure_Transformation(Corelation relation, GameObject edge, Vector3 region_start, Vector3 region_end)
    {
        var offset = region_end - region_start;
        var position = (region_start + region_end) / 2;

        edge.transform.position = position;

        var scale = edge.transform.localScale;
        scale.y = Vector3.Distance(region_start, edge.transform.position);

        scale = ConfigureEdgeWeight(relation, edge, scale);

        edge.transform.localScale = scale;

        edge.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset);
    }

    Vector3 ConfigureEdgeWeight(Corelation relation, GameObject edge, Vector3 scale)
    {
        if (relation.Weight >= low && relation.Weight <= mid_low)
        {
            if (scale != Vector3.zero)
                scale = Scale(scale, 0.8f, 0.8f);
            SetEdgeColor(edge, Color.black);
        }
        if (relation.Weight > mid_low && relation.Weight <= mid)
        {
            if (scale != Vector3.zero)
                scale = Scale(scale, 2f, 2f);
            SetEdgeColor(edge, Color.blue);
        }
        if (relation.Weight > mid && relation.Weight <= high || Math.Abs(mid - high) < 0.00001)
        {
            if (scale != Vector3.zero)
                scale = Scale(scale, 3f, 3f);
            SetEdgeColor(edge, Color.yellow);
        }
        return scale;
    }

    Vector3 Scale(Vector3 scale, float x, float z)
    {
        scale.x = x;
        scale.z = z;
        return scale;
    }

    void SetEdgeColor(GameObject edge, Color color)
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetColor("_Color", color);
        edge.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
        edge.GetComponent<Renderer>().SetPropertyBlock(props);
    }

    void Configure_RigidBody_Constraints(GameObject pointX, GameObject pointY, GameObject edge)
    {
        pointX.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();
        pointY.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();

        pointX.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        pointY.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
    }

    #endregion Configure Correlation

    #region Gather Remove Find Points

    void Find_Gather_Points(Corelation relation, out GameObject pointX, out GameObject pointY)
    {
        pointX = GameObject.Find(relation.PointX.ToUpper());
        pointY = GameObject.Find(relation.PointY.ToUpper());

        GatherActivePoints(pointX, pointY);
    }

    void GatherActivePoints(GameObject pointX, GameObject pointY)
    {
        if (!activePoints.Exists(a => a.name == pointX.name.ToUpper()))
            activePoints.Add(pointX);
        if (!activePoints.Exists(a => a.name == pointY.name.ToUpper()))
            activePoints.Add(pointY);
    }

    void ShowOnlyActivePoints(string atlas_name)
    {
        var points = GameObject.FindGameObjectsWithTag(atlas_name);

        foreach (var point in points)
        {
            if (!activePoints.Exists(a => a.name == point.name))
            {
                point.SetActive(false);
            }
        }
    }

    void RemoveExistingCorrelation()
    {
        if (transform.childCount == 0)
            return;

        foreach (Transform relation in transform)
        {
            Destroy(relation.gameObject);
        }
    }

    void Trigger_Existing_Correlation(string atlas_name)
    {
        if (atlas_name == Current_Atlas)
        {
            gameObject.SetActive(true);

            Configure_Current_Correlation(atlas_name);
            return;
        }

        gameObject.SetActive(false);
    }

    void Configure_Current_Correlation(string atlas_name)
    {
        activePoints = new List<GameObject>();
        foreach (var relation in global.Current_Correlations)
        {
            Find_Gather_Points(relation, out GameObject x, out GameObject y);
        }
        ShowOnlyActivePoints(atlas_name);
    }

    #endregion Gather Remove Find Points

    
}