#pragma warning disable 649

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class CellTypesHandler
    {
        private const string SELECTED = " (Selected)";
        private const int BUTTON_HEIGHT = 24;

        public int selectedCellTypeValue;
        public List<CellType> cellTypes;
        public List<ExtraProp> extraProps;
        private GUIStyle whiteLabelStyle;
        private GUIStyle blackLabelStyle;

        public CellTypesHandler()
        {
            cellTypes = new List<CellType>();
            extraProps = new List<ExtraProp>();
        }

        public void AddCellType(CellType cellType)
        {
            cellTypes.Add(cellType);
        }

        public void AddExtraProp(ExtraProp extraProp)
        {
            extraProps.Add(extraProp);
        }

        public void DrawCellButtons()
        {
            foreach (CellType cellType in cellTypes)
            {
                DrawCellButton(cellType);
            }
        }

        public void SetDefaultLabelStyle()
        {
            whiteLabelStyle = new GUIStyle(GUI.skin.label);
            blackLabelStyle = new GUIStyle(GUI.skin.label);
            whiteLabelStyle.alignment = TextAnchor.MiddleCenter;
            blackLabelStyle.alignment = TextAnchor.MiddleCenter;
            whiteLabelStyle.normal.textColor = Color.white;
            whiteLabelStyle.hover.textColor = Color.white;
            whiteLabelStyle.active.textColor = Color.white;
            whiteLabelStyle.focused.textColor = Color.white;
            blackLabelStyle.normal.textColor = Color.black;
            blackLabelStyle.hover.textColor = Color.black;
            blackLabelStyle.active.textColor = Color.black;
            blackLabelStyle.focused.textColor = Color.black;
            whiteLabelStyle.wordWrap = true;
            blackLabelStyle.wordWrap = true;
        }

        private void DrawCellButton(CellType cellType)
        {
            Rect rect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Space(BUTTON_HEIGHT);
            EditorGUILayout.EndVertical();

            LevelEditorBase.DrawColorRect(rect, cellType.color);
            GetLabelStyle(cellType.color);

            if (selectedCellTypeValue == cellType.value)
            {
                GUI.Label(rect, cellType.label + SELECTED, GetLabelStyle(cellType.color));
            }
            else
            {
                GUI.Label(rect, cellType.label, GetLabelStyle(cellType.color));
            }

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                selectedCellTypeValue = cellType.value;
            }
        }

        public GUIStyle GetLabelStyle(Color color)
        {
            if (((color.r + color.b + color.g) / 3.0f) > 0.5f)
            {
                return blackLabelStyle;
            }
            else
            {
                return whiteLabelStyle;
            }
        }

        public CellType GetCellType(int value)
        {
            for (int i = 0; i < cellTypes.Count; i++)
            {
                if(cellTypes[i].value == value)
                {
                    return cellTypes[i];
                }
            }

            return null;
        }

        public ExtraProp GetExtraProp(int value)
        {
            for (int i = 0; i < extraProps.Count; i++)
            {
                if (extraProps[i].value == value)
                {
                    return extraProps[i];
                }
            }

            return null;
        }

        public class CellType
        {
            public int value;
            public string label;
            public Color color;
            public bool extraPropsEnabled;

            public CellType(int value, string label, Color color,bool extraPropsEnabled = false)
            {
                this.value = value;
                this.label = label;
                this.color = color;
                this.extraPropsEnabled = extraPropsEnabled;
            }
        }

        public class ExtraProp
        {
            public int value;
            public string label;
            public bool display;

            public ExtraProp(int value, string label, bool display = true)
            {
                this.value = value;
                this.label = label;
                this.display = display;
            }
        }
    }
}