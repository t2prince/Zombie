using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Util
{
	public class Effect : MonoBehaviour {

		public static UnityEngine.Coroutine Flicking(GameObject gameObject, float time, float interval, bool isFinalVisible = true)
		{
			return Coroutine.Run(Flick(gameObject, time, interval, isFinalVisible));
		}

		private static IEnumerator Flick(GameObject gameObject, float time, float interval, bool isFinalVisible)
		{
			var viewOn = false;
			var startTime = Time.time;
			
			while (gameObject && Time.time - startTime < time) {
				gameObject.SetActive(viewOn);
				viewOn = !viewOn;

				yield return new WaitForSeconds(interval);
			}
			
			gameObject.SetActive(isFinalVisible);
		}

		public static void CreateEffectByPrefab(GameObject prefab, Vector3 setPosition) {
			var effectInstance = Instantiate(prefab);
			effectInstance.transform.localPosition = setPosition;
			effectInstance.transform.localScale = Vector3.one;

			var animator = effectInstance.GetComponent<Animator>();
			var currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(0);
			
			Coroutine.DelayedAction(() => {
				Destroy(effectInstance.gameObject);
			}, currentAnimatorClipInfo[0].clip.length);
		}
	}
}

