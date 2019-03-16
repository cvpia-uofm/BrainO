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
    Atlas atlas;
    static Matrix<float> rTheta;
    List<TMP_Text> pointLabels;
    // Start is called before the first frame update
    private void Awake()
    {
        SideMenuController.OnChangeAtlas += ChangeAtlas;

        atlas = new Atlas();

        atlas.Desikan_Atlas = LoadAtlas(Atlas.DSK_Atlas);
        atlas.Destrieux_Atlas = LoadAtlas(Atlas.DTX_Atlas);
        atlas.Craddock_Atlas = LoadAtlas(Atlas.CDK_Atlas);
        atlas.Aal116_Atlas = LoadAtlas(Atlas.A116_Atlas);
        atlas.Aal90_Atlas = LoadAtlas(Atlas.A90_Atlas);
    }

    private void ChangeAtlas(string atlas_name)
    {
        switch (atlas_name)
        {
            case Atlas.DSK_Atlas:
                Plot(atlas.Desikan_Atlas);
                break;
            case Atlas.DTX_Atlas:
                Plot(atlas.Destrieux_Atlas);
                break;
            case Atlas.CDK_Atlas:
                Plot(atlas.Craddock_Atlas);
                break;
            case Atlas.A116_Atlas:
                Plot(atlas.Aal116_Atlas);
                break;
            case Atlas.A90_Atlas:
                Plot(atlas.Aal90_Atlas);
                break;
        }
    }

    private IEnumerable<Regions> LoadAtlas(string atlas_name)
    {
        //regions = ExcelFactory<Regions>.Map("..//Assets//Regions//desikan_atlas.csv");
        TextAsset data_raw = Resources.Load<TextAsset>(atlas_name);
        string[] data = data_raw.text.Split(new char[] { '\n' });
        return MapperFactory<Regions>.Map_CSV(data, MapperEnums.Inputs.Regions);
    }

    // Start is called before the first frame update
    private void Start()
    {
        BrainController.OnBrainRotate += RotateLabels;
        Plot(atlas.Desikan_Atlas);


    }

    private void RotateLabels(float X, float Y)
    {
        foreach (var point in pointLabels)
        {
            point.transform.Rotate(Vector3.up, X);
            point.transform.Rotate(Vector3.right, -Y);
        }

    }

    private void Plot(IEnumerable<Regions> atlas_regions)
    {
        RemoveExistingPlot();
        pointLabels = new List<TMP_Text>();
        foreach (var region in atlas_regions)
        {
            float X = (float)region.X;
            float Y = (float)region.Y;
            float Z = (float)region.Z;
            Matrix<float> inputVector = Matrix<float>.Build.DenseOfArray(new float[,] {
                { -X }, { Y }, { Z }, { 1 }
            });
            GameObject Func_Area = Instantiate(Resources.Load("Point")) as GameObject;
            Func_Area.name = region.Abbreviation;
            Func_Area.tag = "Point";
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
            pos = TransformR(pos, "Y", -180);
            
            Func_Area.transform.position = new Vector3(pos[0,0],pos[1,0],pos[2,0]);

            ScalePoints(Func_Area, atlas_regions.Count());
        }
        var trans_Vertex = new Vector3(0.7f, 1.7f, 1.3f);
        transform.localPosition = trans_Vertex;
    }

    private void ScalePoints(GameObject func_Area, int atlas_length)
    {
        if (atlas_length == 68)
            return;
        if(atlas_length < 100)
            func_Area.transform.localScale = new Vector3(4, 4, 4);
        if (atlas_length > 100)
            func_Area.transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
    }

    private void RemoveExistingPlot()
    {
        if (transform.childCount == 0)
            return;

        transform.parent.rotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;

        foreach(Transform point in transform)
        {
            Destroy(point.gameObject);
        }
    }

    public static Matrix<float> TransformR(Matrix<float> inputVector, string Axis, float angle)
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
