using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Models;
using UnityEngine;

public class CorrelationController : MonoBehaviour
{
    private void Awake()
    {
        SideMenuController.OnPlotCorrelation += PlotCorrelations;
        BrainController.OnBrainRotate += RotateLineRenderers;
    }

    private void RotateLineRenderers(float X, float Y)
    {
        //transform.Rotate(Vector3.up, X);
        //transform.Rotate(Vector3.right, -Y);
    }

    private void PlotCorrelations(IEnumerable<Corelation> corelations)
    {
        List<GameObject> activePoints = new List<GameObject>();
        foreach(var relation in corelations)
        {
            var pointX = GameObject.Find(relation.PointX);
            var pointY = GameObject.Find(relation.PointY);

            GatherActivePoints(activePoints, pointX, pointY);

            var region_start = pointX.transform.position;
            var region_end = pointY.transform.position;

            var edge = Instantiate(Resources.Load("Edge") as GameObject);
            edge.name = String.Concat(relation.PointX, "_", relation.PointY);
            edge.transform.parent = transform;

            var offset = region_end - region_start;
            var position = (region_start + region_end) / 2;
            var scale = edge.transform.localScale;

            edge.transform.position = position;
            scale.y = Vector3.Distance(region_start, edge.transform.position);
            edge.transform.localScale = scale;
            edge.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset);

            pointX.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();
            pointY.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();

        }

        ShowOnlyActivePoints(activePoints);
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
                point.SetActive(false);
        }
    }
}
