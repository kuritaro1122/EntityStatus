using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityBehavior.Status.Module {
    internal class ES_Particle : MonoBehaviour {
        [SerializeField] EntityStatus status = null;
        [Header("--- Particle ---")]
        [SerializeField] ParticleCon damageParticle = new ParticleCon();
        [SerializeField] ParticleCon defeatParticle = new ParticleCon();
        [SerializeField] ParticleCon recoverParticle = new ParticleCon();
#if UNITY_EDITOR
        [Header("--- Gizmos ---")]
        [SerializeField] float gizmosRadius = 1f;
#endif
        void Awake() {
            this.status.OnDamage += () => this.damageParticle.Play(this.transform);
            this.status.OnDefeat += () => this.defeatParticle.Play(this.transform);
            this.status.OnRecover += () => this.recoverParticle.Play(this.transform);
        }
        void OnValidate() {
            if (this.status == null) this.status = this.gameObject.GetComponent<EntityStatus>();
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            this.damageParticle.DrawGizmos(this.transform, this.gizmosRadius);
            this.defeatParticle.DrawGizmos(this.transform, this.gizmosRadius);
            this.recoverParticle.DrawGizmos(this.transform, this.gizmosRadius);
        }
#endif
        [System.Serializable]
        public class ParticleCon {
            [SerializeField] ParticleSystem particle = null;
            [Header("instantiate option")]
            [SerializeField] bool instantiate = true;
            [SerializeField] Vector3 offset = Vector3.zero;
            [SerializeField] Vector3 scale = Vector3.one;
            public void Play(Transform self) {
                if (this.particle == null) return;
                if (this.instantiate) {
                    var p = Instantiate(this.particle, self.TransformPoint(this.offset), Quaternion.identity);
                    p.transform.localScale = Vector3.Scale(p.transform.localScale, this.scale);
                    p.Play();
                } else {
                    this.particle.Play();
                }
            }
            public void DrawGizmos(Transform self, float radius) {
                Gizmos.DrawWireSphere(self.TransformPoint(this.offset), radius);
            }
        }
    }
}