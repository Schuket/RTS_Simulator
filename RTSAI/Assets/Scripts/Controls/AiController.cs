using UnityEngine;

public class AiController : UnitController
{
    [SerializeField] UnitFactory LightFactory;
    [SerializeField] UnitFactory MediumFactory;
    [SerializeField] UnitFactory HeavyFactory;

    float LightTimer;
    float MediumTimer;
    float HeavyTimer;


    protected override void Start ()
    {
        base.Start();

        LightTimer = LightFactory.GetBuildDuration;
        SpawnLight();

        MediumTimer = MediumFactory.GetBuildDuration;
        SpawnMedium();

        HeavyTimer = HeavyFactory.GetBuildDuration;
        SpawnHeavy();
	}

    private void SpawnLight()
    {
        currentFactory = LightFactory;
        RequestFactoryBuild();
        Invoke("SpawnLight", LightTimer);
    }

    private void SpawnMedium()
    {
        currentFactory = MediumFactory;
        RequestFactoryBuild();
        Invoke("SpawnMedium", MediumTimer);
    }

    private void SpawnHeavy()
    {
        currentFactory = HeavyFactory;
        RequestFactoryBuild();
        Invoke("SpawnHeavy", HeavyTimer);
    }
}
