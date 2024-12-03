using UnityEngine;

namespace Watermelon.BusStop
{
    public abstract class LevelElementBehavior : MonoBehaviour
    {
        protected LevelElement levelElement;
        public LevelElement LevelElement => levelElement;

        protected ElementPosition elementPosition;
        public ElementPosition ElementPosition => elementPosition;

        protected bool isHighlighted;
        public bool IsHighlighted => isHighlighted;

        protected ElementSpecialEffect specialEffect;
        public ElementSpecialEffect SpecialEffect => specialEffect;

        protected bool isSubmitted;
        public bool IsSubmitted => isSubmitted;

        public virtual void Initialise(LevelElement levelElement, ElementPosition elementPosition)
        {
            this.levelElement = levelElement;
            this.elementPosition = elementPosition;

            isSubmitted = false;
        }

        public void SetPosition(ElementPosition newPosition)
        {
            elementPosition = newPosition;
        }

        public virtual void OnMapUpdated()
        {
            if (specialEffect != null)
            {
                specialEffect.OnMapUpdated();
            }
        }

        public virtual void SetExtraData(object extraData) { }

        public virtual void OnNeighborPicked(ElementPosition neighborPosition, LevelElement.Type neighborType) 
        {
            if(specialEffect != null)
            {
                specialEffect.OnNeighborPicked(neighborPosition, neighborType);
            }
        }

        public abstract void Unload();
        public abstract bool IsPlayableElement();

        public virtual void SetGraphicsState(bool state) { }

        public void PlaySpawnAnimation(float delay = 0)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f, delay).SetEasing(Ease.Type.BackOut);
        }

        public void MarkAsSubmitted()
        {
            isSubmitted = true;
        }

        public void ResetSubmitState()
        {
            isSubmitted = false;
        }

        #region Highlight
        public virtual void Highlight(bool firstSpawn)
        {
            isHighlighted = true;

            if (specialEffect != null)
            {
                specialEffect.OnHighlighted(firstSpawn);

                // Don't spawn particle if special effect blocks it
                if (specialEffect != null && !specialEffect.IsHighlightActive()) return;
            }
        }

        public virtual void Unhighlight()
        {
            isHighlighted = false;

            if (specialEffect != null)
            {
                specialEffect.OnUnhighlighted();
            }
        }
        #endregion

        #region Effects
        public void ApplyEffect(ElementSpecialEffect specialEffect)
        {
            DisableEffect();

            this.specialEffect = specialEffect;

            specialEffect.OnCreated();
        }

        public void DisableEffect()
        {
            if (specialEffect != null)
            {
                specialEffect.OnDisabled();
                specialEffect = null;
            }
        }
        #endregion
    }
}
