using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class DoNotSpendLivesMenu
    {
        private const string MenuName = "Actions/Do Not Spend Lives";
        private const string SettingName = "CanLivesBeSpent";

        public static bool CanLivesBeSpent()
        {
#if UNITY_EDITOR
            return CanLiesBeSpentPrefs;
#else
            return true;
#endif
        }

#if UNITY_EDITOR
        private static bool CanLiesBeSpentPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, true); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 201)]
        private static void ToggleAction()
        {
            bool panelState = CanLiesBeSpentPrefs;
            CanLiesBeSpentPrefs = !panelState;
        }

        [MenuItem(MenuName, true, priority = 201)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, !CanLiesBeSpentPrefs);

            return true;
        }
#endif
    }
}