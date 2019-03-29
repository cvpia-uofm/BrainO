﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Models;
using UnityEngine;

public class CorrelationController : MonoBehaviour
{
    private List<GameObject> activePoints;
    private string Current_Atlas;
    private IEnumerable<Corelation> Current_Correlations;

    private void Awake()
    {
        SideMenuController.OnPlotCorrelation += PlotCorrelations;
        SideMenuController.OnChangeAtlas += ResetCorrelation;
    }

    private void ResetCorrelation(string atlas_name)
    {
        if (atlas_name == Current_Atlas)
        {
            PlotCorrelations(Current_Correlations, Current_Atlas);
            return;
        }
            
        RemoveExistingCorrelation();

        
    }

    private void PlotCorrelations(IEnumerable<Corelation> corelations, string current_atlas)
    {
        InitPlot(corelations, current_atlas);

        foreach (var relation in corelations)
        {
            GameObject pointX, pointY, edge;
            Vector3 region_start, region_end;
            Load_Init_Prefab_Edge(relation, out pointX, out pointY, out region_start, out region_end, out edge);
            Configure_Transformation(edge, region_start, region_end);
            Configure_RigidBody_Constraints(pointX, pointY, edge);
        }

        ShowOnlyActivePoints(activePoints);
    }

    private void Configure_RigidBody_Constraints(GameObject pointX, GameObject pointY, GameObject edge)
    {
        pointX.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();
        pointY.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();

        pointX.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        pointY.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
    }

    private void Configure_Transformation(GameObject edge, Vector3 region_start, Vector3 region_end)
    {
        var offset = region_end - region_start;
        var position = (region_start + region_end) / 2;
        var scale = edge.transform.localScale;

        edge.transform.position = position;
        scale.y = Vector3.Distance(region_start, edge.transform.position);
        edge.transform.localScale = scale;
        edge.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset);
    }

    private void InitPlot(IEnumerable<Corelation> corelations, string current_atlas)
    {
        RemoveExistingCorrelation();

        Current_Atlas = current_atlas;
        Current_Correlations = corelations;

        activePoints = new List<GameObject>();
    }

    private void Load_Init_Prefab_Edge(Corelation relation, out GameObject pointX, out GameObject pointY, out Vector3 region_start, out Vector3 region_end, out GameObject edge)
    {
        pointX = GameObject.Find(relation.PointX);
        pointY = GameObject.Find(relation.PointY);
        GatherActivePoints(activePoints, pointX, pointY);

        region_start = pointX.transform.position;
        region_end = pointY.transform.position;
        edge = Instantiate(Resources.Load("Edge") as GameObject);
        edge.name = String.Concat(relation.PointX, "_", relation.PointY);
        edge.transform.parent = transform;
    }

    private void RemoveExistingCorrelation()
    {
        if (transform.childCount == 0)
            return;

        foreach(Transform relation in transform)
        {
            Destroy(relation.gameObject);
        }
    }

    private void GatherActivePoints(List<GameObject> activePoints, GameObject pointX, GameObject pointY)
    {
        if (!activePoints.Exists(a => a.name == pointX.name))
            activePoints.Add(pointX);
        if (!activePoints.Exists(a => a.name == pointY.name))
            activePoints.Add(pointY);
    }

    private void ShowOnlyActivePoints(List<GameObject> activePoints)
    {
        var points = GameObject.FindGameObjectsWithTag("Point");

        foreach(var point in points)
        {
            if (!activePoints.Exists(a => a.name == point.name))
            {
                point.SetActive(false);
                activePoints.Remove(point);
            }
        }
    }
}
