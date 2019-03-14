using Assets.Models;
using AutoMapperFactory;
using ExcelFactory;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class SideMenuController : MonoBehaviour
{
    public delegate void OnPlotAction(IEnumerable<Corelation> corelations);
    public static event OnPlotAction OnPlotCorrelation;

    private GameObject Left_Hemph;
    private GameObject Right_Hemph;
    
    private IEnumerable<Corelation> Corelations;
    private void Awake()
    {
        Left_Hemph = GameObject.FindGameObjectWithTag("Left_Hemp");
        Right_Hemph = GameObject.FindGameObjectWithTag("Right_Hemp");
        Corelations = new List<Corelation>();
        
    }
    public void Left_Hemph_Activation(bool active) => Left_Hemph.SetActive(active);
    public void Right_Hemph_Activation(bool active) => Right_Hemph.SetActive(active);

    public void Load_Corelation()
    {
        var extensions = new[] {
            new ExtensionFilter("CSV Files", "csv" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (path == null)
            return;

        using (var reader = new StreamReader(path[0]))
        {
            var raw = reader.ReadToEnd();
            string[] data = raw.Split('\n');
            Corelations = MapperFactory<Corelation>.Map_CSV(data, MapperEnums.Inputs.Correlations);

            if ((Corelations as List<Corelation>).Count != 0)
                OnPlotCorrelation(Corelations);
        }

    }

    private int CountRows(StreamReader reader)
    {
        int count = 0;
        while (!String.IsNullOrWhiteSpace(reader.ReadLine()))
            count++;
        return count;
    }
}
