using UnityEngine;
using UnityEngine.UI;

public class BuildPointsText : MonoBehaviour
{
    [SerializeField] UnitController PlayerController;
    Text nbOfBuildPoints;
    
    private void Start()
    {
        nbOfBuildPoints = GetComponent<Text>();
    }

    private void Update()
    {
        nbOfBuildPoints.text = "Player Build Points = " + PlayerController.TotalBuildPoints.ToString();
    }
}
