using Assets.Func_Area_Model;
using Assets.Models;
using AutoMapperFactory;
using ExcelFactory;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PointsController : MonoBehaviour
{
    //public GameObject Func_Area;
    List<Regions> regions;
    Matrix<float> rTheta;
    List<TMP_Text> pointLabels;
    List<LineRenderer> lineObjects;
    // Start is called before the first frame update
    private void Awake()
    {
        pointLabels = new List<TMP_Text>();
        regions = new List<Regions>();
        lineObjects = new List<LineRenderer>();

        LoadFunc_Area_Pos();

        SideMenuController.OnPlotCorrelation += PlotCorrelations;
    }

    private void PlotCorrelations(IEnumerable<Corelation> corelations)
    {
        foreach(var cor in corelations)
        {
            var p1 = GameObject.Find(cor.PointX);
            var p2 = GameObject.Find(cor.PointY);

            var lineObj = new GameObject();
            lineObj.name = cor.PointX + "_" + cor.PointY;
            var lineR = lineObj.AddComponent<LineRenderer>();
            lineR.useWorldSpace = true;
            lineR.SetPositions(new Vector3[] { p1.transform.position, p2.transform.position });

            lineObjects.Add(lineR);
        }
    }

    private void LoadFunc_Area_Pos()
    {
        //regions = ExcelFactory<Regions>.Map("..//Assets//Regions//desikan_atlas.csv");
        TextAsset data_raw = Resources.Load<TextAsset>("Data");
        string[] data = data_raw.text.Split(new char[] { '\n' });
        regions = (List<Regions>) MapperFactory<Regions>.Map_CSV(data, MapperEnums.Inputs.Regions);
    }

    // Start is called before the first frame update
    private void Start()
    {
        BrainController.OnBrainRotate += RotateLabelsAndLines;
        Plot();
    }

    private void RotateLabelsAndLines(float X, float Y)
    {
        foreach (var point in pointLabels)
        {
            point.transform.Rotate(Vector3.up, X);
            point.transform.Rotate(Vector3.right, -Y);
        }

        foreach(var lineobj in lineObjects)
        {
            var pos_begin = lineobj.GetPosition(0);
            var pos_end = lineobj.GetPosition(1);
            var mat_begin = Matrix<float>.Build.DenseOfArray(new float[,]
            {
                { pos_begin.x }, {pos_begin.y}, {pos_begin.y}, { 1 }
            });
            var mat_end = Matrix<float>.Build.DenseOfArray(new float[,]
            {
                { pos_end.x }, {pos_end.y}, {pos_end.y}, { 1 }
            });
            var T_mat_begin = TransformR(mat_begin, "X", X);
            T_mat_begin = TransformR(T_mat_begin, "Y", -Y);

            var T_mat_end = TransformR(mat_end, "X", X);
            T_mat_end = TransformR(T_mat_end, "Y", -Y);


            var new_pos_1 = new Vector3(T_mat_begin[0, 0], T_mat_begin[1, 0], T_mat_begin[2, 0]);
            var new_pos_2 = new Vector3(T_mat_end[0, 0], T_mat_end[1, 0], T_mat_end[2, 0]);
            lineobj.SetPositions(new Vector3[] { new_pos_1, new_pos_2 });
        }
    }
    private void Plot()
    {
        foreach (var region in regions)
        {
            float X = (float)region.X;
            float Y = (float)region.Y;
            float Z = (float)region.Z;
            Matrix<float> inputVector = Matrix<float>.Build.DenseOfArray(new float[,] {
                { X }, { Y }, { Z }, { 1 }
            });
            GameObject Func_Area = Instantiate(Resources.Load("Point")) as GameObject;
            Func_Area.name = region.Abbreviation;

            //Instantiate(Func_Area, Func_Area.transform);
            if (region.Hemisphere.Equals("left"))
            {
                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetColor("_Color", Color.red);
                Func_Area.GetComponent<Renderer>().SetPropertyBlock(props);
            }
            var canvas = Func_Area.transform.Find("Canvas");
            var textobj = canvas.GetComponentInChildren<TMP_Text>();
            textobj.text = region.Abbreviation;
            pointLabels.Add(textobj);

            Func_Area.transform.SetParent(this.transform);
            var pos = TransformR(inputVector, "X", 90);
            pos = TransformR(pos, "Y", 180);
            Func_Area.transform.position = new Vector3(pos[0,0],pos[1,0],pos[2,0]);
               
           
        }
        var c = new Vector3(0.8f, 1.8f, 1.48f);
        gameObject.transform.localPosition = c;
    }

    private Matrix<float> TransformR(Matrix<float> inputVector, string Axis, float angle)
    {
        angle = angle * Mathf.PI / 180;

        switch (Axis)
        {
            case "Z":
                rTheta = Matrix<float>.Build.DenseOfArray(new float[,] {
                    { Mathf.Cos(angle), Mathf.Sin(angle), 0, 0 },
                    { -Mathf.Sin(angle), Mathf.Cos(angle), 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1 }
                });
                break;
            case "Y":
                rTheta = Matrix<float>.Build.DenseOfArray(new float[,] {
                    { Mathf.Cos(angle), 0, -Mathf.Sin(angle), 0 },
                    { 0, 1, 0, 0 },
                    { Mathf.Sin(angle), 0, Mathf.Cos(angle), 0 },
                    { 0, 0, 0, 1 }
                });
                break;
            case "X":
                rTheta = Matrix<float>.Build.DenseOfArray(new float[,] {
                    { 1, 0, 0, 0 },
                    { 0, Mathf.Cos(angle), Mathf.Sin(angle), 0 },
                    { 0, -Mathf.Sin(angle), Mathf.Cos(angle), 0 },
                    { 0, 0, 0, 1 }
                });
                break;
        }


        var pos = rTheta.Multiply(inputVector);
        return pos;
    }
}
