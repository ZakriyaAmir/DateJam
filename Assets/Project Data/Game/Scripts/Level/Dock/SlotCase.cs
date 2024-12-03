using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Watermelon.BusStop.DockBehavior;
using Watermelon.BusStop;
using Watermelon;

namespace Watermelon
{
    public class SlotCase
    {
        public BaseCharacterBehavior Behavior { get; private set; }

        public bool IsBeingRemoved { get; set; }

        public bool IsMoving { get; set; }
        public MoveType MoveType { get; set; }

        private TweenCaseCollection elementMoveCases;

        public SlotCase(BaseCharacterBehavior behavior)
        {
            Behavior = behavior;
        }

        public void SubmitMove(Vector3 position, Vector3 scale, Vector3 rotation, bool instant = false)
        {
            IsMoving = true;
            MoveType = MoveType.Submit;

            var distance = Vector3.Distance(position, Behavior.transform.position);
            var duration = Mathf.Clamp(distance / 5f, 0.5f, 5f);

            if (elementMoveCases != null && !elementMoveCases.IsComplete()) elementMoveCases.Kill();
            elementMoveCases = Tween.BeginTweenCaseCollection();

            if (!instant)
            {
                Behavior.MoveTo(new Vector3[] { position }, false, () => { OnMoveEnded(true); });
            }
            else
            {
                Behavior.transform.position = position;

                Tween.NextFrame(() => OnMoveEnded(true));
            }

            Tween.EndTweenCaseCollection();
        }

        private void OnMoveEnded(bool rotate)
        {
            IsMoving = false;
            OnMovementEnded(this, MoveType);

            if (rotate)
            {
                TweenCaseRotateTowards rotateTowards = new TweenCaseRotateTowards(Behavior.transform, Quaternion.Euler(0, GameController.Data.ActivateVehicles ? 0 : 180, 0), 900, 0.15f);
                rotateTowards.SetDuration(float.MaxValue);
                rotateTowards.OnComplete(() =>
                {
                });
                rotateTowards.StartTween();
            }
        }

        public void ShiftMove(Vector3 position)
        {
            IsMoving = true;
            MoveType = MoveType.Shift;

            if (elementMoveCases != null && !elementMoveCases.IsComplete()) elementMoveCases.Kill();

            elementMoveCases = Tween.BeginTweenCaseCollection();

            Behavior.MoveTo(new Vector3[] { position }, true, () => { OnMoveEnded(false); });

            Tween.EndTweenCaseCollection();
        }

        public void Clear(bool disableBlock = true)
        {
            if (elementMoveCases != null && !elementMoveCases.IsComplete()) elementMoveCases.Kill();

            if(disableBlock)
                Behavior.gameObject.SetActive(false);
        }
    }
}