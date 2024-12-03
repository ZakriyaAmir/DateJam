using UnityEngine;

namespace Watermelon
{
    public static class AnimationTweenCases
    {
        #region Extensions
        public static TweenCase WaitForEnd(this Animation tweenObject, float delay = 0, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new Wait(tweenObject).SetDelay(delay).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
        }
        #endregion

        public class Wait : TweenCase
        {
            public Animation animation;

            public Wait(Animation animation)
            {
                this.animation = animation;

                duration = float.MaxValue;
            }

            public override void DefaultComplete()
            {

            }

            public override void Invoke(float deltaTime)
            {
                if (!animation.isPlaying)
                    Complete();
            }

            public override bool Validate()
            {
                return true;
            }
        }
    }
}
