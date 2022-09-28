using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityBehavior.Status {
    class ES_DamageColorCon : MonoBehaviour {
        [SerializeField] EntityStatus status = null;

        void Awake() {
            status.OnDamage += OnDamage;
            InitColorChanger();
        }
        void OnValidate() {
            if (this.status == null) this.status = this.gameObject.GetComponent<EntityStatus>();
        }

        private void OnDamage() {
            if (this.colorEffectCoroutine == null && this.gameObject != null && this.gameObject.activeInHierarchy) {
                this.colorEffectCoroutine = StartCoroutine(DamageColorEffect());
            }
        }

        [Header("--- Color ---")]
        [SerializeField] Renderer[] bodyRenderers;
        [SerializeField] Color damageColor = Color.red;
        private Coroutine colorEffectCoroutine = null;
        private struct ColorChanger {
            [SerializeField] Material material;
            [SerializeField] Color defaultColor;
            public ColorChanger(Material material) {
                this.material = material;
                this.defaultColor = material.color;
            }
            public void SetColor(Color color) => this.material.color = color;
            public void AddColor(Color color) => this.material.color += color;
            public void ResetColor() => this.material.color = defaultColor;
        }
        private ColorChanger[] colorChangers = null;
        private IEnumerator DamageColorEffect() {
            if (bodyRenderers != null) {
                Color _damageColor = this.damageColor;
                foreach (var cc in this.colorChangers) cc.SetColor(_damageColor);
                yield return new WaitForSeconds(0.1f);
            }
            ResetColorAll();
            colorEffectCoroutine = null;
        }
        private void InitColorChanger() {
            this.colorChangers = new ColorChanger[this.bodyRenderers.Length];
            for (int i = 0; i < this.colorChangers.Length; i++)
                this.colorChangers[i] = new ColorChanger(this.bodyRenderers[i].material);
        }
        private void ResetColorAll() {
            if (this.colorChangers == null) return;
            foreach (var c in this.colorChangers)
                c.ResetColor();
        }
    }
}