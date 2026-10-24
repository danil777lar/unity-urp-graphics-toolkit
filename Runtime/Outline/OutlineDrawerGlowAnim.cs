using UnityEngine;

namespace Larje.Graphics.Outline
{
    public class OutlineDrawerGlowAnim : MonoBehaviour
    {
        [SerializeField] private float glowSpeed = 2f;
        [SerializeField] private float outlineWidthMultiplier = 1f;
        [SerializeField] private float outlineColorPower = 10f;

        private float _outlineWidthMultiplier = 1f;
        private float _outlineColorPower = 1f;

        private OutlineDrawer _outlineDrawer;

        private void Start()
        {
            _outlineDrawer = GetComponent<OutlineDrawer>();

            _outlineDrawer.OutlineColorPower.AddValue(() => _outlineColorPower);
            _outlineDrawer.OutlineWidthMultiplier.AddValue(() => _outlineWidthMultiplier);
        }

        private void Update()
        {
            _outlineWidthMultiplier = 1f + (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f * outlineWidthMultiplier;
            _outlineColorPower = 1f + (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f * outlineColorPower;
        }
    }
}
