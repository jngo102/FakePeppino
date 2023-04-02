using UnityEngine;

namespace FakePeppino
{
    internal static class Extensions
    {
        public static AnimationClip GetClipByName(this Animator animator, string clipName)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName)
                {
                    return clip;
                }
            }

            return null;
        }
    }
}
