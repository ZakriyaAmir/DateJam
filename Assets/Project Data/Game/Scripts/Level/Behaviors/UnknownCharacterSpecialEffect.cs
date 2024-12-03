using UnityEngine;

namespace Watermelon.BusStop
{
    public sealed class UnknownCharacterSpecialEffect : ElementSpecialEffect
    {
        [SerializeField] Material customMaterial;
        [SerializeField] SpriteRenderer unknownMarkSpriteRenderer;

        private bool isActive;

        private TweenCase disableTweenCase;
        private BaseCharacterBehavior characterBehavior;

        public override void OnCreated()
        {
            transform.localPosition = new Vector3(0, 0, LevelData.BLOCK_OVERLAY_Z_OFFSET);

            characterBehavior = (BaseCharacterBehavior)linkedElement;
            characterBehavior.SetCustomMaterial(customMaterial);

            unknownMarkSpriteRenderer.color = Color.white;

            isActive = true;
        }

        public override void OnDisabled()
        {
            disableTweenCase.KillActive();

            characterBehavior.ResetMaterial();

            Destroy(gameObject);
        }

        private void DestroyCrate()
        {
            if (!isActive) return;

            isActive = false;

            characterBehavior.ResetMaterial();

            unknownMarkSpriteRenderer.DOFade(0.0f, 0.2f).OnComplete(() =>
            {
                linkedElement.DisableEffect();
            });
        }

        public override void OnMapUpdated() 
        {
            if(LevelController.HasEmptyNeighbor(linkedElement.ElementPosition))
            {
                DestroyCrate();
            }
        }

        public override void OnNeighborPicked(ElementPosition neighborPosition, LevelElement.Type neighborType)
        {
            DestroyCrate();
        }

        public override void OnHighlighted(bool firstSpawn) 
        { 
            if(firstSpawn)
            {
                linkedElement.DisableEffect();
            }
        }

        public override void OnUnhighlighted() { }
    }
}
