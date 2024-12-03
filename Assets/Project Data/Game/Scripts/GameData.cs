using UnityEngine;
using Watermelon.SkinStore;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Content/Game Data")]
    public class GameData : ScriptableObject
    {
        [SerializeField] bool activateVehicles;
        public bool ActivateVehicles => activateVehicles;

        public void OnValidate()
        {
            SkinStoreHelper.SetTabState(SkinTab.Bus, activateVehicles);
        }
    }
}
