using Assets.Func_Area_Model;
using AutoMapperFactory;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsController : MonoBehaviour
{
    //public GameObject Func_Area;
    IEnumerable<Regions> regions;
    const float rotateSpeed = 200;
    Matrix<float> rTheta;

    // Start is called before the first frame update
    private void Awake()
    {
        regions = new List<Regions>();
        LoadFunc_Area_Pos();
    }

    private void LoadFunc_Area_Pos()
    {
        //regions = ExcelFactory<Regions>.Map("..//Assets//Regions//desikan_atlas.csv");
        TextAsset data_raw = Resources.Load<TextAsset>("Data");
        string[] data = data_raw.text.Split(new char[] { '\n' });
        regions = MapperFactory<Regions>.Map_CSV(data);
    }

    // Start is called before the first frame update
    private void Start()
    {
        Plot();
    }
    private void Update()
    {
        //Rotation();
    }
    private void Rotation()
    {
        if (Input.GetMouseButton(0))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Mathf.Deg2Rad;
            float rotateY = Input.GetAxis("Mouse Y") * rotateSpeed * Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotateX);
            transform.Rotate(Vector3.right, -rotateY);
        }
    }

    private void Plot()
    {
        int i = 1;
        foreach (var region in regions)
        {
            if (i <= 1)
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

                Func_Area.transform.SetParent(this.transform);
                var pos = TransformR(inputVector, "X", 90);
                pos = TransformR(pos, "Y", 180);
                Func_Area.transform.position = new Vector3(pos[0,0],pos[1,0],pos[2,0]);
                
                //i++;
            }
            var c = new Vector3((float)3, (float)4, (float)5);
            gameObject.transform.position = c;
            //Func_Area.transform.SetParent(gameObject.Find("Points").transform);
        }
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
