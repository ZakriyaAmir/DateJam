using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILevelNumberText : MonoBehaviour
    {
        private const string LEVEL_LABEL = "LEVEL {0}";
        private static UILevelNumberText instance;

        [SerializeField] UIScaleAnimation uIScalableObject;

        private static UIScaleAnimation UIScalableObject => instance.uIScalableObject;
        private static TextMeshProUGUI levelNumberText;

        private static bool IsDisplayed = false;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<TextMeshProUGUI>();
        }


        private void OnEnable()
        {
            GameController.OnLevelChangedEvent += UpdateLevelNumber;
        }

        private void OnDisable()
        {
            GameController.OnLevelChangedEvent -= UpdateLevelNumber;
        }

        public static void Show(bool immediately = false)
        {
            if (IsDisplayed)
                return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(scaleMultiplier: 1.05f, immediately: immediately);
        }

        public static void Hide(bool immediately = false)
        {
            if (!IsDisplayed)
                return;

            IsDisplayed = false;

           UIScalableObject.Hide(scaleMultiplier: 1.05f, immediately: immediately, onCompleted: delegate
           {
               levelNumberText.enabled = false;
           });
        }

        private void UpdateLevelNumber()
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, LevelController.DisplayLevelNumber + 1);
        }

    }
}
