using UnityEngine;
using System.Collections;

namespace Template.UIBase
{

    public class UIRequestWaitingMain : UIElement
    {

        [SerializeField] private BroTweener tweener;
        [SerializeField] private ParticleSystem indicator;


        private void OnEnable()
        {
            tweener.SetProgress(0);
            indicator.Stop();

            StartCoroutine(Process());
        }


        private IEnumerator Process()
        {
            yield return new WaitForSeconds(1f);

            tweener.Play(false);
            indicator.Play();
        }
    }
}