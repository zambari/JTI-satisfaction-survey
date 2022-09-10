using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public class SurveryResponse
{
    public string dateTime;
    public int response;
}

[System.Serializable]
public class SurveryResults : SavableJson<SurveryResults>
{
    public override string fileName => "surveys.json";
    public List<SurveryResponse> responses = new List<SurveryResponse>();

    public void AddResponse(int satisfaction)
    {
        Debug.Log($"adding response {satisfaction}");
        var dateString = System.DateTime.Now.ToString("s", CultureInfo.InvariantCulture);
        responses.Add(new SurveryResponse() { dateTime = dateString, response = satisfaction });
        Save();
    }
}