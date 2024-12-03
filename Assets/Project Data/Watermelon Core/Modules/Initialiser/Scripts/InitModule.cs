using UnityEngine;

namespace Watermelon
{
    public abstract class InitModule : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        protected string moduleName;

        public abstract void CreateComponent(Initialiser initialiser);

        public InitModule()
        {
            moduleName = "Default Module";
        }
    }
}