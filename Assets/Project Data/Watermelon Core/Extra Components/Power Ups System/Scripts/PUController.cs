using System.Collections.Generic;
using UnityEngine;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class PUController : MonoBehaviour
    {
        private static PUController instance;

        [DrawReference]
        [SerializeField] PUDatabase database;

        [LineSpacer("Sounds")]
        [SerializeField] AudioClip activateSound;

        private static PUBehavior[] activePowerUps;
        public static PUBehavior[] ActivePowerUps => activePowerUps;

        private static Dictionary<PUType, PUBehavior> powerUpsLink;

        private static PUUIController powerUpsUIController;
        public static PUUIController PowerUpsUIController => powerUpsUIController;

        private Transform behaviorsContainer;

        public static event OnPowerUpUsedCallback OnPowerUpUsed;

        public void Initialise()
        {
#if MODULE_POWERUPS
            instance = this;

            behaviorsContainer = new GameObject("[POWER UPS]").transform;
            behaviorsContainer.gameObject.isStatic = true;

            PUSettings[] powerUpSettings = database.PowerUps;
            activePowerUps = new PUBehavior[powerUpSettings.Length];
            powerUpsLink = new Dictionary<PUType, PUBehavior>();

            for (int i = 0; i < activePowerUps.Length; i++)
            {
                // Initialise power ups
                powerUpSettings[i].InitialiseSave();
                powerUpSettings[i].Initialise();

                // Spawn behavior object 
                GameObject powerUpBehaviorObject = Instantiate(powerUpSettings[i].BehaviorPrefab, behaviorsContainer);
                powerUpBehaviorObject.transform.ResetLocal();

                PUBehavior powerUpBehavior = powerUpBehaviorObject.GetComponent<PUBehavior>();
                powerUpBehavior.InitialiseSettings(powerUpSettings[i]);
                powerUpBehavior.Initialise();

                activePowerUps[i] = powerUpBehavior;

                // Add power up to dictionary
                powerUpsLink.Add(activePowerUps[i].Settings.Type, activePowerUps[i]);
            }

            UIGame gameUI = UIController.GetPage<UIGame>();

            powerUpsUIController = gameUI.PowerUpsUIController;
            powerUpsUIController.Initialise(this);
#else
            Debug.LogError("[PU Controller]: Module Define isn't active!");
#endif
        }

        public static bool PurchasePowerUp(PUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                if(powerUpBehavior.Settings.HasEnoughCurrency())
                {
                    CurrenciesController.Substract(powerUpBehavior.Settings.CurrencyType, powerUpBehavior.Settings.Price);

                    powerUpBehavior.Settings.Save.Amount += powerUpBehavior.Settings.PurchaseAmount;

                    powerUpsUIController.RedrawPanels();

                    return true;
                }
                else
                {
                    UIController.ShowPage<UIIAPStore>();

                    return false;
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }

            return false;
        }

        public static void AddPowerUp(PUType powerUpType, int amount)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount += amount;

                powerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void SetPowerUpAmount(PUType powerUpType, int amount)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount = amount;

                powerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void UsePowerUp(PUType powerUpType)
        {
            if(powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                if(!powerUpBehavior.IsBusy)
                {
                    if(powerUpBehavior.Activate())
                    {
                        AudioController.PlaySound(instance.activateSound, 0.45f);

                        powerUpBehavior.Settings.Save.Amount--;

                        powerUpsUIController.OnPowerUpUsed(powerUpBehavior);

                        OnPowerUpUsed?.Invoke(powerUpType);
                    }
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void ResetPowerUp(PUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                PUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount = 0;

                powerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void ResetPowerUps()
        {
            foreach(PUBehavior powerUp in activePowerUps)
            {
                powerUp.Settings.Save.Amount = 0;
            }

            powerUpsUIController.RedrawPanels();
        }

        public static PUBehavior GetPowerUpBehavior(PUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                return powerUpsLink[powerUpType];
            }

            Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));

            return null;
        }

        public static void ResetBehaviors()
        {
            for(int i = 0; i < activePowerUps.Length; i++)
            {
                activePowerUps[i].ResetBehavior();
            }
        }

        [Button("Give Test Amount")]
        public void GiveDebugAmount()
        {
            if (!Application.isPlaying) return;

            for(int i = 0; i < activePowerUps.Length; i++)
            {
                activePowerUps[i].Settings.Save.Amount = 999;
            }

            powerUpsUIController.RedrawPanels();
        }

        [Button("Reset Amount")]
        public void ResetDebugAmount()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < activePowerUps.Length; i++)
            {
                activePowerUps[i].Settings.Save.Amount = 0;
            }

            powerUpsUIController.RedrawPanels();
        }

        public delegate void OnPowerUpUsedCallback(PUType powerUpType);
    }
}

// -----------------
// PU Controller v1.2.1
// -----------------

// Changelog
// v 1.2.1
// • Added notch offset on mobile devices
// • Added Show, Hide methods to PUUIController
// v 1.2
// • Added isDirty state for UI panels (redraws automatically in Update)
// • Added visuals for busy state
// v 1.1
// • Added ResetPowerUp, ResetPowerUps, SetPowerUpAmount, GetPowerUpBehavior methods
// v 1.0
// • Basic PU logic