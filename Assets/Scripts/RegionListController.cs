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
    public static event OnPathofRegionAction OnPathAction;

    public delegate void OnFocusRegion(Regions region);
    public static event OnFocusRegion OnFocusPoint;


    [Inject]
    readonly IAtlas atlas;
    [Inject]
    readonly IGlobal global;

    void Awake()
    {
        SideMenuController.OnChangeAtlas += SideMenuController_OnChangeAtlas;
        SideMenuController.RestorePoints += SideMenuController_RestorePoints;
        CorrelationController.Update_Regionlist += CorrelationController_Update_Regionlist;

    }

    void SideMenuController_RestorePoints(string atlas_name, IEnumerable<Regions> regions)
    {
        SearchField.text = "";
        Populate_Region_list(regions);
    }

    void CorrelationController_Update_Regionlist(IEnumerable<Regions> atlas_regions)
    {
        if (atlas_regions.Count() != 0)
        {
            Populate_Region_list(atlas_regions);
        }
    }

    void Update()
    {
        if (IsMouseOver())
        {
            global.MouseOverUI = true;
        }
        else
            global.MouseOverUI = false;
    }
    void Start()
    {
        Populate_Region_list(atlas.Desikan_Atlas);
    }

    void SideMenuController_OnChangeAtlas(string atlas_name)
    {
        Populate_Region_list(global.Atlas_Regions_value_pairs[atlas_name]);
    }

    void Populate_Region_list(IEnumerable<Regions> atlas_regions)
    {
        foreach(Transform ext_region in Content.transform)
        {
            Destroy(ext_region.gameObject);
        }
        foreach (var region in atlas_regions)
        {
            Construct_Region_for_view(region);
        }

        ScrollBar.value = 1;
    }

    void Construct_Region_for_view(Regions region)
    {
        var item = Instantiate(ItemPrefab.gameObject) as GameObject;
        var region_name_item = item.GetComponentsInChildren<Text>().SingleOrDefault(a => a.name == "Region_name");

        region_name_item.text = region.Abbreviation;

        //var region_weight_item = item.GetComponentsInChildren<Text>().SingleOrDefault(a => a.name == "Region_weight");
        //region_weight_item.gameObject.SetActive(false);

        var btn_comp = item.GetComponent<Button>();
        btn_comp.onClick.AddListener(delegate { ShowPathofRegion(region_name_item.text); });

        item.transform.SetParent(Content.transform);
    }

    void ShowPathofRegion(string region_name)
    {
        if (!global.CorrelationActivated)
        {
            OnFocusPoint(global.Current_Region_list.SingleOrDefault(a => a.Abbreviation.ToUpper() == region_name.ToUpper()));
            return;
        }
        StartCoroutine(OnPathAction(region_name));
    }

    public void OnSearch(string txt)
    {
        List<Regions> result = null;
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

    bool IsMouseOver() => EventSystem.current.IsPointerOverGameObject();
}
