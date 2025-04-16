using System.Collections.Generic;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public static class MetaIAPManager
{
    public enum IAPType { Consumable, Durable }

    public enum SKUs
    {
        Consumable_1,
        Consumable_2,
        Durable_1,
        Durable_2,
    }

    private static readonly Dictionary<SKUs, string> SkuMap = new()
    {
        { SKUs.Consumable_1, "powerball_pack_1" },
        { SKUs.Consumable_2, "powerball_pack_2" },
        { SKUs.Durable_1,    "premium_board_1"  },
        { SKUs.Durable_2,    "premium_board_2"  },
    };

    public static Dictionary<string, Product> Products = new();
    public static HashSet<string> PurchasedSkus = new();

    public static void Init()
    {
        FetchProductPrices();
        FetchPurchasedProducts();
    }

    public static void FetchProductPrices()
    {
        var skus = new List<string>(SkuMap.Values).ToArray();
        IAP.GetProductsBySKU(skus).OnComplete(GetProductsBySKUCallback);
    }

    private static void GetProductsBySKUCallback(Message<ProductList> msg)
    {
        if (msg.IsError)
        {
            HandleError(msg);
            return;
        }

        foreach (Product p in msg.GetProductList())
        {
            Products[p.Sku] = p;
            Debug.LogFormat("Product: sku:{0} name:{1} price:{2}", p.Sku, p.Name, p.FormattedPrice);
        }
    }

    public static void FetchPurchasedProducts()
    {
        IAP.GetViewerPurchases().OnComplete(GetViewerPurchasesCallback);
    }

    private static void GetViewerPurchasesCallback(Message<PurchaseList> msg)
    {
        if (msg.IsError)
        {
            HandleError(msg);
            return;
        }

        foreach (Purchase p in msg.GetPurchaseList())
        {
            PurchasedSkus.Add(p.Sku);
            Debug.LogFormat("Purchased: sku:{0} granttime:{1} id:{2}", p.Sku, p.GrantTime, p.ID);
        }
    }

    public static void Purchase(SKUs sku)
    {
        if (!SkuMap.ContainsKey(sku)) return;
        IAP.LaunchCheckoutFlow(SkuMap[sku]).OnComplete(LaunchCheckoutFlowCallback);
    }

    private static void LaunchCheckoutFlowCallback(Message<Purchase> msg)
    {
        if (msg.IsError)
        {
            HandleError(msg);
            return;
        }

        var p = msg.GetPurchase();
        Debug.Log("Purchased: " + p.Sku);
    }

    private static void HandleError<T>(Message<T> msg)
    {
        var error = msg.GetError();
        Debug.LogError($"[IAP Error] {error.Code}: {error.Message}");
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}