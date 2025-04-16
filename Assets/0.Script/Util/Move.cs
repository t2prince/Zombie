using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Jjamcat.Util;
using Random = UnityEngine.Random;

namespace Util
{
	public struct MoveInfo
	{
		public float moveTime;
		public Vector3 to;
		public GameObject movingObject;
		public bool isLocalPos;
		public System.Action callback;

		public MoveInfo (GameObject obj, Vector3 toPos, float time, bool isLocalPos = true, System.Action callback = null)
		{
			moveTime = time;
			to = toPos;
			movingObject = obj;
			this.isLocalPos = isLocalPos;
			this.callback = callback;
		}		
	}

	public struct MoveInfoForTransform
	{
		public float moveTime;
		public GameObject movingObject;
		public bool isLocalPos;
		public System.Action callback;
		public Transform to;

		public MoveInfoForTransform (GameObject obj, Transform toTransform, float time, bool isLocalPos = true, System.Action callback = null)
		{
			this.moveTime = time;
			this.to = toTransform;
			this.movingObject = obj;
			this.isLocalPos = isLocalPos;
			this.callback = callback;
		}
		
	}
	
	public struct ScaleInfo
	{
		public float scaleTime;
		public GameObject scaleObject;
		public Vector3 originScale;
		public Vector3 destScale;
		public System.Action callback;		

		public ScaleInfo (GameObject obj, Vector3 toScale, Vector3 originScale, float time, System.Action callback = null)
		{
			this.scaleTime = time;
			this.originScale = originScale;
			this.destScale = toScale;
			this.scaleObject = obj;
			this.callback = callback;
		}
		
	}
	
	public struct RotateInfo
	{
		public float rotateTime;
		public float delayTime;
		public GameObject rotateObject;
		public Vector3 originAngle;
		public Vector3 destAngle;
		public bool isLocal;
		public System.Action callback;
		
		public RotateInfo (GameObject obj, Vector3 toAngle, Vector3 originAngle, float time, float delayTime = 0f,  bool isLocal = true, System.Action callback = null)
		{
			this.delayTime = delayTime;
			this.rotateTime = time;
			this.originAngle = originAngle;
			this.destAngle = toAngle;
			this.rotateObject = obj;
			this.isLocal = isLocal;
			this.callback = callback;
		}
	}

	public class Move : SingletonGameObject<Move>
	{
		public static void MoveLinear(MoveInfo info)
		{
			Coroutine.Run(DoMoveLinear(info));
		}

		public static void MoveLinear(List<MoveInfo> infoList)
		{
			Coroutine.Run(DoMoveLinear(infoList));
		}
		
		public static void MoveEaseInOut(MoveInfo info)
		{
			Coroutine.Run(DoMoveEaseInOut(info));
		}

		public static void MoveCurve(MoveInfo info)
		{
			
		}
		
		public static IEnumerator DoMoveLinear (MoveInfo info)
		{
			if (info.movingObject == null)
				yield break;
			
			var startTime = Time.time;
			var target = info.movingObject;
			var from = (info.isLocalPos) ? target.transform.localPosition : target.transform.position;
			var to = info.to;
			var direction = (to - from).normalized;
			
			while (target && Time.time - startTime < info.moveTime) {
				var rate = (Time.time - startTime) / info.moveTime;
				
				if (info.isLocalPos)
					target.transform.localPosition = Vector3.Lerp(from, to, rate);
				else
					target.transform.position = Vector3.Lerp(from, to, rate);
				
				yield return null;
			}

			if (info.isLocalPos)
				target.transform.localPosition = to;
			else
				target.transform.position = to;
			
			info.callback?.Invoke();
		}

		public static IEnumerator DoMoveLinearForward(MoveInfo info)
		{
			if (info.movingObject == null)
				yield break;
			
			var startTime = Time.time;
			var target = info.movingObject;
			var from = (info.isLocalPos) ? target.transform.localPosition : target.transform.position;
			var to = info.to;
			var direction = (to - from).normalized;
			
			while (target && Time.time - startTime < info.moveTime) {
				var rate = (Time.time - startTime) / info.moveTime;
				var rotation = Quaternion.Slerp(target.transform.rotation, Quaternion.LookRotation(direction), rate);
				if (info.isLocalPos)
					target.transform.localPosition = Vector3.Lerp(from, to, rate);
				else
					target.transform.position = Vector3.Lerp(from, to, rate);
				target.transform.rotation = rotation;
				
				yield return null;
			}

			if (info.isLocalPos)
				target.transform.localPosition = to;
			else
				target.transform.position = to;
			
			info.callback?.Invoke();
		}
		
		private static IEnumerator DoMoveLinear (List<MoveInfo> infoList)
		{
			foreach (var info in infoList)
			{
				yield return Coroutine.Run(DoMoveLinear(info));
			}					
		}
		
		private static IEnumerator DoMoveEaseInOut (MoveInfo info)
		{
			float startTime = Time.time;
			var target = info.movingObject;
			var from = (info.isLocalPos) ? target.transform.localPosition : target.transform.position;
			var to = info.to;
			var curve = AnimationCurve.EaseInOut(0, 0, info.moveTime, 1);
			
			while (target && Time.time - startTime < info.moveTime) {
				var targetPosition = Vector3.Lerp(from, to, curve.Evaluate(Time.time - startTime));

				if (info.isLocalPos)
					target.transform.localPosition = targetPosition;
				else
					target.transform.position = targetPosition;
				
				yield return null;
			}

			if (info.isLocalPos)
				target.transform.localPosition = to;
			else
				target.transform.position = to;
			
			info.callback?.Invoke();
		}

		private static IEnumerator DoMoveCurve(MoveInfo info, float curveRate)
		{
			//iTween.tweens
			yield return null;
		}

		public static IEnumerator DoMoveLinearToTransform (MoveInfoForTransform info)
		{
			var startTime = Time.time;
			var target = info.movingObject;
			var from = (info.isLocalPos) ? target.transform.localPosition : target.transform.position;
			var to = info.to.position;
			
			do
			{
				yield return null;
				var rate = (Time.time - startTime) / info.moveTime;
				var targetPosition = Vector3.Lerp(from, to, rate);
				if (info.isLocalPos)
					target.transform.localPosition = targetPosition;
				else
					target.transform.position = targetPosition;
			} while (target && Time.time - startTime < info.moveTime);

			yield return null;
			
			if (info.isLocalPos)
				target.transform.localPosition = to;
			else
				target.transform.position = to;
			
			info.callback?.Invoke();
		}
		
		public static IEnumerator MoveLinearSquare(MoveInfo info)
		{
			var startTime = Time.time;
			var target = info.movingObject;
			var from = target.transform.localPosition;
			var to = info.to;

			while (target && Time.time - startTime < info.moveTime) {
				float rate = (Time.time - startTime) / info.moveTime;
				target.transform.localPosition = Vector3.Lerp (from, to, rate * rate );
				yield return null;
			}

			target.transform.localPosition = to;
			info.callback?.Invoke();
		}
		
		public static IEnumerator MoveLinearInverseSquare(MoveInfo info)
		{
			var startTime = Time.time;
			var target = info.movingObject;
			var from = target.transform.localPosition;
			var to = info.to;

			while (target && Time.time - startTime < info.moveTime) {
				var rate = (Time.time - startTime) / info.moveTime;
				target.transform.localPosition = Vector3.Lerp (from, to, (float) Math.Log10(rate * 10.0));
				yield return null;
			}

			target.transform.localPosition = to;
			info.callback?.Invoke();
		}

		public static void Switch(GameObject first, GameObject second, float time)
		{
			var firstToSeconde = new MoveInfo(first, second.transform.position,time, false);
			var secondToFirst = new MoveInfo(second, first.transform.position,time, false);

			MoveLinear(firstToSeconde);
			MoveLinear(secondToFirst);
		}
		

		public static void Shake(float time, float radius, GameObject obj)
		{
			var rigidbody2D = obj.GetComponentInChildren<Rigidbody2D> ();

			Coroutine.Run(rigidbody2D != null
				? ShakeRigidBody(time, radius, rigidbody2D)
				: ShakeTransform(time, radius, obj.transform));
		}

		public static IEnumerator ShakeTransform (float time, float radius, Transform obj)
		{
			//60 fps 1 sec 
			Vector3 shakePos = new Vector3();
			Vector3 prevMovedPos = Vector3.zero;
			int maxFrame = Mathf.RoundToInt(time * 60f);

			for (int i = 0; i < maxFrame; i++) 
			{
				float randomAngle = Random.Range (0, Mathf.PI * 2f);
				float randomRadius = Random.Range (0, radius);
				shakePos.x = randomRadius * Mathf.Cos (randomAngle);
				shakePos.y = randomRadius * Mathf.Sin (randomAngle);
				obj.position -= prevMovedPos;
				obj.position += shakePos;
				prevMovedPos = shakePos;
				yield return null;
			}
			obj.position -= prevMovedPos;
			yield return null;
		}
		public static IEnumerator ShakeRigidBody (float time, float radius, Rigidbody2D obj)
		{
			Vector2 shakePos = new Vector2();
			Vector2 prevMovedPos = Vector2.zero;
			int maxFrame = Mathf.RoundToInt(time * 60f);

			for (int i = 0; i < maxFrame; i++) 
			{
				float randomAngle = Random.Range (0, Mathf.PI * 2f);
				float randomRadius = Random.Range (0, radius);
				shakePos.x = randomRadius * Mathf.Cos (randomAngle);
				shakePos.y = randomRadius * Mathf.Sin (randomAngle);
				obj.position -= prevMovedPos;
				obj.position += shakePos;
				prevMovedPos = shakePos;
				yield return null;
			}
			obj.position -= prevMovedPos;
			yield return null;
		}
		

		public static IEnumerator LookAt(GameObject target, Vector3 lookAtPos, float time)
		{
			float startTime = Time.time;
			var originRotation = target.transform.rotation;

			while (target && Time.time - startTime < time) {
				//float rate = (Time.time - startTime) / time;
				var direction = (lookAtPos - target.transform.position).normalized;

				var angle =  Quaternion.FromToRotation (Vector3.up, direction).eulerAngles;
				target.transform.rotation = Quaternion.Euler(new Vector3(0f, angle.y, angle.z));
				yield return null;
			}

			if(target != null)
				target.transform.rotation = originRotation;
		}

		public static IEnumerator ScalePunch(ScaleInfo info)
		{
			var startTime = Time.time;
			var target = info.scaleObject;

			var from = target.transform.localScale;
			var to = info.destScale;

			while (target && Time.time - startTime < (info.scaleTime/2f))
			{
				var rate = (Time.time - startTime) / (info.scaleTime/2);
				target.transform.localScale = Vector3.Lerp(from, to, rate);
				yield return null;
			}

			startTime = Time.time;
			while (target && Time.time - startTime < (info.scaleTime/2f))
			{
				var rate = (Time.time - startTime) / (info.scaleTime/2f);
				target.transform.localScale = Vector3.Lerp(to, from, rate);
				yield return null;
			}

			target.transform.localScale = from;


			info.callback?.Invoke();
		}
		
		public static void ScaleLinear(ScaleInfo info)
		{
			Coroutine.Run(DoScaleLinear(info));
		}
		
		private static IEnumerator DoScaleLinear(ScaleInfo info)
		{
			var startTime = Time.time;
			var target = info.scaleObject;

			var from = target.transform.localScale;
			var to = info.destScale;

			while (target && Time.time - startTime < (info.scaleTime))
			{
				var rate = (Time.time - startTime) / info.scaleTime;
				target.transform.localScale = Vector3.Lerp(from, to, rate);
				yield return null;
			}

			target.transform.localScale = to;

			info.callback?.Invoke();
		}
		
		
		public static void RotateLinear(RotateInfo info)
		{
			Coroutine.Run(DoRotateLinear(info));
		}

		public static void RotateEaseInOut(RotateInfo info)
		{
			Coroutine.Run(DoRotateEaseInOut(info));
		}

		public static void RotateEaseInOut(List<RotateInfo> infos)
		{
			Coroutine.Run(DoRotateEaseInOut(infos));
		}
		
		
		public static IEnumerator DoRotateLinear(RotateInfo info)
		{
			var target = info.rotateObject;

			var from = info.originAngle;
			var to = info.destAngle;

			if (info.delayTime > 0)
			{
				yield return new WaitForSeconds(info.delayTime);
			}
			
			var startTime = Time.time;
			
			if (info.isLocal)
			{
				while (target && Time.time - startTime < info.rotateTime)
				{
					var rate = (Time.time - startTime) / info.rotateTime;
					target.transform.localEulerAngles = Vector3.Lerp(from, to, rate);
					yield return null;
				}
				
				target.transform.localEulerAngles = to;								
			}
			else
			{
				while (target && Time.time - startTime < info.rotateTime)
				{
					var rate = (Time.time - startTime) / info.rotateTime;
					target.transform.eulerAngles = Vector3.Lerp(from, to, rate);
					yield return null;
				}
				
				target.transform.eulerAngles = to;
			}

			info.callback?.Invoke();
		}
		
		
		public static IEnumerator DoRotateEaseInOut(RotateInfo info)
		{
			var startTime = Time.time;
			var target = info.rotateObject;

			var from = info.originAngle;
			var to = info.destAngle;

			if (info.delayTime > 0)
			{
				yield return new WaitForSeconds(info.delayTime);
			}

			var curve = AnimationCurve.EaseInOut(0, 0, info.rotateTime, 1);
			if (info.isLocal)
			{
				
				while (target && Time.time - startTime < info.rotateTime)
				{
					target.transform.localEulerAngles = Vector3.Lerp(from, to, curve.Evaluate(Time.time - startTime));
					yield return null;
				}
				
				target.transform.localEulerAngles = to;								
			}
			else
			{								
				while (target && Time.time - startTime < info.rotateTime)
				{
					target.transform.eulerAngles = Vector3.Lerp(from, to, curve.Evaluate(Time.time - startTime));
					yield return null;
				}
				
				target.transform.eulerAngles = to;
			}
			
			


			info.callback?.Invoke();
		}
		
		
		public static IEnumerator DoRotateEaseInOut(List<RotateInfo> infos)
		{
			foreach (var info in infos)
			{
				var startTime = Time.time;
				var target = info.rotateObject;

				var from = info.originAngle;
				var to = info.destAngle;

				if (info.delayTime > 0)
				{
					yield return new WaitForSeconds(info.delayTime);
				}

				var curve = AnimationCurve.EaseInOut(0, 0, info.rotateTime, 1);
				if (info.isLocal)
				{
				
					while (target && Time.time - startTime < info.rotateTime)
					{
						target.transform.localEulerAngles = Vector3.Lerp(from, to, curve.Evaluate(Time.time - startTime));
						yield return null;
					}
				
					target.transform.localEulerAngles = to;								
				}
				else
				{								
					while (target && Time.time - startTime < info.rotateTime)
					{
						target.transform.eulerAngles = Vector3.Lerp(from, to, curve.Evaluate(Time.time - startTime));
						yield return null;
					}
				
					target.transform.eulerAngles = to;
				}

				info.callback?.Invoke();
			}
		}
		
	}
}
