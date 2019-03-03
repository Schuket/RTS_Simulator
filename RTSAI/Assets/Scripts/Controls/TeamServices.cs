using UnityEngine;

public enum Team
{
    Red,
    Green,

    Neutral
}

public class TeamServices : MonoBehaviour
{
    static TeamServices instance = null;

    UnitController [] ControllersArray;

    public static UnitController GetControllerByTeam(Team team)
    {
        if (instance.ControllersArray.Length < (int)team)
            return null;
        return instance.ControllersArray[(int)team];
    }

    public static Team GetOpponent(Team team)
    {
        if (team == Team.Green)
            return Team.Red;
        return Team.Green;
    }

    void Awake ()
    {
        instance = this;
        ControllersArray = new UnitController[2];
        foreach (UnitController controller in FindObjectsOfType<UnitController>())
        {
            ControllersArray[(int)controller.GetTeam] = controller;
        }
    }
}
