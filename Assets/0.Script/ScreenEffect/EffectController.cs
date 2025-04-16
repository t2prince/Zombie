using System;
using Jjamcat.Util;
using UnityEngine;

namespace Jamcat.Effect.ScreenEffect
{
    public class EffectController : SingletonGameObject<EffectController>
    {
        public FadeInOut fadeInOut;

        private void Awake()
        {
            fadeInOut = GetComponentInChildren<FadeInOut>();
        }
    }
}