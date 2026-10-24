using UnityEngine;

namespace Larje.Graphics.PostFX
{
    public abstract class PostFXEffect : ScriptableObject
    {
        public abstract PostFXProcessor CreateProcessor();
    }
}
