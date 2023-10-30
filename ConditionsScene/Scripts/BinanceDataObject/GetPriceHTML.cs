using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using TMPro;

public class GetPriceHTML : MonoBehaviour
{
    [SerializeField] private TMP_Text current_Future_BTC_Price;
    private string url = "https://fapi.binance.com/fapi/v1/ticker/price?symbol=BTCUSDT";
    void Start()
    {
        StartCoroutine(GetCurrentPriceBTCFutre());
    }
    IEnumerator GetCurrentPriceBTCFutre()
    {
        UnityWebRequest request;
        WaitForSeconds delay = new WaitForSeconds(5f);
        string getString = "";
        string price = "";

        while (true)
        {
            request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
            }
            else
            {
                getString = request.downloadHandler.text;
                Debug.Log(getString);
                price = getString.Substring(getString.IndexOf("price") + 8, 8);
                current_Future_BTC_Price.text = price;
            }
            yield return delay;
        }
    }
}
