namespace Watermelon
{
    public static class TweenExtensions
    {
        public static bool KillActive(this TweenCase tweenCase)
        {
            if (tweenCase != null && tweenCase.IsActive)
            {
                tweenCase.Kill();

                return true;
            }

            return false;
        }

        public static void KillActive(this TweenCase[] tweenCases)
        {
            if(tweenCases != null)
            {
                foreach (TweenCase tweenCase in tweenCases)
                {
                    if (tweenCase != null && tweenCase.IsActive)
                    {
                        tweenCase.Kill();
                    }
                }
            }
        }

        public static bool CompleteActive(this TweenCase tweenCase)
        {
            if (tweenCase != null && !tweenCase.IsCompleted)
            {
                tweenCase.Complete();

                return true;
            }

            return false;
        }

        public static void CompleteActive(this TweenCase[] tweenCases)
        {
            if (tweenCases != null)
            {
                foreach (TweenCase tweenCase in tweenCases)
                {
                    if (tweenCase != null && tweenCase.IsActive)
                    {
                        tweenCase.Complete();
                    }
                }
            }
        }
    }
}