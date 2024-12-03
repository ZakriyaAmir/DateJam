using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.BusStop;

namespace Watermelon
{
    public class BusStateMachine: AbstractStateMachine<BusState>
    {
        private MaleBehavior bus;

        private void Awake()
        {
            bus = GetComponent<MaleBehavior>();

            var spawningCase = new StateCase();
            spawningCase.state = new BusSpawningState(bus);
            spawningCase.transitions = new List<StateTransition<BusState>> { new StateTransition<BusState>(SpawnTransition) };

            var waitingCase = new StateCase();
            waitingCase.state = new BusWaitingState(bus);
            waitingCase.transitions = new List<StateTransition<BusState>> { new StateTransition<BusState>(WaitTransition, StateTransitionType.OnFinish) };

            var collectingCase = new StateCase();
            collectingCase.state = new BusCollectingState(bus);
            collectingCase.transitions = new List<StateTransition<BusState>> { new StateTransition<BusState>(CollectTransition, StateTransitionType.OnFinish) };

            var exitingCase = new StateCase();
            exitingCase.state = new BusExitingState(bus);
            exitingCase.transitions = new List<StateTransition<BusState>>();

            states.Add(BusState.Spawning, spawningCase);
            states.Add(BusState.Waiting, waitingCase);
            states.Add(BusState.Collecting, collectingCase);
            states.Add(BusState.Exiting, exitingCase);
        }

        private bool SpawnTransition(out BusState state)
        {
            if (EnvironmentBehavior.IsCollectingPlaceAvailable)
            {
                state = BusState.Collecting;
                return true;
            }
            if (EnvironmentBehavior.IsWaitingPlaceAvailable)
            {
                state = BusState.Waiting;
                return true;
            }

            state = BusState.Spawning;
            return false;
        }

        private bool WaitTransition(out BusState state)
        {
            state = BusState.Collecting;

            return true;
        }

        private bool CollectTransition(out BusState state)
        {
            state = BusState.Exiting;

            return true;
        }
    }

    public class BusSpawningState: StateBehavior<MaleBehavior>
    {
        public BusSpawningState(MaleBehavior target) : base(target) { }

        public override void OnStart()
        {
            Target.Spawn();
        }
    }

    public class BusWaitingState : StateBehavior<MaleBehavior>
    {
        public BusWaitingState(MaleBehavior target) : base(target) { }

        public override void OnStart()
        {
            //Target.MoveToWaitingPos();
        }

        public override void OnUpdate()
        {
            if (EnvironmentBehavior.IsCollectingPlaceAvailable && Vector3.Distance(Target.transform.position, LevelController.Environment.BusWaitPos) < 0.1f)
            {
                InvokeOnFinished();
            }
        }
    }

    public class BusCollectingState : StateBehavior<MaleBehavior>
    {
        public BusCollectingState(MaleBehavior target) : base(target) { }

        public override void OnStart()
        {
            //Target.MoveToCollectingPos();
        }

        public override void OnUpdate()
        {
            if (!Target.HasAvailableSit && !Target.IsAvailableToEnter)
            {
                InvokeOnFinished();
            }
        }
    }

    public class BusExitingState : StateBehavior<MaleBehavior>
    {
        public BusExitingState(MaleBehavior target) : base(target) { }

        public override void OnStart()
        {
            Target.MoveToExit();
        }
    }

    public enum BusState
    {
        Spawning,
        Waiting, 
        Collecting,
        Exiting
    }
}