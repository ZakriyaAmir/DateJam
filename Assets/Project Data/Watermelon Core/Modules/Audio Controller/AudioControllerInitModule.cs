#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Core/Audio Controller")]
    public class AudioControllerInitModule : InitModule
    {
        [SerializeField] AudioSettings audioSettings;

        [Space]
        [SerializeField] bool playRandomMusic = true;

        public override void CreateComponent(Initialiser initialiser)
        {
            AudioController audioController = new AudioController();
            audioController.Initialise(audioSettings, initialiser.gameObject);

            // Create audio listener
            AudioController.CreateAudioListener();

            if (playRandomMusic)
                AudioController.PlayRandomMusic();
        }

        public AudioControllerInitModule()
        {
            moduleName = "Audio Controller";
        }
    }
}