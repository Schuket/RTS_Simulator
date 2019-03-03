using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    [SerializeField]
    private float CaptureGaugeStart = 100f;
    [SerializeField]
    private float CaptureGaugeSpeed = 1f;
    [SerializeField]
    private int BuildPoints = 5;

    private float CaptureGaugeValue;
    private Team owningTeam = Team.Neutral;
    private Team capturingTeam = Team.Neutral;

    private Material mat;

    private int[] teamScore;

	void Start ()
    {
        mat = GetComponent<Renderer>().material;
        CaptureGaugeValue = CaptureGaugeStart;
        teamScore = new int[2];
        teamScore[0] = 0;
        teamScore[1] = 0;
    }

    void Update () {
        if (capturingTeam == owningTeam || capturingTeam == Team.Neutral)
            return;

        CaptureGaugeValue -= teamScore[(int)capturingTeam] * CaptureGaugeSpeed;

        if (CaptureGaugeValue <= 0f)
        {
            CaptureGaugeValue = 0f;
            OnCaptured(capturingTeam);
        }
    }

    private void ResetCapture()
    {
        CaptureGaugeValue = CaptureGaugeStart;
        capturingTeam = Team.Neutral;
    }

    void OnCaptured(Team newTeam)
    {
        if (owningTeam != newTeam)
        {
            UnitController teamController = TeamServices.GetControllerByTeam(newTeam);
            if (teamController != null)
                teamController.AddBuildPoints(BuildPoints);

            if (owningTeam != Team.Neutral)
            {
                // remove points to previously owning team
                teamController = TeamServices.GetControllerByTeam(owningTeam);
                if (teamController != null)
                    teamController.AddBuildPoints(-BuildPoints);
            }
        }

        owningTeam = newTeam;
        mat.color = newTeam == Team.Green ? Color.green : Color.red;
    }

    private void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (unit == null)
            return;

        teamScore[(int)unit.GetTeam] += unit.GetCost;

        if (capturingTeam == Team.Neutral)
        {
            if (teamScore[(int)TeamServices.GetOpponent(unit.GetTeam)] == 0)
                capturingTeam = unit.GetTeam;
        }
        else
        {
            if (teamScore[(int)TeamServices.GetOpponent(unit.GetTeam)] > 0)
                ResetCapture();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (unit == null)
            return;

        teamScore[(int)unit.GetTeam] -= unit.GetCost;
        if (teamScore[(int)unit.GetTeam] == 0)
        {
            Team opponentTeam = TeamServices.GetOpponent(unit.GetTeam);
            if (teamScore[(int)opponentTeam] == 0)
                ResetCapture();
            else
                capturingTeam = opponentTeam;
        }
    }
}
