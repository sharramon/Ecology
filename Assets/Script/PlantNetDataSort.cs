using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlantNetDataSort : MonoBehaviour
{
    [SerializeField] private TextAsset _plantJson;
    public bool isUseSpoofFile;

    [Header("Text")]
    [SerializeField] private TMP_Text _commonName;
    [SerializeField] private TMP_Text _scientificName;

    [System.Serializable]
    public class PlantNetResult
    {
        public string bestMatch;
        public List<Result> results;
    }

    [System.Serializable]
    public class Result
    {
        public float score;
        public Species species;
    }

    [System.Serializable]
    public class Species
    {
        public string scientificName;
        public List<string> commonNames;
    }

    private void Start()
    {
        
    }

    public void SetJSON(TextAsset plantJSON)
    {
        _plantJson = plantJSON;
    }

    public void GetPlantName()
    {
        if (!isUseSpoofFile || _plantJson == null)
        {
            Debug.LogWarning("Spoof file disabled or no JSON assigned.");
            return;
        }

        PlantNetResult data = JsonConvert.DeserializeObject<PlantNetResult>(_plantJson.text);

        if (data.results != null && data.results.Count > 0)
        {
            var top = data.results[0];
            string scientificName = top.species.scientificName;

            List<string> commonNames = top.species.commonNames ?? new List<string>();
            string common = (commonNames.Count > 0)
                ? commonNames[0]
    :           "No common name available.";

            Debug.Log($"Best match:\nScientific: {scientificName}, Common: {common}");

            _commonName.text = common;
            _scientificName.text = scientificName;
        }
        else
        {
            Debug.LogWarning("No results found in JSON.");
        }
    }
    
    public void SetAndSortJSON(string jsonString)
    {
        PlantNetResult data = JsonConvert.DeserializeObject<PlantNetResult>(jsonString);

        if (data.results != null && data.results.Count > 0)
        {
            var top = data.results[0];
            string scientificName = top.species.scientificName;

            List<string> commonNames = top.species.commonNames ?? new List<string>();
            string common = (commonNames.Count > 0) ? commonNames[0] : "No common name available.";

            Debug.Log($"Best match:\nScientific: {scientificName}, Common: {common}");

            _commonName.text = common;
            _scientificName.text = scientificName;
        }
        else
        {
            Debug.LogWarning("No results found in JSON.");
        }
    }

    public void ShowError(string code, string message)
    {
        _commonName.text = $"Error: {code}";
        _scientificName.text = message;
    }
}
