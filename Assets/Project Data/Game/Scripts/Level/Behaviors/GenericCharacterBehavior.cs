using System.Collections;
using UnityEngine;
using Watermelon.BusStop;

namespace Watermelon
{
    [RequireComponent(typeof(BoxCollider))]
    public class GenericCharacterBehavior : BaseCharacterBehavior
    {
        public enum MoveType { Direct, Path }

        [Header("Movement")]
        [SerializeField] MoveType moveType = MoveType.Path;

        [ShowIf("IsDirectMoveType")]
        [SerializeField] float moveDuration = 0.2f;
        [ShowIf("IsDirectMoveType")]
        [SerializeField] Ease.Type moveEasing = Ease.Type.Linear;

        [ShowIf("IsDirectMoveType")]
        [SerializeField] float moveSlotDuration = 0.2f;
        [ShowIf("IsDirectMoveType")]
        [SerializeField] Ease.Type moveSlotEasing = Ease.Type.Linear;

        [HideIf("IsDirectMoveType")]
        [SerializeField] float moveSpeed = 10;

        [Header("Highlight")]
        [SerializeField] Vector3 disableScale = new Vector3(0.7f, 0.7f, 0.7f);
        [SerializeField] Vector3 activeScale = Vector3.one;

        [SerializeField] bool highlightAnimation = true;
        [ShowIf("highlightAnimation")]
        [SerializeField] float highlightActiveTime = 0.2f;
        [ShowIf("highlightAnimation")]
        [SerializeField] Ease.Type highlightActiveEasing = Ease.Type.BackOut;

        [ShowIf("highlightAnimation")]
        [SerializeField] float highlightDisableTime = 0.2f;
        [ShowIf("highlightAnimation")]
        [SerializeField] Ease.Type highlightDisableEasing = Ease.Type.BackIn;

        private TweenCase moveTweenCase;
        private TweenCase scaleTweenCase;

        private Coroutine movementCoroutine;

        private void Reset()
        {
            characterRenderer = transform.GetComponentInChildren<Renderer>();

            BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }
        }

        public override void Initialise(LevelElement levelElement, ElementPosition elementPosition)
        {
            base.Initialise(levelElement, elementPosition);

            isMovementActive = false;
            isSubmitted = false;

            if (highlightAnimation)
            {
                transform.localScale = disableScale;
            }
        }

        public override void MoveTo(Vector3[] path, bool isSlots, SimpleCallback onCompleted)
        {
            if(moveType == MoveType.Path)
            {
                if (movementCoroutine != null) StopCoroutine(movementCoroutine);
                movementCoroutine = StartCoroutine(MovementCoroutine(path, moveSpeed, !isSlots, onCompleted));
            }
            else if(moveType == MoveType.Direct)
            {
                Vector3 position = path[path.Length - 1];

                if (!isSlots)
                {
                    moveTweenCase = transform.DOMove(position, moveDuration).SetEasing(moveEasing).OnComplete(() =>
                    {
                        onCompleted?.Invoke();
                    });
                }
                else
                {
                    moveTweenCase = transform.DOMove(position, moveSlotDuration).SetEasing(moveSlotEasing).OnComplete(() =>
                    {
                        onCompleted?.Invoke();
                    });
                }
            }
        }

        public override void OnElementClicked(bool isClickAllowed)
        {
            if (isClickAllowed)
            {
                LevelController.OnElementClicked(this, elementPosition);

                AudioController.PlaySound(AudioController.Sounds.clickSound);

                Vector3[] path = MapUtils.CalculatePath(elementPosition.X, elementPosition.Y, LevelController.LevelMap);
                MoveTo(path, false, () =>
                {
                    LevelController.OnElementSubmittedToSlot(this, false);
                });

                LevelController.SubmitElement(this, elementPosition);
            }
        }

        public override void OnElementSubmittedToBus()
        {

        }

        public override void Highlight(bool firstSpawn)
        {
            base.Highlight(firstSpawn);

            if(highlightAnimation)
            {
                if (firstSpawn)
                {
                    transform.localScale = activeScale;
                }
                else
                {
                    scaleTweenCase.KillActive();
                    scaleTweenCase = transform.DOScale(activeScale, highlightActiveTime).SetEasing(highlightActiveEasing);
                }
            }
        }

        public override void Unhighlight()
        {
            base.Unhighlight();

            if(highlightAnimation)
            {
                if (!isSubmitted)
                {
                    scaleTweenCase.KillActive();
                    scaleTweenCase = transform.DOScale(disableScale, highlightDisableTime).SetEasing(highlightDisableEasing);
                }
            }
        }

        public override void Unload()
        {
            base.Unload();

            scaleTweenCase.KillActive();
            moveTweenCase.KillActive();
        }

        private bool IsDirectMoveType()
        {
            return moveType == MoveType.Direct;
        }
    }
}
