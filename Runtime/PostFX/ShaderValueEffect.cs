using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Larje.Graphics.PostFX
{
    [CreateAssetMenu(fileName = "ShaderValueEffect", menuName = "Larje/Graphics/PostFX/Shader Value Effect")]
    public sealed class ShaderValueEffect : PostFXEffect
    {
        [SerializeField] private Shader shader;
        [SerializeField] private string valueProperty = "_Value";

        public override PostFXProcessor CreateProcessor()
        {
            return new Processor(this);
        }

        public sealed class Processor : PostFXProcessor
        {
            private bool _destroyed;
            private readonly int _propertyId;
            private readonly Material _material;
            private readonly List<Func<float>> _providers = new List<Func<float>>();

            public override Material Material => _material;
            public override bool Enabled => _material != null && !_destroyed && GetValue() > 0f;

            public Processor(ShaderValueEffect effect) : base(effect)
            {
                _propertyId = Shader.PropertyToID(effect.valueProperty);
                if (effect.shader != null)
                {
                    _material = new Material(effect.shader);
                }
            }

            public void AddProvider(Func<float> provider)
            {
                if (!_providers.Contains(provider))
                {
                    _providers.Add(provider);
                }
            }

            public void RemoveProvider(Func<float> provider)
            {
                _providers.Remove(provider);
            }

            public override void Update()
            {
                if (!_destroyed && _material != null)
                {
                    _material.SetFloat(_propertyId, GetValue());
                }
            }

            public override void Destroy()
            {
                _destroyed = true;
                if (_material != null)
                {
                    Object.Destroy(_material);
                }
            }

            private float GetValue()
            {
                float result = 0f;
                foreach (Func<float> provider in _providers)
                {
                    result = Mathf.Max(result, provider());
                }

                return result;
            }
        }
    }
}
