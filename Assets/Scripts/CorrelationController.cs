using Assets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CorrelationController : MonoBehaviour
{
    private const double epsilon = 0.0001f;
    private string Current_Atlas;
    private IEnumerable<Corelation> Current_Correlations;
    private List<GameObject> activePoints;
    private double low, mid, high, mid_low;

    public delegate void OnActivateAction();
    public static event OnActivateAction ActivateAllPoints;

    public delegate void OnWeightThrUpdateAction(double low, double mid, double high);
    public static event OnWeightThrUpdateAction UpdateWeightThr;

    private void Awake()
    {
        SideMenuController.OnPlotCorrelation += PlotCorrelations;
        SideMenuController.OnChangeAtlas += Trigger_Existing_Correlation;
        SideMenuController.ApplyThr_bool += ApplyThr_bool;
        SideMenuController.ApplyThr_text += ApplyThr_text;
        SideMenuController.RestorePoints += RemoveExistingCorrelations;
    }

    private void RemoveExistingCorrelations(string atlas_name, IEnumerable<Regions> regions)
    {
        RemoveExistingCorrelation();
    }

    #region Threshold Events
    IEnumerator ApplyThr_bool(double thr_l, double thr_h, bool active)
    {
        if (!active)
        {
            var in_active_cor = Current_Correlations.Where(a => a.Weight >= thr_l && a.Weight <= thr_h);
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
        else
        {
            var in_active_cor = Current_Correlations.Where(a => a.Weight >= thr_l && a.Weight <= thr_h);
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

        
    }
    IEnumerator ApplyThr_text(double low_thr, double mid_thr, double high_thr)
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
            foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                foreach (var cor in Current_Correlations)
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
        if (Math.Abs(mid_thr - mid) > epsilon)
        {
            mid = mid_thr;
            mid_low = (mid + low) / 2;
            var cor_thr = Current_Correlations.Where(a => a.Weight > mid_low && a.Weight < mid);
            foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                foreach (var cor in Current_Correlations)
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
        if (Math.Abs(high_thr - high) > epsilon)
        {
            if (high_thr < high)
            {
                StartCoroutine(ApplyThr_bool(high_thr + 0.001f, high, false));
            }
            else
                StartCoroutine(ApplyThr_bool(high, high_thr, true));
            high = high_thr;
            var cor_thr = Current_Correlations.Where(a => a.Weight >= mid && a.Weight <= high);
            foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                foreach (var cor in Current_Correlations)
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

    private void Update()
    {
    }

    #region Configure Correlation

    private void PlotCorrelations(IEnumerable<Corelation> corelations, string current_atlas)
    {
        InitPlot(corelations, current_atlas);

        FindThresholdMarkers(corelations);

        foreach (var relation in corelations)
        {
            Load_Init_Prefab_Edge(relation, out GameObject pointX, out GameObject pointY,
                out Vector3 region_start, out Vector3 region_end, out GameObject edge);

            Configure_Transformation(relation, edge, region_start, region_end);

            Configure_RigidBody_Constraints(pointX, pointY, edge);
        }

        ShowOnlyActivePoints(current_atlas);
        
    }

    #region Threshold
    private void FindThresholdMarkers(IEnumerable<Corelation> corelations)
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

    private void FindThresholdMarkers_mid()
    {
        mid = (high + low) / 2;
        mid_low = (mid + low) / 2;
    }

    private double FindThresholdMarkers_high(double weight)
    {
        if (weight > high)
            return weight;
        return high;
    }

    private double FindThresholdMarkers_low(double weight)
    {
        if (weight < low)
            return weight;
        return low;
    } 
    #endregion

    private void InitPlot(IEnumerable<Corelation> corelations, string current_atlas)
    {
        RemoveExistingCorrelation();
        ActivateAllPoints();

        transform.parent.position = Vector3.zero;
        transform.parent.rotation = Quaternion.identity;
        transform.parent.localScale = new Vector3(40, 40, 40);

        var points_abr = GameObject.Find("Points").GetComponentsInChildren<TMP_Text>().ToList();

        foreach(var point in points_abr)
        {
            point.transform.rotation = new Quaternion(0, 180, 0, 0);
        }

        Current_Atlas = current_atlas;
        Current_Correlations = corelations;

        activePoints = new List<GameObject>();
    }

    private void Load_Init_Prefab_Edge(Corelation relation, out GameObject pointX, out GameObject pointY, out Vector3 region_start, out Vector3 region_end, out GameObject edge)
    {
        Find_Gather_Points(relation, out pointX, out pointY);

        region_start = pointX.transform.position;
        region_end = pointY.transform.position;
        edge = Instantiate(Resources.LoadAsync("Edge").asset as GameObject);
        edge.name = string.Concat(relation.PointX, "_", relation.PointY);
        edge.transform.parent = transform;
    }

    private void Configure_Transformation(Corelation relation, GameObject edge, Vector3 region_start, Vector3 region_end)
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

    private Vector3 ConfigureEdgeWeight(Corelation relation, GameObject edge, Vector3 scale)
    {
        if (relation.Weight >= low && relation.Weight <= mid_low)
        {
            scale.x = 0.8f;
            scale.z = 0.8f;
            SetEdgeColor(edge, Color.yellow);
        }
        if (relation.Weight > mid_low && relation.Weight <= mid)
        {
            scale.x = 2f;
            scale.z = 2f;
            SetEdgeColor(edge, Color.magenta);
        }
        if (relation.Weight > mid && relation.Weight <= high || Math.Abs(mid - high) < 0.00001)
        {
            scale.x = 3f;
            scale.z = 3f;
            SetEdgeColor(edge, Color.black);
        }
        return scale;
    }

    private static void SetEdgeColor(GameObject edge, Color color)
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetColor("_Color", color);
        edge.GetComponent<Renderer>().SetPropertyBlock(props);
    }

    private void Configure_RigidBody_Constraints(GameObject pointX, GameObject pointY, GameObject edge)
    {
        pointX.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();
        pointY.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();

        pointX.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        pointY.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
    }

    #endregion Configure Correlation

    #region Gather Remove Find Points

    private void Find_Gather_Points(Corelation relation, out GameObject pointX, out GameObject pointY)
    {
        pointX = GameObject.Find(relation.PointX.ToUpper());
        pointY = GameObject.Find(relation.PointY.ToUpper());

        GatherActivePoints(pointX, pointY);
    }

    private void GatherActivePoints(GameObject pointX, GameObject pointY)
    {
        if (!activePoints.Exists(a => a.name == pointX.name.ToUpper()))
            activePoints.Add(pointX);
        if (!activePoints.Exists(a => a.name == pointY.name.ToUpper()))
            activePoints.Add(pointY);
    }

    private void ShowOnlyActivePoints(string atlas_name)
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

    private void RemoveExistingCorrelation()
    {
        if (transform.childCount == 0)
            return;

        foreach (Transform relation in transform)
        {
            Destroy(relation.gameObject);
        }
    }

    private void Trigger_Existing_Correlation(string atlas_name)
    {
        if (atlas_name == Current_Atlas)
        {
            gameObject.SetActive(true);

            Configure_Current_Correlation(atlas_name);
            return;
        }

        gameObject.SetActive(false);
    }

    private void Configure_Current_Correlation(string atlas_name)
    {
        activePoints = new List<GameObject>();
        foreach (var relation in Current_Correlations)
        {
            Find_Gather_Points(relation, out GameObject x, out GameObject y);
        }
        ShowOnlyActivePoints(atlas_name);
    }

    #endregion Gather Remove Find Points
}