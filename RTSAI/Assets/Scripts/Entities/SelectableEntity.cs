using UnityEngine;

public abstract class SelectableEntity : MonoBehaviour
{
    [SerializeField]
    protected Team team;
    public Team GetTeam { get { return team; } }

    protected bool isSelected = false;
    protected Material material;
    protected Color baseColor = Color.white;

    private Color GetTeamColor()
    {
        return team == Team.Red ? Color.red : Color.green;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        material.color = isSelected ? GetTeamColor() : baseColor;
    }

    virtual protected void Awake()
    {
        material = GetComponent<Renderer>().material;
        baseColor = material.color;
    }
}
