using Assets.Func_Area_Model;
using AutoMapperFactory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsController : MonoBehaviour
{
    public GameObject Func_Area;
    IEnumerable<Regions> regions;
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
        GenerateCo_Ord();
    }

    private void GenerateCo_Ord()
    {
        foreach (var region in regions)
        {
            float X = (float)(region.X);
            float Y = (float)(region.Y);
            float Z = (float)(region.Z);
            var pos = new Vector3(X, Y, Z);
            Func_Area.transform.position = pos;
            Instantiate(Func_Area, Func_Area.transform);
            Func_Area.transform.SetParent(gameObject.Find("Points").transform);
        }
    }
}
