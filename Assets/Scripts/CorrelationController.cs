﻿using Assets.Models;
using System.Collections.Generic;
using UnityEngine;

public class CorrelationController : MonoBehaviour
{
    private string Current_Atlas;
    private IEnumerable<Corelation> Current_Correlations;
    private List<GameObject> activePoints;

    public delegate void OnActivateAction();

    public static event OnActivateAction ActivateAllPoints;

    private void Awake()
    {
        SideMenuController.OnPlotCorrelation += PlotCorrelations;
        SideMenuController.OnChangeAtlas += Trigger_Existing_Correlation;
    }

    private void Update()
    {
    }

    #region Configure Correlation

    private void PlotCorrelations(IEnumerable<Corelation> corelations, string current_atlas)
    {
        InitPlot(corelations, current_atlas);

        foreach (var relation in corelations)
        {
            Load_Init_Prefab_Edge(relation, out GameObject pointX, out GameObject pointY,
                out Vector3 region_start, out Vector3 region_end, out GameObject edge);

            Configure_Transformation(relation, edge, region_start, region_end);

            Configure_RigidBody_Constraints(pointX, pointY, edge);
        }

        ShowOnlyActivePoints(current_atlas);
    }

    private void InitPlot(IEnumerable<Corelation> corelations, string current_atlas)
    {
        RemoveExistingCorrelation();
        ActivateAllPoints();

        transform.parent.position = Vector3.zero;
        transform.parent.rotation = Quaternion.identity;
        transform.parent.localScale = new Vector3(40, 40, 40);

        Current_Atlas = current_atlas;
        Current_Correlations = corelations;

        activePoints = new List<GameObject>();
    }

    private void Load_Init_Prefab_Edge(Corelation relation, out GameObject pointX, out GameObject pointY, out Vector3 region_start, out Vector3 region_end, out GameObject edge)
    {
        Find_Gather_Points(relation, out pointX, out pointY);

        region_start = pointX.transform.position;
        region_end = pointY.transform.position;
        edge = Instantiate(Resources.Load("Edge") as GameObject);
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
        scale.x = (float)relation.Weight;
        scale.z = (float)relation.Weight;
        edge.transform.localScale = scale;
        
        edge.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset);
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
        pointX = GameObject.Find(relation.PointX);
        pointY = GameObject.Find(relation.PointY);
        pointX.transform.localScale = new Vector3(7, 7, 7);
        GatherActivePoints(pointX, pointY);
    }

    private void GatherActivePoints(GameObject pointX, GameObject pointY)
    {
        if (!activePoints.Exists(a => a.name == pointX.name))
            activePoints.Add(pointX);
        if (!activePoints.Exists(a => a.name == pointY.name))
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