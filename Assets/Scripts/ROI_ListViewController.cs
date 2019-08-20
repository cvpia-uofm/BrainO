using Assets.Models;
using Assets.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public RectTransform ListViewPanel;
    public Text Legend_txt_high;
    public Text Legend_txt_mid;
    public Text Legend_txt_low;

    [Inject]
    readonly IGlobal global;

    void Awake()
    {
        ROIsController.Populate_ROI_ListView += ROIsController_Populate_ROI_ListView;
        ROIsController.UpdateROIthrLgd += ROIsController_UpdateROIthrLgd;
    }

    void ROIsController_UpdateROIthrLgd(double low, double mid, double high)
    {
        Legend_txt_high.text = mid.ToString() + " ≤ thr ≤ " + high.ToString();
        Legend_txt_mid.text = ((mid + low) / 2).ToString() + " ≤ thr < " + mid.ToString();
        Legend_txt_low.text = low.ToString() + " ≤ thr < " + ((mid + low) / 2).ToString();
        Init_ListView();
        Populate_ROI_List_fig(global.Current_rOIs);
    }

    void ROIsController_Populate_ROI_ListView()
    {
        if (global.ROIActivated)
        {
            Init_ListView();
            Populate_ROI_List_fig(global.Current_rOIs);
        }
    }

    void Init_ListView()
    {
        var roi_items = ListViewPanel.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("ItemPrefabRoi"));
        foreach(var roi_item in roi_items)
        {
            Destroy(roi_item.gameObject);
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

            region.text = roi.Region;
            factor.text = roi.Importance_factor;

            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            Region_obj_holder.GetComponentsInChildren<Transform>().Single(a => a.name.Equals(roi.Region, StringComparison.CurrentCultureIgnoreCase)).GetComponent<Renderer>().GetPropertyBlock(properties);
            legend.GetComponent<Image>().color = properties.GetColor("_Color");
            item.transform.SetParent(Content.transform);
        }
    }
}
