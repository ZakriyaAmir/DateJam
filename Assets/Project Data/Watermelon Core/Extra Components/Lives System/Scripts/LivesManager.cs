using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class LivesManager : MonoBehaviour
    {
        private static LivesManager instance;

        [SerializeField] LivesData data;

        private static LivesSave save;

        public static int Lives { get => save.livesCount; private set => SetLifes(value); }
        private static DateTime LivesDate { get => save.date; set => save.date = value; }

        private static Coroutine livesCoroutine;

        private static List<LivesIndicator> indicators = new List<LivesIndicator>();
        private static List<AddLivesPanel> addLivesPanels = new List<AddLivesPanel>();

        public static bool IsMaxLives => Lives == instance.data.maxLivesCount;

        private void Awake()
        {
        }

        private void Start()
        {
            instance = this;

            save = SaveController.GetSaveObject<LivesSave>("Lives");
            save.Init(data);

            // For init purposses
            SetLifes(Lives);

            if (save.infiniteLives)
            {
                Tween.InvokeCoroutine(InfiniteLivesCoroutine());
            } else if (Lives < data.maxLivesCount)
            {
                livesCoroutine = Tween.InvokeCoroutine(LivesCoroutine());
            }
        }

        public static void AddPanel(AddLivesPanel panel)
        {
            if(!addLivesPanels.Contains(panel)) addLivesPanels.Add(panel);

            if(instance == null)
            {
                Tween.NextFrame(() => {
                    panel.SetLivesCount(Lives);
                });
            } else
            {
                panel.SetLivesCount(Lives);
            }
        }

        public static void RemovePanel(AddLivesPanel panel)
        {
            addLivesPanels.Remove(panel);
        }

        public static void AddIndicator(LivesIndicator indicator)
        {
            if(!indicators.Contains(indicator)) indicators.Add(indicator);

            if(instance == null)
            {
                Tween.NextFrame(() => {
                    indicator.Init(instance.data);

                    indicator.SetLivesCount(Lives);

                    indicator.SetInfinite(save.infiniteLives);
                });
            } else
            {
                indicator.Init(instance.data);

                indicator.SetLivesCount(Lives);
                indicator.SetInfinite(save.infiniteLives);
            }
        }

        public static void RemoveIndicator(LivesIndicator indicator)
        {
            indicators.Remove(indicator);
        }

        private static void SetLifes(int value)
        {
            save.livesCount = value;

            foreach(var indicator in indicators)
            {
                indicator.SetLivesCount(Lives);
                indicator.SetInfinite(save.infiniteLives);
            }

            foreach(var panel in addLivesPanels)
            {
                panel.SetLivesCount(value);
            }
        }

        public static void RemoveLife()
        {
            if (save.infiniteLives || !DoNotSpendLivesMenu.CanLivesBeSpent()) return;

            Lives--;

            if (Lives < 0)
                Lives = 0;

            if (livesCoroutine == null)
            {
                LivesDate = DateTime.Now;
                livesCoroutine = Tween.InvokeCoroutine(instance.LivesCoroutine());
            }
        }

        public static void AddLife()
        {
            if (Lives < instance.data.maxLivesCount)
                Lives++;
        }

        private IEnumerator InfiniteLivesCoroutine()
        {
            var wait = new WaitForSeconds(0.25f);
            while (DateTime.Now < save.date) 
            {
                var span = save.date - DateTime.Now;

                foreach(var indicator in indicators)
                {
                    indicator.SetDuration(span);
                }

                yield return wait;
            }

            save.infiniteLives = false;

            SetLifes(data.maxLivesCount);

            foreach (var indicator in indicators)
            {
                indicator.FullText();
            }

            foreach (var panel in addLivesPanels)
            {
                panel.SetTime(data.fullText);
            }

            yield return LivesCoroutine();
        }

        private IEnumerator LivesCoroutine()
        {
            var oneLifeSpan = TimeSpan.FromSeconds(data.oneLifeRestorationDuration);

            var wait = new WaitForSeconds(0.25f);
            while (Lives < data.maxLivesCount)
            {
                var timespan = DateTime.Now - LivesDate;

                if (timespan >= oneLifeSpan)
                {
                    Lives++;

                    LivesDate = DateTime.Now;
                }

                foreach (var indicator in indicators)
                {
                    indicator.SetDuration(oneLifeSpan - timespan);
                }

                foreach (var panel in addLivesPanels)
                {
                    panel.SetTime(string.Format(data.timespanFormat, oneLifeSpan - timespan));
                }

                yield return wait;
            }

            foreach (var indicator in indicators)
            {
                indicator.FullText();
            }

            foreach (var panel in addLivesPanels)
            {
                panel.SetTime(data.fullText);
            }

            livesCoroutine = null;
        }

        public static void StartInfiniteLives(float duration)
        {
            instance.InfiniteLives(duration);
        }

        private void InfiniteLives(float duration)
        {
            save.infiniteLives = true;
            save.date = DateTime.Now + TimeSpan.FromSeconds(duration);

            SetLifes(data.maxLivesCount);

            if (livesCoroutine != null)
            {
                Tween.StopCustomCoroutine(livesCoroutine);
                livesCoroutine = null;
            }
            Tween.InvokeCoroutine(InfiniteLivesCoroutine());
        }

        private class LivesSave : ISaveObject
        {
            public int livesCount;
            public bool infiniteLives;

            public long dateBinary;
            public DateTime date;

            [SerializeField] bool firstTime = true;

            public void Init(LivesData data)
            {
                if (firstTime)
                {
                    firstTime = false;

                    livesCount = data.maxLivesCount;
                    date = DateTime.Now;
                }
                else
                {
                    date = DateTime.FromBinary(dateBinary);

                    if (infiniteLives)
                    {
                        livesCount = data.maxLivesCount;

                        if (DateTime.Now >= date) infiniteLives = false;
                    }

                    if (livesCount < data.maxLivesCount)
                    {
                        var timeDif = DateTime.Now - date;

                        var oneLifeSpan = TimeSpan.FromSeconds(data.oneLifeRestorationDuration);

                        while (timeDif >= oneLifeSpan && livesCount < data.maxLivesCount)
                        {
                            timeDif -= oneLifeSpan;
                            date += oneLifeSpan;

                            livesCount++;
                        }
                    }
                }
            }

            public void Flush()
            {
                dateBinary = date.ToBinary();
            }
        }

        #region Development

        [Button("Add Life")]
        private void AddLifeDev()
        {
            AddLife();
        }

        [Button("Remove Life")]
        private void RemoveLifeDev()
        {
            RemoveLife();
        }

        #endregion
    }


}