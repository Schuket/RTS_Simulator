using UnityEngine;

public class GroupFormation: MonoBehaviour
{
    public enum Type
    {
        SQUARE = 0,
        CIRCLE,
        ARROW
    }
    private Type _currType;

    [SerializeField] private float _distBetweenUnits = 2f;

    private GameObject[] _unitTargetPosition;

    public void SetUpFormation(Type type, int nbOfUnits)
    {
        _currType = type;
        _unitTargetPosition = new GameObject[nbOfUnits];

        for(int i = 0; i < nbOfUnits; i++)
        {
            GameObject gao = new GameObject();
            gao.transform.parent = this.transform;
            _unitTargetPosition[i] = gao;
        }

        transform.rotation = Quaternion.identity;

        switch (type)
        {
            case Type.SQUARE:
                SetUpSquareFormation();
                break;
            case Type.CIRCLE:
                SetUpCircleFormation();
                break;
            case Type.ARROW:
                SetUpArrowFormation();
                break;
            default:
                break;
        }
    }

    public void ChangeFormation(Type type)
    {
        if (type == _currType)
            return;

        _currType = type;

        //Enable Formation to be oriented in the right way
        transform.rotation = Quaternion.identity; 

        switch (type)
        {
            case Type.SQUARE:
                SetUpSquareFormation();
                break;
            case Type.CIRCLE:
                SetUpCircleFormation();
                break;
            case Type.ARROW:
                SetUpArrowFormation();
                break;
            default:
                break;
        }
    }

    public Vector3 GetPositionOfUnitFromIndex(int idx)
    {
        return _unitTargetPosition[idx].transform.position;
    }

    #region FormationPositionCalculation

    private void SetUpSquareFormation()
    {
        Vector3[] positionArray = new Vector3[_unitTargetPosition.Length];

        //Simulate a Spirale
        float offset = _distBetweenUnits;
        int unitByLine = 1;
        int lineLeft = 2;
        Vector3 dir = Vector3.back * offset;

        positionArray[0] = new Vector3(0f, 0f, 0f);

        for (int i = 1; i < positionArray.Length; i++)
        {
            positionArray[i] = positionArray[i - 1] + dir;

            if(i % unitByLine == 0)
            {
                lineLeft--;
                if(lineLeft == 0)
                {
                    unitByLine++;
                    lineLeft = 2;
                }

                if (dir.normalized == Vector3.back)        dir = Vector3.left * offset;
                else if (dir.normalized == Vector3.left)   dir = Vector3.forward * offset;
                else if (dir.normalized == Vector3.forward) dir = Vector3.right * offset;
                else if (dir.normalized == Vector3.right)  dir = Vector3.back * offset;
            }
        }

        SetFormationPosition(positionArray);
    }

    private void SetUpCircleFormation()
    {
        Vector3[] positionArray = new Vector3[_unitTargetPosition.Length];

        float zOffset = _distBetweenUnits;
        int zCount = 0;
        float offsetRotation = 45f;
        int rotCount = 0;

        for (int i = 0; i < positionArray.Length; i++)
        {
            positionArray[i] = Quaternion.Euler(0f, rotCount * offsetRotation, 0f) * new Vector3(0f, 0f, zCount * zOffset);

            rotCount++;

            // To Improve, could reduce offsetRotation, so that the more unit are far from center, the less is the angle beetwen each of them
            if ((i == 0) || (i >= 8 && i % 8 == 0)) 
            {
                zCount++;
                rotCount = 0;
            }
        }

        SetFormationPosition(positionArray);
    }

    private void SetUpArrowFormation()
    {
        Vector3[] positionArray = new Vector3[_unitTargetPosition.Length];

        float xOffset = _distBetweenUnits /1.5f;
        float zOffset = _distBetweenUnits /1.5f;
        int xCount = 0;
        int zCount = 0;

        for (int i = 0; i < positionArray.Length; i++)
        {
            positionArray[i] = new Vector3(xCount * xOffset, 0f, zCount * -zOffset);

            if (i % 2 == 0)
            {
                if (i < 6)
                    xCount++;
                zCount++;
            }

            xOffset *= -1f;

            if (i == 6)
            {
                xCount = 1;
                zCount = 2;
                xOffset *= 0.8f;
                zOffset *= 1.5f;
            }
        }

        SetFormationPosition(positionArray);
    }

    private void SetFormationPosition(Vector3[] positions)
    {
        //To Improve : Assign _unitTargetPosition in function of Units distance with the position -> center of formation is for the closest unit, etc ...
        for (int i = 0; i < _unitTargetPosition.Length; i++)
            _unitTargetPosition[i].transform.position = transform.position + positions[i];
    }
    #endregion
}
