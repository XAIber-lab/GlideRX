using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AcquisitionRange : MonoBehaviour
{
    public DataPersistenceManager dataManager;

    private TextMeshProUGUI acquisitionRangeText;

    private void Awake()
    {
        acquisitionRangeText = this.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        acquisitionRangeText.text = "Acquisition Range: " + dataManager.gameData.acquisitionRange + 
            "\nClosing Rate: " + dataManager.gameData.closingRate +
            "\nAcquisition Time: " + dataManager.gameData.acquisitionTime;
    }
}
