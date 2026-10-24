using System;
using System.Collections.Generic;
using Larje.Core.Saves;
using UnityEngine;

namespace Larje.Graphics.VolumetricLight
{
    [Serializable]
    public sealed class VolumetricLightMaterials
    {
        [SerializeField] private List<Entry> entries = new List<Entry>();

        public Material Get(ISaveService saves)
        {
            VolumetricLightQuality quality = saves != null ? saves.Get<GraphicsSettingsData>().VolumetricLight : VolumetricLightQuality.High;
            Entry entry = entries.Find(x => x.Quality == quality);
            return entry != null ? entry.Material : null;
        }

        [Serializable]
        private sealed class Entry
        {
            public VolumetricLightQuality Quality;
            public Material Material;
        }
    }
}
