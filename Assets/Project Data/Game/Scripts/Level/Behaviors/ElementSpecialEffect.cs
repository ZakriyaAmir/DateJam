using System;
using UnityEngine;

namespace Watermelon.BusStop
{

    public abstract class ElementSpecialEffect : MonoBehaviour
    {
        [SerializeField] LevelData.SpecialEffectType effectType;
        public LevelData.SpecialEffectType EffectType => effectType;

        protected LevelElementBehavior linkedElement;

        public abstract void OnCreated();
        public abstract void OnDisabled();

        public abstract void OnMapUpdated();

        public virtual bool IsHighlightActive() { return true; }
        public abstract void OnHighlighted(bool firstSpawn);
        public abstract void OnUnhighlighted();

        public virtual void OnNeighborPicked(ElementPosition neighborPosition, LevelElement.Type neighborType) { }

        public void DisableEffect()
        {
            if (linkedElement != null)
            {
                linkedElement.DisableEffect();
            }
        }

        public void ApplyEffect(LevelElementBehavior elementBehavior)
        {
            GameObject effectObject = Instantiate(gameObject);
            effectObject.transform.SetParent(elementBehavior.transform);
            effectObject.transform.ResetLocal();

            ElementSpecialEffect elementSpecialEffect = effectObject.GetComponent<ElementSpecialEffect>();
            elementSpecialEffect.linkedElement = elementBehavior;

            elementBehavior.ApplyEffect(elementSpecialEffect);
        }
    }
}
