using Assets.Models;
using Assets.Models.Interfaces;
using AutoMapperFactory;
using ExcelFactory;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

public class PointsController : MonoBehaviour
{
    IAtlas atlas;
    [Inject]
    readonly IGlobal global;
    static Matrix<float> rTheta;
    List<TMP_Text> pointLabels;



    [Inject]
    public void Construct(IAtlas _atlas)
    {
        atlas = _atlas;
    }

    void Awake()
    {
        SideMenuController.OnChangeAtlas += ChangeAtlas;
        SideMenuController.RestorePoints += RestoreAtlasPoints;
        CorrelationController.ActivateAllPoints += ActivateAllPoints;
        CorrelationController.OnPathAction += CorrelationController_OnPathAction;
        Init_Atlas();
    }

    IEnumerator CorrelationController_OnPathAction(string[] regions)
    {
        foreach(var region in global.Current_Region_list)
        {
            foreach (Transform child in transform)
            {
                if(child.name == region.Abbreviation.ToUpper())
                {
                    Add_Color_to_hemp(region, child.gameObject);
                    break;
                }
            }
        }

        foreach(var region in regions)
        {
            var obj = transform.Find(region.ToUpper()).gameObject;
            obj.GetComponent<Renderer>().material.shader = Shader.Find("Custom/Outline");
            
        }
        yield return null;
    }
    void RestoreAtlasPoints(string atlas_name, IEnumerable<Regions> regions)
    {
        var correlations = GameObject.Find("Correlations").GetComponentsInChildren<Transform>(true).Where(a => a.name != "Correlations").ToList();
        Plot(regions, atlas_name);

    }

    void Start()
    {
        BrainController.OnBrainRotate += RotateLabels;
        Plot(atlas.Desikan_Atlas, Atlas.DSK_Atlas);
    }

    #region Events

    void ChangeAtlas(string atlas_name)
    {
        switch (atlas_name)
        {
            case Atlas.DSK_Atlas:
                Plot(atlas.Desikan_Atlas, Atlas.DSK_Atlas);
                break;

            case Atlas.DTX_Atlas:
                Plot(atlas.Destrieux_Atlas, Atlas.DTX_Atlas);
                break;

            case Atlas.CDK_Atlas:
                Plot(atlas.Craddock_Atlas, Atlas.CDK_Atlas);
                break;

            case Atlas.A116_Atlas:
                Plot(atlas.Aal116_Atlas, Atlas.A116_Atlas);
                break;

            case Atlas.A90_Atlas:
                Plot(atlas.Aal90_Atlas, Atlas.A90_Atlas);
                break;
        }
    }

    void ActivateAllPoints()
    {
        foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.SetActive(true);
        }
    }

    void RotateLabels(float X, float Y)
    {
        foreach (var point in pointLabels)
        {
            point.transform.Rotate(Vector3.up, X);
            point.transform.Rotate(Vector3.right, -Y);
        }
    }

    #endregion Events

    #region Initialize Atlas

    void Init_Atlas()
    {
        atlas.Desikan_Atlas = LoadAtlas(Atlas.DSK_Atlas);
        atlas.Destrieux_Atlas = LoadAtlas(Atlas.DTX_Atlas);
        atlas.Craddock_Atlas = LoadAtlas(Atlas.CDK_Atlas);
        atlas.Aal116_Atlas = LoadAtlas(Atlas.A116_Atlas);
        atlas.Aal90_Atlas = LoadAtlas(Atlas.A90_Atlas);
    }

    IEnumerable<Regions> LoadAtlas(string atlas_name)
    {
        TextAsset data_raw = Resources.Load<TextAsset>(atlas_name);
        string[] data = data_raw.text.Split(new char[] { '\n' });
        return MapperFactory<Regions>.Map_CSV(data, MapperEnums.Inputs.Regions);
    }

    #endregion Initialize Atlas

    #region Points Configuration

    void Plot(IEnumerable<Regions> atlas_regions, string atlas_name)
    {
        IniPlot(atlas_regions);

        foreach (var region in atlas_regions)
        {
            Matrix<float> inputVector;
            GameObject Func_Area;
            Load_Init_Point(region, atlas_name, out inputVector, out Func_Area);
            Add_Color_to_hemp(region, Func_Area);
            Add_Label_to_Point(region, Func_Area);
            Set_Tranform_of_Point(atlas_regions, inputVector, Func_Area);
        }

        Translate_Points();
    }

    private void IniPlot(IEnumerable<Regions> atlas_regions)
    {
        global.Current_Region_list = new List<Regions>(atlas_regions);
        RemoveExistingPlot();
        pointLabels = new List<TMP_Text>();
    }


    void Load_Init_Point(Regions region, string atlas_name, out Matrix<float> inputVector, out GameObject Func_Area)
    {
        float X = (float)region.X;
        float Y = (float)region.Y;
        float Z = (float)region.Z;
        inputVector = Matrix<float>.Build.DenseOfArray(new float[,] {
                { -X }, { Y }, { Z }, { 1 }
            });
        Func_Area = Instantiate(Resources.Load("Point")) as GameObject;
        Func_Area.name = region.Abbreviation.ToUpper();

        Func_Area.tag = atlas_name;
    }

    void Add_Color_to_hemp(Regions region, GameObject Func_Area)
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        Func_Area.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
        if (region.Hemisphere.Equals("left") || region.Abbreviation.StartsWith("l", StringComparison.CurrentCulture))
        {
            props.SetColor("_Color", Color.red);
            Func_Area.GetComponent<Renderer>().SetPropertyBlock(props);
        }
        if (region.Hemisphere.Equals("right") || region.Abbreviation.StartsWith("r", StringComparison.CurrentCulture))
        {
            props.SetColor("_Color", Color.blue);
            Func_Area.GetComponent<Renderer>().SetPropertyBlock(props);
        }
    }

    void Add_Label_to_Point(Regions region, GameObject Func_Area)
    {
        var canvas = Func_Area.transform.Find("Canvas");
        var textobj = canvas.GetComponentsInChildren<TMP_Text>().Single(a => a.name == "Abbreviation");
        textobj.text = region.Abbreviation;
        pointLabels.Add(textobj);
    }

    void Set_Tranform_of_Point(IEnumerable<Regions> atlas_regions, Matrix<float> inputVector, GameObject Func_Area)
    {
        Func_Area.transform.SetParent(this.transform);

        var pos = TransformR(inputVector, "X", 90);
        pos = TransformR(pos, "Y", -180);

        Func_Area.transform.position = new Vector3(pos[0, 0], pos[1, 0], pos[2, 0]);

        ScalePoints(Func_Area, atlas_regions.Count());
    }

    void ScalePoints(GameObject func_Area, int atlas_length)
    {
        if (atlas_length == 68)
        {
            func_Area.transform.localScale = new Vector3(5f, 5f, 5f);
            return;
        }
        if (atlas_length < 100)
        {
            func_Area.transform.localScale = new Vector3(4f, 4f, 4f);
            return;
        }
        if (atlas_length > 100)
        {
            func_Area.transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
            return;
        }
    }

    void Translate_Points()
    {
        var trans_Vertex = new Vector3(0.7f, 1.7f, 1.3f);
        transform.localPosition = trans_Vertex;
    }

    void RemoveExistingPlot()
    {
        if (transform.childCount == 0)
            return;
        SetDaefaultTransform();

        foreach (Transform point in transform)
        {
            Destroy(point.gameObject);
        }
    }

    void SetDaefaultTransform()
    {
        transform.parent.rotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.parent.localScale = new Vector3(40, 40, 40);
    }

    #endregion Points Configuration

    #region Axis Transformation

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

    #endregion Axis Transformation
}