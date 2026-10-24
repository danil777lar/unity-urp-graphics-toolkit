using UnityEngine;

namespace Larje.Graphics.PostFX
{
    public abstract class PostFXProcessor
    {
        private readonly PostFXEffect _effect;

        public PostFXEffect Effect => _effect;
        public abstract Material Material { get; }
        public abstract bool Enabled { get; }

        protected PostFXProcessor(PostFXEffect effect)
        {
            _effect = effect;
        }

        public abstract void Update();
        public abstract void Destroy();
    }
}
