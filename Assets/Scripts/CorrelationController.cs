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
        //DeactivateAllPoints();
        foreach(var relation in corelations)
        {
            var pointX = GameObject.Find(relation.PointX);
            var pointY = GameObject.Find(relation.PointY);

            pointX.SetActive(true);
            pointY.SetActive(true);

            var region_start = pointX.transform.position;
            var region_end = pointY.transform.position;

            var edge = Instantiate(Resources.Load("Edge") as GameObject);
            edge.name = String.Concat(relation.PointX, "_", relation.PointY);
            edge.transform.parent = transform;

            var offset = region_end - region_start;
            var position = (region_start + region_end)/2;
            var scale = edge.transform.localScale;

            scale.y = offset.magnitude;
            edge.transform.localScale = scale;
            edge.transform.position = position;

            edge.transform.rotation = Quaternion.FromToRotation(Vector3.up, offset);

            pointX.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();
            pointY.AddComponent<FixedJoint>().connectedBody = edge.GetComponent<Rigidbody>();

        }
    }

   
    private void DeactivateAllPoints()
    {
        var points = GameObject.Find("Points").GetComponentsInChildren<Transform>();

        foreach(var point in points)
        {
            if(point.tag == "Point")
            {
                point.gameObject.SetActive(false);
            }
        }
    }
}
