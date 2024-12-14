using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Watermelon.BusStop;
using static UnityEngine.GraphicsBuffer;

namespace Watermelon
{
    public class MaleBehavior : MonoBehaviour
    {
        [SerializeField] Transform enterPosition;

        [SerializeField] Animator animator;

        public List<Transform> seats = new List<Transform>();

        public LevelElement.Type Type { get; private set; }

        private IStateMachine stateMachine;
        private bool initEnable = true;

        public List<GameObject> passengers = new List<GameObject>();

        public int PassengersCount => passengers.Count;
        public bool HasAvailableSit => passengers.Count < 3;
        public bool IsAvailableToEnter { get; private set; }
        public GameObject countCanvas;
        public TMP_Text countText;

        /*private void Awake()
        {
            stateMachine = GetComponent<IStateMachine>();
        }

        private void OnEnable()
        {
            if (!initEnable)
                stateMachine.StartMachine();
            else
                initEnable = false;
        }

        private void OnDisable()
        {
            stateMachine.StopMachine();
        }

        public void SetType(LevelElement.Type type)
        {
            Type = type;
        }*/

        public void Spawn()
        {
            transform.position = LevelController.Environment.BusSpawnPos;

            passengers.Clear();
        }

        /*public void MoveToWaitingPos()
        {
            EnvironmentBehavior.AssignWaitingBus(this);
            Move(LevelController.Environment.BusWaitPos);
        }

        public void MoveToCollectingPos()
        {
            EnvironmentBehavior.AssignCollectingBus(this);

            if (EnvironmentBehavior.WaitingBus == this)
                EnvironmentBehavior.RemoveWaitingBus();

            var duration = 1;

            if (Vector3.Distance(transform.position, LevelController.Environment.BusSpawnPos) < 0.1f)
            {
                duration = 2;
            }
            Move(LevelController.Environment.BusCollectPos, duration, MakeAvailable);
        }*/

        public void Collect(GameObject passenger)
        {
            //Remove female from femaletilemanager by giving its row and column reference but first we have to assign each female an up to date row and column ID also remove return from this function
            var _femaleTileManager = FindObjectOfType<FemaleTileManager>();
            passengers.Add(passenger);
            var sitIndex = passengers.Count - 1;
            _femaleTileManager.RemoveCharacter(passenger.GetComponent<femaleInfo>().Row, passenger.GetComponent<femaleInfo>().Column);
            StartCoroutine(MoveToDestination(passenger.transform, enterPosition.transform, sitIndex));
            //update female count
            countText.text = $"{PassengersCount}/{seats.Count}";
            /*passenger.GetComponent<BaseCharacterBehavior>().MoveTo(new Vector3[] { enterPosition.transform.position }, false, () =>
            {
                var sit = seats[sitIndex];
                passenger.transform.SetParent(sit);
                passenger.transform.position = sit.position;
                //passenger.transform.rotation = Quaternion.Euler(0, 90, 0);
                //passenger.GetComponent<BaseCharacterBehavior>().PlaySpawnAnimation();
                //passenger.GetComponent<BaseCharacterBehavior>().OnElementSubmittedToBus();

                if (!HasAvailableSit)
                    IsAvailableToEnter = false;
            });*/
        }

        private IEnumerator MoveToDestination(Transform objectToMove, Transform targetPosition, int sitIndex)
        {
            Vector3 startPosition = objectToMove.position;
            float elapsedTime = 0f;
            float time = 0.3f;

            while (elapsedTime < time)
            {
                objectToMove.LookAt(targetPosition);
                objectToMove.position = Vector3.Lerp(startPosition, targetPosition.position, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            objectToMove.position = targetPosition.position; // Snap to the target position at the end
            var sit = seats[sitIndex];
            objectToMove.transform.SetParent(sit);
            objectToMove.transform.position = sit.position;
            objectToMove.transform.localRotation = Quaternion.Euler(0, 90, 0);
            //objectToMove.GetComponent<BaseCharacterBehavior>().PlaySpawnAnimation();
            //objectToMove.GetComponent<BaseCharacterBehavior>().OnElementSubmittedToBus();

            if (!HasAvailableSit)
                IsAvailableToEnter = false;
        }

        public void CollectInstant(BaseCharacterBehavior passenger)
        {
            passengers.Add(passenger.gameObject);
            var sitIndex = passengers.Count - 1;

            var sit = seats[sitIndex];
            passenger.transform.SetParent(sit);
            passenger.transform.position = sit.position;
            passenger.transform.rotation = Quaternion.Euler(0, 90, 0);
            passenger.PlaySpawnAnimation();
            passenger.OnElementSubmittedToBus();

            if (!HasAvailableSit)
                IsAvailableToEnter = false;
        }

        private void MakeAvailable()
        {
            IsAvailableToEnter = true;

            if (EnvironmentBehavior.WaitingBus == null)
                EnvironmentBehavior.SpawnNextBusFromQueue();
        }

        public void MoveToExit()
        {
            //Turn heads to exit position
            foreach (var passenger in passengers) 
            {
                passenger.transform.LookAt(-LevelController.Environment.BusExitPos);
            }
            //transform.LookAt(LevelController.Environment.BusExitPos);
            LevelController.OnMatchComplete();
            Instantiate(FindObjectOfType<GameController>().heartParticle,transform);

            //EnvironmentBehavior.RemoveCollectingBus();

            StartCoroutine(Move(LevelController.Environment.BusExitPos, 1, Clear));
        }

        private TweenCaseCollection moveCase;

        public IEnumerator Move(Vector3 position, float duration = 1, SimpleCallback onReached = null)
        {
            /* Vector3 startPosition = transform.position;
             Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1f);
             float elapsedTime = 0f;
             float time = 0.4f;

             while (elapsedTime < time)
             {
                 transform.LookAt(position);
                 transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / time);
                 elapsedTime += Time.deltaTime;
                 yield return null;
             }*/

            yield return new WaitForSeconds(0.15f);
            if (moveCase != null && !moveCase.IsComplete())
                moveCase.Kill();

            moveCase = Tween.BeginTweenCaseCollection();

            if (animator != null)
                animator.SetTrigger("Start");
            transform.DOMove(position, duration).OnComplete(onReached).SetEasing(Ease.Type.QuadOutIn);
            if (animator != null)
                Tween.DelayedCall(duration - 0.1f, () => animator.SetTrigger("Break"));

            Tween.EndTweenCaseCollection();
            yield return null;
        }

        public void Clear()
        {
            /*if (moveCase != null && !moveCase.IsComplete())
                moveCase.Kill();
            IsAvailableToEnter = false;

            for (int i = 0; i < passengers.Count; i++)
            {
                passengers[i].transform.SetParent(null);
                passengers[i].gameObject.SetActive(false);
            }
            passengers.Clear();

            stateMachine.StopMachine();*/
            
            gameObject.SetActive(false);
            if (FindFirstObjectByType<MaleBehavior>() == null) 
            {
                GameController.WinGame();
            }
            Destroy(gameObject);
        }
    }
}