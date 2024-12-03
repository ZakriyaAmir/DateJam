using UnityEngine;

namespace Watermelon.BusStop
{
    public class TweenCaseRotateTowards : TweenCase
    {
        private Transform tweenObject;

        private Quaternion targetRotation;

        private float minimumDistanceSqr;
        private float speed;

        public TweenCaseRotateTowards(Transform tweenObject, Quaternion targetRotation, float speed, float minimumDistance)
        {
            parentObject = tweenObject.gameObject;

            this.tweenObject = tweenObject;
            this.targetRotation = targetRotation;
            this.speed = speed;

            minimumDistanceSqr = Mathf.Pow(minimumDistance, 2);
        }

        public override bool Validate()
        {
            return parentObject != null;
        }

        public override void DefaultComplete()
        {
            tweenObject.rotation = targetRotation;
        }

        public override void Invoke(float deltaTime)
        {
            tweenObject.rotation = Quaternion.Lerp(tweenObject.rotation, targetRotation, speed * deltaTime);

            if (Quaternion.Angle(tweenObject.rotation, targetRotation) <= minimumDistanceSqr)
                Complete();
        }
    }
}