using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Watermelon
{
    public class LivesIndicator : MonoBehaviour
    {
        [Space]
        [SerializeField] TextMeshProUGUI livesCountText;
        [SerializeField] Image infinityImage;
        [SerializeField] TextMeshProUGUI durationText;

        [Space]
        [SerializeField] Button addButton;
        [SerializeField] AddLivesPanel addLivesPanel;

        private LivesData Data { get; set; }

        private bool isInitialised;

        public void Init(LivesData data)
        {
            if (isInitialised) return;
            
            Data = data;

            if(addLivesPanel != null)
            {
                addButton.gameObject.SetActive(true);
                addButton.onClick.AddListener(() => addLivesPanel.Show());
            } else
            {
                addButton.gameObject.SetActive(false);
            }

            isInitialised = true;
        }

        public void SetInfinite(bool isInfinite)
        {
            infinityImage.gameObject.SetActive(isInfinite);
            livesCountText.gameObject.SetActive(!isInfinite);
        }

        public void SetLivesCount(int count)
        {
            if (!isInitialised) return;

            livesCountText.text = count.ToString();

            addButton.gameObject.SetActive(count != Data.maxLivesCount && addLivesPanel != null);
            if(count == Data.maxLivesCount)
            {
                FullText();
            }
        }

        public void SetDuration(TimeSpan duration) 
        {
            if (!isInitialised) return;

            if (duration >= TimeSpan.FromHours(1))
            {
                durationText.text = string.Format(Data.longTimespanFormat, duration);
            }
            else
            {
                durationText.text = string.Format(Data.timespanFormat, duration);
            }

            SetTextSize(!addButton.gameObject.activeSelf);
        }

        public void FullText()
        {
            if (!isInitialised) return;

            durationText.text = Data.fullText;

            SetTextSize(true);
        }

        private void SetTextSize(bool fullPanel)
        {
            if(fullPanel)
            {
                durationText.rectTransform.offsetMin = new Vector2(70, 0);
                durationText.rectTransform.offsetMax = new Vector2(-38, 0);
            }
            else
            {
                durationText.rectTransform.offsetMin = new Vector2(95, 0);
                durationText.rectTransform.offsetMax = new Vector2(-100, 0);
            }
        }

        private void OnEnable()
        {
            LivesManager.AddIndicator(this);
        }

        private void OnDisable()
        {
            LivesManager.RemoveIndicator(this);
        }
    }
}