using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class DockBehavior : MonoBehaviour
    {
        private static DockBehavior instance;

        [SerializeField] AnimationCurve positionYCurve;

        [SerializeField] public static List<SlotBehavior> slots;
        [SerializeField] public List<SlotBehavior> slots2 => slots;

        private LevelController levelController;
        private Vector3 defaultContainerPosition;

        private BaseCharacterBehavior lastPickedObject;

        //[SerializeField] public bool IsFilled => slots[^1].IsOccupied;
        [SerializeField] public bool IsFilled;
        //[SerializeField] public bool IsAlmostFilled => slots[5].IsOccupied;
        [SerializeField] public bool IsEmpty => !slots[0].IsOccupied;
         public bool finalSubmitted;

        private TweenCase delayTweenCase;

        public static BaseCharacterBehavior LastPickedObject => instance.lastPickedObject;
        public static AnimationCurve PositionYCurve => instance.positionYCurve;

        public GameController _gameController;

        public int matchFailCount;
        public bool gameOver;

        public void Initialise(LevelController levelController)
        {
            instance = this;
            matchFailCount = 0;
            this.levelController = levelController;

            defaultContainerPosition = transform.position;

            lastPickedObject = null;

            slots = new List<SlotBehavior>();
            transform.GetComponentsInChildren(slots);

            slots.Sort((slot1, slot2) => (int)((slot2.Position.x - slot1.Position.x) * 100));

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].Init();
            }
            _gameController = FindObjectOfType<GameController>();
        }

        public void DisposeQuickly()
        {
            gameOver = false;
            matchFailCount = 0;

            delayTweenCase.KillActive();

            lastPickedObject = null;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                slot.Clear();
            }

            for (int i = 0; i < addedDepth * 3; i++)
            {
                var slot = slots[slots.Count - 1];
                slots.RemoveAt(slots.Count - 1);

                slot.Clear();
                Destroy(slot.gameObject);
            }

            addedDepth = 0;
        }

        public void DisableRevert()
        {
            lastPickedObject = null;
        }


        public void MyDebug()
        {
            Debug.Log($"Slots Count: {slots.Count}");

            for (int i = 0; i < slots.Count; i++)
            {
                Debug.Log($"Slot {i}: {slots[i].IsOccupied.ToString()}");
            }
        }

        private int addedDepth = 0;

        private void RemoveMatch(List<BaseCharacterBehavior> charactersToRemove)
        {
            finalSubmitted = false;

            lastPickedObject = null;

            var slotsToRemove = new List<SlotCase>();
            for (int i = 0; i < slots.Count; i++)
            {
                var slotCase = slots[i].SlotCase;

                if (slotCase != null && charactersToRemove.Contains(slotCase.Behavior))
                {
                    slotsToRemove.Add(slotCase);
                    slotCase.IsBeingRemoved = true;
                }
            }

            Tween.DoFloat(1f, 1.3f, 0.3f, (value) => {
                for (int i = 0; i < slotsToRemove.Count; i++)
                {
                    var slotCase = slotsToRemove[i];
                    slotCase.Behavior.transform.localScale = Vector3.one * value;
                }
            }).SetEasing(Ease.Type.SineIn).OnComplete(() => {
                for (int i = 0; i < slotsToRemove.Count; i++)
                {
                    var slotCase = slotsToRemove[i];
                    var element = slotCase.Behavior;

                    slotCase.IsBeingRemoved = false;

                    ParticlesController.PlayParticle("Slot Highlight").SetDuration(1).SetPosition(element.transform.position + Vector3.back * 0.2f);

                    element.transform.localScale = Vector3.one;
                    element.gameObject.SetActive(false);

                    for (int j = 0; j < slots.Count; j++)
                    {
                        var slot = slots[j];

                        if (slot.SlotCase == slotCase)
                        {
                            slot.RemoveSlot();
                            break;
                        }
                    }
                }

                ShiftAllLeft();

                if (addedDepth > 0)
                {
                    addedDepth--;
                    for (int i = 0; i < 3; i++)
                    {
                        var tempSlot = slots[^1];
                        slots.RemoveAt(slots.Count - 1);

                        tempSlot.Clear();
                        Destroy(tempSlot.gameObject);
                    }
                }
                levelController.OnMatchCompleted();
            });
        }

        public int AmountInSlots(LevelElement matchable)
        {
            return slots.FindAll((slot) => { return slot.IsOccupied && slot.SlotCase.Behavior.LevelElement == matchable; }).Count;
        }

        private bool CheckMatch(bool remove = true)
        {
            if (IsEmpty) return false;
            if (!GameController.Data.ActivateVehicles)
            {
                return CheckDockMatch(remove);
            }
            else
            {
                CheckBusMatch();

                return false;
            }
        }

        public static List<BaseCharacterBehavior> GetHintCharacters()
        {
            SlotBehavior[] elementsArray = slots.FindAll(x => x.IsOccupied).GroupBy(x => x.SlotCase.Behavior.LevelElement.ElementType).OrderByDescending(g => g.Count()).SelectMany(g => g).ToArray();

            if (!elementsArray.IsNullOrEmpty())
            {
                List<BaseCharacterBehavior> baseCharacterBehaviors = new List<BaseCharacterBehavior>();
                baseCharacterBehaviors.Add(elementsArray[0].SlotCase.Behavior);

                LevelElement.Type elementType = elementsArray[0].SlotCase.Behavior.LevelElement.ElementType;

                for (int i = 1; i < elementsArray.Length; i++)
                {
                    if (elementsArray[i].SlotCase.Behavior.LevelElement.ElementType == elementType)
                    {
                        baseCharacterBehaviors.Add(elementsArray[i].SlotCase.Behavior);
                    }
                }

                return baseCharacterBehaviors;
            }

            return null;
        }

        private bool CheckDockMatch(bool remove = true)
        {
            int counter = 1;
            var matchable = slots[0].SlotCase.Behavior.LevelElement;
            var list = new List<BaseCharacterBehavior> { slots[0].SlotCase.Behavior };

            for (int i = 1; i < slots.Count; i++)
            {
                var slot = slots[i];

                if (!slot.IsOccupied) return false;

                var slotCase = slot.SlotCase;
                var element = slotCase.Behavior;

                if (counter == 0)
                {
                    matchable = element.LevelElement;
                }

                if (element.LevelElement == matchable && !slotCase.IsBeingRemoved && (!slot.SlotCase.IsMoving || !remove))
                {
                    counter++;
                    list.Add(element);

                    if (counter == 3)
                    {
                        if (remove) RemoveMatch(list);

                        return true;
                    }
                }
                else if (!slotCase.IsBeingRemoved)
                {
                    counter = 1;
                    matchable = element.LevelElement;
                    list = new List<BaseCharacterBehavior> { element };
                }
                else
                {
                    counter = 0;
                    list = new List<BaseCharacterBehavior>();
                }
            }

            return false;
        }

        private void CheckBusMatch()
        {
            //var bus = EnvironmentBehavior.CollectingBus;
            //if (bus == null) return;
            //if (!bus.IsAvailableToEnter || !bus.HasAvailableSit) return;
            
            //for is filled bool
            bool filledAlt = true;
            foreach (var slot in slots)
            {
                if (!slot.IsOccupied)
                {
                    filledAlt = false;
                    break;
                }
            }
            IsFilled = filledAlt;
            //

            bool removed = false;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                //Debug.Log($"slot {slot} | {slot.IsOccupied}");
                if (!slot.IsOccupied)
                {
                    matchFailCount = 0;
                }

                if (slot.SlotCase == null) continue;
                if (_gameController._femaleTileManager.isBusy) continue;
                if (slot.SlotCase.Behavior == null) continue;
                if (slot.SlotCase.Behavior.gameObject == null) continue;
                var male = slot.SlotCase.Behavior.gameObject.GetComponent<MaleBehavior>();
                if (slot.IsOccupied)
                {
                    if (IsFilled)
                    {
                        matchFailCount++;
                        if (matchFailCount > 2000)
                        {
                            gameOver = true;
                            levelController.OnSlotsFilled();
                        }
                    }
                    else 
                    {
                        matchFailCount = 0;
                    }

                    for (int index = 0; index < _gameController._femaleTileManager.lowermostCharacters.Length; index++)
                    {
                        GameObject obj = _gameController._femaleTileManager.lowermostCharacters[index];

                        // Skip if the object is null
                        if (obj == null) continue;

                        // Match color
                        if (slot.SlotCase.Behavior.LevelElement.ElementType.ToString() == obj.GetComponent<HumanoidCharacterBehavior>().color)
                        {
                            //Reset count on match
                            matchFailCount = 0;
                            // Remove the GameObject by setting it to null
                            _gameController._femaleTileManager.lowermostCharacters[index] = null;

                            // bus.Collect(slot.SlotCase.Behavior);
                            male.Collect(obj);

                            if (male.PassengersCount > 2)
                            {
                                slot.Invoke(nameof(slot.RemoveSlot),0.3f);
                                removed = true;
                                male.MoveToExit();
                            }

                            // Stop processing if conditions are met
                            if (!male.IsAvailableToEnter || !male.HasAvailableSit) break;
                        }
                    }
                }
            }

            if (removed)
            {
                lastPickedObject = null;
                ShiftAllLeft();
            }
        }

        private int CalculateIndexSlots(LevelElementBehavior matchableObject)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.IsOccupied) return i;

                var block = slot.SlotCase.Behavior.LevelElement;
                if (matchableObject.LevelElement == block)
                {
                    /*for (int j = i + 1; j < slots.Count; j++)
                    {
                        var nextSlot = slots[j];

                        if (!nextSlot.IsOccupied) return j;

                        var nextBlock = nextSlot.SlotCase.Behavior.LevelElement;
                        if (!slot.IsOccupied || matchableObject.LevelElement != nextBlock)
                        {
                            return j;
                        }
                    }*/
                }
            }

            return -1;
        }

        public static int GetSlotsAvailable()
        {
            int counter = 0;
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (!slots[i].IsOccupied) counter++;
            }

            return counter;
        }

        public void SetOverlayPosition()
        {
            transform.position = defaultContainerPosition.SetZ(1.0f);
        }

        public void SetDefaultPosition()
        {
            transform.position = defaultContainerPosition;
        }

        public bool SubmitToSlot(BaseCharacterBehavior element, bool instant)
        {
            int index = CalculateIndexSlots(element);

            SlotCase slotCase = new SlotCase(element);

            slotCase.IsMoving = true;
            slotCase.MoveType = MoveType.Submit;

            slots[index].Assign(slotCase, instant);
            /*if (slots[index].IsOccupied)
            {
                Debug.Log("zak1");
                Insert(slotCase, index, instant);
            }
            else
            {
                Debug.Log("zak2");
                slots[index].Assign(slotCase, instant);
            }*/

            lastPickedObject = element;

            if (CheckMatch(false))
            {
                if (addedDepth < 2)
                {
                    addedDepth++;
                    for (int i = 0; i < 3; i++)
                    {
                        slots.Add(SlotBehavior.GetTempSlot(slots[^1], slots[^2]));
                    }
                }

            }
            /*else if (IsFilled && matchFailCount > 8)
            {
                levelController.OnSlotsFilled();
            }*/

            return true;
        }

        public static BaseCharacterBehavior RemoveLastPicked()
        {
            if (instance.lastPickedObject == null) return null;

            var objToReturn = instance.lastPickedObject;
            instance.lastPickedObject = null;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                if (slot.IsOccupied && slot.SlotCase.Behavior == objToReturn)
                {
                    slot.SlotCase.Clear(false);
                    slot.RemoveSlot();

                    instance.ShiftAllLeft();

                    break;
                }
            }

            return instance.lastPickedObject;
        }

        public void Insert(SlotCase slotCase, int index, bool instant = false)
        {
            var freeCase = slotCase;

            for (int i = index; i < slots.Count; i++)
            {
                var slot = slots[i];

                var caseToShift = slot.RemoveSlot();

                if (freeCase != null)
                {
                    if (freeCase.IsMoving && freeCase.MoveType == MoveType.Submit)
                    {
                        slot.Assign(freeCase, instant);
                    }
                    else
                    {
                        slot.AssingFast(freeCase);
                    }
                }

                freeCase = caseToShift;
            }
        }
        public void ShiftAllLeft()
        {
            finalSubmitted = false;
            var lastIndex = -1;

            for (int i = 0; i < slots.Count - 1; i++)
            {
                var recepient = slots[i];

                if (recepient.IsOccupied) 
                {
                    //Debug.Log($"zak = {slots[i]} = {slots[i].IsOccupied}");
                }
                /*var recepient = slots[i];

                if (recepient.IsOccupied) continue;

                bool found = false;
                for (int j = i + 1; j < slots.Count; j++)
                {
                    var donor = slots[j];

                    if (!donor.IsOccupied) continue;

                    var slotCase = donor.RemoveSlot();
                    if (slotCase.IsMoving && slotCase.MoveType == MoveType.Submit)
                    {
                        recepient.Assign(slotCase);
                    }
                    else
                    {
                        recepient.AssingFast(slotCase);
                    }

                    found = true;

                    break;
                }

                lastIndex = i;
                if (!found) break;*/
            }

            if (lastIndex == -1) return;

            for (int i = lastIndex; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.RestoreColor(Color.white);
            }
        }

        public void LateUpdate()
        {
            /*if (Time.frameCount % 15 == 0 && !IsEmpty)
            {*/
            /*if (!GameController.Data.ActivateVehicles)
            {
                if (!CheckMatch() && IsFilled)
                {
                    levelController.OnSlotsFilled();
                }
            }
            else
            {*/
            if (!gameOver)
            {
                CheckBusMatch();
            }
                    /*if (IsFilled && !EnvironmentBehavior.IsCollectingPlaceAvailable)
                    {
                        levelController.OnSlotsFilled();
                    }*/
                //}

            //}
        }

        public static void OnMovementEnded(SlotCase slotCase, MoveType moveType)
        {
            instance.OnMoveEnded(slotCase, moveType);
        }

        public void OnMoveEnded(SlotCase slotCase, MoveType moveType)
        {

            CheckMatch();

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                if (slot.IsOccupied && (slot.SlotCase.IsMoving || slot.SlotCase.IsBeingRemoved)) return;
            }

            ShiftAllLeft();

            for (int i = 0; i < addedDepth * 3; i++)
            {
                var slot = slots[slots.Count - 1];
                slots.RemoveAt(slots.Count - 1);

                slot.Clear();
                Destroy(slot.gameObject);
            }

            addedDepth = 0;
        }

        public enum MoveType
        {
            Submit,
            Shift,
        }
    }
}
