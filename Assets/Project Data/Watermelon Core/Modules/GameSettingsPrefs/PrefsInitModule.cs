using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Prefs Settings", Core = true)]
    public class PrefsInitModule : InitModule
    {
        [SerializeField] PrefsSettings prefsSettings;

        public override void CreateComponent(Initialiser Initialiser)
        {
            prefsSettings.Initialise();
        }

        public PrefsInitModule()
        {
            moduleName = "Prefs Settings";
        }
    }
}

// -----------------
// Prefs Settings v1.0
// -----------------