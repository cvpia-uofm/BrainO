using Assets.Func_Area_Model;
//using ExcelFactoryLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using AutoMapperFactory;

public class BrainController : MonoBehaviour
{
    public Transform Func_Areas_Pos;
    public GameObject Func_Area;
    const float rotateSpeed = 200;
    IEnumerable<Regions> regions;

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
        
    }
    private void Update()
    {

        if (Input.GetMouseButton(0))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Mathf.Deg2Rad;
            float rotateY = Input.GetAxis("Mouse Y") * rotateSpeed * Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotateX);
            transform.Rotate(Vector3.right, -rotateY); 
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            gameObject.transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
            //gameObject.transform.Translate(Input.mousePosition.x, Input.mousePosition.y, Input.GetAxis("Mouse ScrollWheel") * 30f); 
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            gameObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0.5f);
            //gameObject.transform.Translate(Input.mousePosition.x, Input.mousePosition.y, Input.GetAxis("Mouse ScrollWheel") * 30f); 
        }
    }
}
