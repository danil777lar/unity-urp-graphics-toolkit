using System;
using Larje.Core.Saves;
using Larje.Graphics.VolumetricLight;

namespace Larje.Graphics
{
    [Serializable]
    [SaveSection("graphics_settings", SaveScope.System)]
    public sealed class GraphicsSettingsData
    {
        public VolumetricLightQuality VolumetricLight = VolumetricLightQuality.High;
    }
}
