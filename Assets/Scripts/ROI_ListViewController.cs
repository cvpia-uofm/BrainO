using Assets.Models;
using Assets.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ROI_ListViewController : MonoBehaviour
{
    public RectTransform ItemPrefab_ROI;
    public RectTransform Content;
    public RectTransform ScrollView;
    public Scrollbar ScrollBar;
    public GameObject Region_obj_holder;

    [Inject]
    readonly IGlobal global;

    void Awake()
    {
        ROIsController.Populate_ROI_ListView += ROIsController_Populate_ROI_ListView;
    }

    void ROIsController_Populate_ROI_ListView()
    {
        if (global.ROIActivated)
        {
            Populate_ROI_List_fig(global.Current_rOIs);
        }
    }

    void Populate_ROI_List_fig(IList<ROI> current_rOIs)
    {
        foreach (var roi in current_rOIs)
        {
            var item = Instantiate(ItemPrefab_ROI.gameObject) as GameObject;
            var region = item.GetComponentsInChildren<Text>().Single(a => a.name == "Region");
            var factor = item.GetComponentsInChildren<Text>().Single(a => a.name == "Factor");
            var legend = item.GetComponentsInChildren<Image>().Single(a => a.name == "Legend");

            region.text = roi.Region.ToUpper();
            factor.text = roi.Importance_factor;

            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            Region_obj_holder.GetComponentsInChildren<Transform>().Single(a => a.name.ToUpper() == roi.Region.ToUpper()).GetComponent<Renderer>().GetPropertyBlock(properties);
            legend.GetComponent<Image>().color = properties.GetColor("_Color");
            item.transform.SetParent(Content.transform);
        }
    }
}
