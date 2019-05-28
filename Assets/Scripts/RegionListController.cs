using Assets.Models;
using Assets.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RegionListController : MonoBehaviour
{
    public RectTransform ItemPrefab;
    public RectTransform Content;
    public RectTransform ScrollView;

    public InputField SearchField;

    public delegate IEnumerator OnPathofRegionAction(string region_name);
    public static event OnPathofRegionAction OnPathAction;


    [Inject]
    readonly IAtlas atlas;
    [Inject]
    readonly IGlobal global;

    void Awake()
    {
        SideMenuController.OnChangeAtlas += SideMenuController_OnChangeAtlas;
    }
    private void Start()
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
    }

    void Construct_Region_for_view(Regions region)
    {
        var item = Instantiate(ItemPrefab.gameObject) as GameObject;
        var region_name_item = item.GetComponentsInChildren<Text>().SingleOrDefault(a => a.name == "Region_name");

        region_name_item.text = region.Abbreviation;

        var region_weight_item = item.GetComponentsInChildren<Text>().SingleOrDefault(a => a.name == "Region_weight");
        region_weight_item.gameObject.SetActive(false);

        var btn_comp = item.GetComponent<Button>();
        btn_comp.onClick.AddListener(delegate { ShowPathofRegion(region_name_item.text); });

        item.transform.SetParent(Content.transform);
    }

    void ShowPathofRegion(string region_name)
    {
        StartCoroutine(OnPathAction(region_name));
    }
}
