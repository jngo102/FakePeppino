using System.Collections;
using UnityEngine;

namespace FakePeppino
{
    internal class Silhouette : MonoBehaviour
    {
        private const float FadeTime = 1;

        private IEnumerator Start()
        {
            iTween.ColorTo(gameObject, new Color(1, 0, 0, 0), FadeTime);
            yield return new WaitForSeconds(FadeTime);

            gameObject.Recycle();
        }
    }
}