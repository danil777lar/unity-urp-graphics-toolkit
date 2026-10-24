using Cysharp.Threading.Tasks;
using Larje.Core.Locations;
using UnityEngine;

namespace Larje.Graphics.Lightmaps
{
    [CreateAssetMenu(fileName = "LightmapLocationAction", menuName = "Larje/Graphics/Lightmap Location Action")]
    public sealed class LightmapLocationAction : LocationAction
    {
        [SerializeField] private LightmapPackConfig pack;

        public override void Apply(LocationDefinition location)
        {
            if (pack != null)
            {
                pack.ApplyAsync().Forget();
            }
        }
    }
}
