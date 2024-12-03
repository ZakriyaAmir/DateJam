using UnityEngine;

namespace Watermelon
{
    public class EditorCustomStyles
    {
        public const string ICON_SPACE = "  ";

        private EditorCustomStylesData editorCustomStyles;

        private Color iconColor;

        private GUISkin guiSkin;
        public GUISkin Skin => guiSkin;

        public GUIStyle tab;

        public GUIStyle box;
        public GUIStyle box_01;
        public GUIStyle box_02;
        public GUIStyle box_03;

        public GUIStyle labelCentered;

        public GUIStyle label_small;
        public GUIStyle label_small_bold;

        public GUIStyle label_medium;
        public GUIStyle label_medium_bold;

        public GUIStyle label_large;
        public GUIStyle label_large_bold;

        public GUIStyle button_01;
        public GUIStyle button_01_large;
        public GUIStyle button_01_mini;

        public GUIStyle button_02;
        public GUIStyle button_02_large;
        public GUIStyle button_02_mini;

        public GUIStyle button_03;
        public GUIStyle button_03_large;
        public GUIStyle button_03_mini;

        public GUIStyle button_04;
        public GUIStyle button_04_large;
        public GUIStyle button_04_mini;

        public GUIStyle button_05;
        public GUIStyle button_05_large;
        public GUIStyle button_05_mini;

        public GUIStyle helpbox;
        public GUIStyle helpboxLabel;

        public GUIStyle button_tab;

        public GUIStyle boxHeader;

        public GUIStyle padding00;
        public GUIStyle padding05;
        public GUIStyle padding10;

        public GUIStyle panelBottom;

        public GUIStyle boxCompiling;

        public EditorCustomStyles()
        {
            editorCustomStyles = EditorUtils.GetAsset<EditorCustomStylesData>();

            if(editorCustomStyles == null)
            {
                Debug.LogError("[Custom Editor]: Failed to load EditorCustomStyles scriptable object!");

                return;
            }

            iconColor = editorCustomStyles.IconColor;
            guiSkin = editorCustomStyles.Skin;

            InitStyles();
        }

        public void InitStyles()
        {
            if (guiSkin == null) return;

            tab = guiSkin.GetStyle("Tab");

            box = guiSkin.box;

            box_01 = guiSkin.GetStyle("box_01");
            box_02 = guiSkin.GetStyle("box_02");
            box_03 = guiSkin.GetStyle("box_03");

            label_small = guiSkin.GetStyle("label_small");
            label_small_bold = guiSkin.GetStyle("label_small_bold");

            label_medium = guiSkin.GetStyle("label_medium");
            label_medium_bold = guiSkin.GetStyle("label_medium_bold");

            label_large = guiSkin.GetStyle("label_large");
            label_large_bold = guiSkin.GetStyle("label_large_bold");

            button_01 = guiSkin.GetStyle("button_01");
            button_01_large = guiSkin.GetStyle("button_01_large");
            button_01_mini = button_01_large.ConvertButtonToMini();

            button_02 = guiSkin.GetStyle("button_02");
            button_02_large = guiSkin.GetStyle("button_02_large");
            button_02_mini = button_02_large.ConvertButtonToMini();

            button_03 = guiSkin.GetStyle("button_03");
            button_03_large = guiSkin.GetStyle("button_03_large");
            button_03_mini = button_03_large.ConvertButtonToMini();

            button_04 = guiSkin.GetStyle("button_04");
            button_04_large = guiSkin.GetStyle("button_04_large");
            button_04_mini = button_04_large.ConvertButtonToMini();

            button_05 = guiSkin.GetStyle("button_05");
            button_05_large = guiSkin.GetStyle("button_05_large");
            button_05_mini = button_05_large.ConvertButtonToMini();

            helpbox = guiSkin.GetStyle("helpbox");

            helpboxLabel = new GUIStyle(label_small);
            helpboxLabel.fontSize = 12;
            helpboxLabel.alignment = TextAnchor.MiddleLeft;
            helpboxLabel.margin = new RectOffset(3, 3, 2, 2);
            helpboxLabel.padding = new RectOffset(1, 1, 0, 0);
            helpboxLabel.stretchWidth = false;
            helpboxLabel.wordWrap = true;

            button_tab = guiSkin.GetStyle("button_tab");

            boxHeader = guiSkin.GetStyle("boxHeader");

            panelBottom = guiSkin.GetStyle("panelButton");

            labelCentered = new GUIStyle(guiSkin.label);
            labelCentered.alignment = TextAnchor.MiddleCenter;

            padding00 = new GUIStyle().GetPaddingStyle(new RectOffset(0, 0, 0, 0));
            padding05 = new GUIStyle().GetPaddingStyle(new RectOffset(0, 0, 5, 5));
            padding10 = new GUIStyle().GetPaddingStyle(new RectOffset(0, 0, 10, 10));

            boxCompiling = guiSkin.GetStyle("boxCompiling");
        }

        public Texture2D GetIcon(string name)
        {
            if (editorCustomStyles == null) return null;

            return editorCustomStyles.GetIcon(name);
        }

        public Texture2D GetIcon(string name, Color color)
        {
            if (editorCustomStyles == null) return null;

            return editorCustomStyles.GetIcon(name).ChangeColor(color);
        }
    }
}
