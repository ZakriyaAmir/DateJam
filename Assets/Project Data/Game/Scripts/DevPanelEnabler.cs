using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class DevPanelEnabler
    {
        private const string MenuName = "Actions/Show Dev Panel";
        private const string SettingName = "IsDevPanelDisplayed";

        public static bool IsDevPanelDisplayed()
        {
#if UNITY_EDITOR
            return IsDevPanelDisplayedPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool IsDevPanelDisplayedPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 201)]
        private static void ToggleAction()
        {
            bool devPanelState = IsDevPanelDisplayedPrefs;
            IsDevPanelDisplayedPrefs = !devPanelState;
        }

        [MenuItem(MenuName, true, priority = 201)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsDevPanelDisplayedPrefs);

            return !Application.isPlaying;
        }
#endif
    }
}