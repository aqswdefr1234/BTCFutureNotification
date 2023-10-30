using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using TMPro;

public class GetPriceHTML : MonoBehaviour
{
    [SerializeField] private TMP_Text current_Future_BTC_Price;
    //[SerializeField] private TMP_Text candle_Futre_BTC;
    private string url = "https://fapi.binance.com/fapi/v1/ticker/price?symbol=BTCUSDT";
    void Start()
    {
        StartCoroutine(GetCurrentPriceBTCFutre());
        //StartCoroutine(GetCandleData("1d", 20));
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
    /*
    IEnumerator GetCandleData(string _interval, int _limit)
    {
        string candleUrl = $"https://fapi.binance.com/fapi/v1/klines?symbol=BTCUSDT&interval={_interval}&limit={_limit}";
        UnityWebRequest request;
        WaitForSeconds delay = new WaitForSeconds(10f);
        string getString = "";
        //string price = "";

        while (true)
        {
            request = UnityWebRequest.Get(candleUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
            }
            else
            {
                getString = request.downloadHandler.text;
                Debug.Log(getString);
                //price = getString.Substring(getString.IndexOf("price") + 8, 8);
                candle_Futre_BTC.text = getString;
            }
            yield return delay;
        }
    }*/
}
