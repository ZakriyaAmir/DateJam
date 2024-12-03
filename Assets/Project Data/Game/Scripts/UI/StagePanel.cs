using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class StagePanel
    {
        [SerializeField] Transform containerTransform;
        [SerializeField] GameObject stagePrefab;
        [SerializeField] GameObject spacerPrefab;

        private Pool stagePool;
        private Pool spacerPool;

        private StageBehavior[] stages;

        public void Initialise()
        {
            stagePool = new Pool(new PoolSettings(stagePrefab, 1, true, containerTransform));
            spacerPool = new Pool(new PoolSettings(spacerPrefab, 1, true, containerTransform));
        }

        public void Spawn(int amount)
        {
            Clear();

            stages = new StageBehavior[amount];
            for (int i = 0; i < amount; i++)
            {
                GameObject stageObject = stagePool.GetPooledObject();
                stageObject.transform.SetAsLastSibling();

                StageBehavior stageBehavior = stageObject.GetComponent<StageBehavior>();
                stageBehavior.SetDefaultColor();

                stages[i] = stageBehavior;

                if (i + 1 < amount)
                {
                    GameObject spacerObject = spacerPool.GetPooledObject();
                    spacerObject.transform.SetAsLastSibling();
                }
            }

            if (amount == 1)
                stages[0].gameObject.SetActive(false);
        }

        public void Activate(int index)
        {
            if(stages.IsInRange(index))
            {
                stages[index].SetActiveColor();
            }
        }

        public void Clear()
        {
            stagePool.ReturnToPoolEverything();
            spacerPool.ReturnToPoolEverything();
        }
    }
}
