using Assets.Models;
using Assets.Models.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class RegionListController : MonoBehaviour
{
    public RectTransform ItemPrefab;
    public RectTransform Content;
    public RectTransform ScrollView;
    public Scrollbar ScrollBar;

    public InputField SearchField;

    public delegate IEnumerator OnPathofRegionAction(string region_name);
    public static event OnPathofRegionAction OnRegionSelected;

    public delegate void OnFocusRegion(Region region);
    public static event OnFocusRegion OnFocusPoint;

    public delegate void OnFocusROI(ROI sel_rOI, ROI prev_sel_rOi);
    public static event OnFocusROI OnFocus_rOI;

    public delegate void OnRestorePreviousState(string region_name);
    public static event OnRestorePreviousState RestorePreviousStateofRegion;

    [Inject]
    readonly IAtlas atlas;
    [Inject]
    readonly IGlobal global;

    ROI Prev_Sel_rOI = null;

    void Awake()
    {
        SideMenuController.OnChangeAtlas += SideMenuController_OnChangeAtlas;
        SideMenuController.RestorePoints += SideMenuController_RestorePoints;
        CorrelationController.Update_Regionlist += CorrelationController_Update_Regionlist;
    }


    void SideMenuController_RestorePoints(string atlas_name, IEnumerable<Region> regions)
    {
        SearchField.text = "";
        Populate_Region_list(regions);
    }

    void CorrelationController_Update_Regionlist(IEnumerable<Region> atlas_regions)
    {
        if (atlas_regions.Count() != 0)
        {
            Populate_Region_list(atlas_regions);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl))
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.name == "ItemPrefab(Clone)")
                {
                    PreviousSelectedItem = SelectedItem;
                    return;
                }
            }
            if (PreviousSelectedItem == null)
                return;
            if (PreviousSelectedItem.name == "ItemPrefab(Clone)")
            {
                if (SelectedItem == null || SelectedItem.name != "ItemPrefab(Clone)")
                {
                    string region_name = PreviousSelectedItem.GetComponentInChildren<Text>().text;

                    if (true)
                    {
                        RestorePreviousStateofRegion(region_name);
                    }
                }
            }
        }
    }

    void Start()
    {
        Populate_Region_list(atlas.Desikan_Atlas);
    }

    void SideMenuController_OnChangeAtlas(string atlas_name)
    {
        Populate_Region_list(global.Atlas_Regions_value_pairs[atlas_name]);
    }

    void Populate_Region_list(IEnumerable<Region> atlas_regions)
    {
        foreach (Transform ext_region in Content.transform)
        {
            Destroy(ext_region.gameObject);
        }
        foreach (var region in atlas_regions)
        {
            Construct_Region_for_view(region);
        }

        ScrollBar.value = 1;
    }

    void Construct_Region_for_view(Region region)
    {
        var item = Instantiate(ItemPrefab.gameObject) as GameObject;
        var region_name_item = item.GetComponentsInChildren<Text>().SingleOrDefault(a => a.name == "Region_name");

        region_name_item.text = region.Abbreviation;

        //var region_weight_item = item.GetComponentsInChildren<Text>().SingleOrDefault(a => a.name == "Region_weight");
        //region_weight_item.gameObject.SetActive(false);

        var btn_comp = item.GetComponent<Button>();
        btn_comp.onClick.AddListener(delegate { SelectRegionAction(region_name_item.text); });

        item.transform.SetParent(Content.transform);
    }

    void SelectRegionAction(string region_name)
    {
        global.AnyRegionSelected = true;

        if (!global.CorrelationActivated && !global.ROIActivated)
        {
            OnFocusPoint(global.Current_Region_list.SingleOrDefault(a => a.Abbreviation.ToUpper() == region_name.ToUpper()));
            return;
        }
        if (global.ROIActivated && !global.CorrelationActivated)
        {
            var roi = global.Current_rOIs.SingleOrDefault(a => a.Region.ToLower() == region_name.ToLower());
            OnFocus_rOI(roi, Prev_Sel_rOI);
            Prev_Sel_rOI = roi;
            return;
        }
        StartCoroutine(OnRegionSelected(region_name));
    }

    public void OnSearch(string txt)
    {
        List<Region> result = null;
        if (!global.CorrelationActivated)
        {
            result = global.Current_Region_list.Where(a => a.Abbreviation.ToUpper().Contains(txt.ToUpper())).ToList();
        }
        else
        {
            result = global.Current_Active_Regions.Where(a => a.Abbreviation.ToUpper().Contains(txt.ToUpper())).ToList();
        }
        Populate_Region_list(result);
    }

    GameObject SelectedItem => EventSystem.current.currentSelectedGameObject;
    GameObject PreviousSelectedItem;

}
