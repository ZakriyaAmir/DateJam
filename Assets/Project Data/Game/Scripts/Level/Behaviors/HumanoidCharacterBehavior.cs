using System.Collections;
using TMPro;
using UnityEngine;

namespace Watermelon.BusStop
{
    [RequireComponent(typeof(BoxCollider))]
    public class HumanoidCharacterBehavior : BaseCharacterBehavior
    {
        private static readonly int PARTICLE_POOF = ParticlesController.GetHash("Poof");

        private static readonly int ANIMATOR_IDLE_ANIMATION = Animator.StringToHash("Idle");
        private static readonly int ANIMATOR_SITTING_ANIMATION = Animator.StringToHash("Sitting");

        private static readonly int ANIMATOR_MOVEMENT_BOOL = Animator.StringToHash("IsMoving");
        private static readonly int ANIMATOR_SLOTS_BOOL = Animator.StringToHash("Slots");

        [SerializeField] Animator graphicsAnimator;
        [SerializeField] ParticleSystem trailParticleSystem;

        [Header("Movement")]
        [SerializeField] float movementSpeed = 10;

        [Header("Outline")]
        [SerializeField] float outlineActiveWidth = 3.5f;
        [SerializeField] float outlineDisableWidth = 1.5f;

        private TweenCase rotateTweenCase;

        private Coroutine movementCoroutine;

        private MaterialPropertyBlock propertyBlock;

        public GameObject countCanvas;
        public TMP_Text countText;

        public string color;

        private void OnEnable()
        {
            if (countCanvas != null) { countCanvas.SetActive(false); }
        }

        public override void Initialise(LevelElement levelElement, ElementPosition elementPosition)
        {
            base.Initialise(levelElement, elementPosition);

            isMovementActive = false;
            isSubmitted = false;

            trailParticleSystem.Stop();

            graphicsAnimator.SetBool(ANIMATOR_MOVEMENT_BOOL, false);

            propertyBlock = new MaterialPropertyBlock();

            characterRenderer.SetOutlineWidth(propertyBlock, outlineDisableWidth);
        }

        public override void MoveTo(Vector3[] path, bool isSlots, SimpleCallback onCompleted)
        {
            graphicsAnimator.SetBool(ANIMATOR_SLOTS_BOOL, isSlots);
            graphicsAnimator.SetBool(ANIMATOR_MOVEMENT_BOOL, true);

            if (isSlots)
            {
                graphicsAnimator.Play("MovementSlot", -1, Random.Range(0.0f, 1.0f));
            }

            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MovementCoroutine(path, movementSpeed, !isSlots, () =>
            {
                graphicsAnimator.SetBool(ANIMATOR_MOVEMENT_BOOL, false);

                trailParticleSystem.Stop();

                onCompleted?.Invoke();
            }));
        }


        public override void OnElementClicked(bool isClickAllowed)
        {
            if (isClickAllowed)
            {
                LevelController.OnElementClicked(this, elementPosition);

                AudioController.PlaySound(AudioController.Sounds.clickSound);

                Vector3[] path = MapUtils.CalculatePath(elementPosition.X, elementPosition.Y, LevelController.LevelMap);

                // Rotate to first waypoint
                Vector3 rotationVector = path[0] - transform.position;
                rotationVector.z = 0;

                trailParticleSystem.Play();

                if (rotationVector != Vector3.zero)
                {
                    rotateTweenCase = new TweenCaseRotateTowards(transform, Quaternion.LookRotation(rotationVector.normalized, Vector3.back), 1200, 0.1f);
                    rotateTweenCase.SetDuration(float.MaxValue);
                    rotateTweenCase.OnComplete(() =>
                    {
                        MoveTo(path, false, () =>
                        {
                            LevelController.OnElementSubmittedToSlot(this, false);
                        });
                    });
                    rotateTweenCase.StartTween();
                }
                else
                {
                    MoveTo(path, false, () =>
                    {
                        LevelController.OnElementSubmittedToSlot(this, false);
                    });
                }

                LevelController.SubmitElement(this, elementPosition);
                countCanvas.SetActive(true);
            }
        }

        public override void OnElementSubmittedToBus()
        {
            ParticlesController.PlayParticle(PARTICLE_POOF).SetPosition(transform.position + new Vector3(0, 1, 0));

            graphicsAnimator.Play(ANIMATOR_SITTING_ANIMATION, -1, Random.Range(0.0f, 1.0f));
        }

        public override void Highlight(bool firstSpawn)
        {
            base.Highlight(firstSpawn);

            graphicsAnimator.Play(ANIMATOR_IDLE_ANIMATION, -1, Random.Range(0.0f, 1.0f));

            characterRenderer.SetOutlineWidth(propertyBlock, outlineActiveWidth);
        }

        public override void Unhighlight()
        {
            base.Unhighlight();

            if (isSubmitted) return;

            characterRenderer.SetOutlineWidth(propertyBlock, outlineDisableWidth);
        }

        public override void Unload()
        {
            base.Unload();

            rotateTweenCase.KillActive();
        }

        private void Reset()
        {
#if UNITY_EDITOR
            characterRenderer = transform.GetComponentInChildren<Renderer>();
            graphicsAnimator = transform.GetComponentInChildren<Animator>();
            trailParticleSystem = transform.GetComponentInChildren<ParticleSystem>();
            if (trailParticleSystem == null)
            {
                if (!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gameObject))
                {
                    GameObject dustParticle = RuntimeEditorUtils.GetAssetByName<GameObject>("Running Dust");
                    if (dustParticle != null)
                    {
                        GameObject particleInstance = GameObject.Instantiate(dustParticle, transform);
                        particleInstance.name = dustParticle.name;

                        trailParticleSystem = particleInstance.GetComponent<ParticleSystem>();
                    }
                }
            }

            movementSpeed = 10;

            BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }
#endif
        }
    }
}
