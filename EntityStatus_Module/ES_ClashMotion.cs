using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityBehavior.Status.Module;
using ObjectOrderControl;
using KuriKit.Rigidbodys;

namespace EntityBehavior.Status.Module {
    [RequireComponent(typeof(ContactlessRigidbody))]
    class ES_ClashMotion : MonoBehaviour {
        private const string StageObjectTag = "StageObject";
        //[SerializeField] Rigidbody rb = null;
        [SerializeField] ContactlessRigidbody rb;
        [SerializeField] EntityStatus status = null;
        [Header("---")]
        [SerializeField, Range(MinHardness + 1, MaxPriority)] public int blastPriority = 0;
        [SerializeField, Range(MinHardness, MaxPriority)] public int hardnessPriority = 0;
        const int MaxPriority = 10;
        const int MinHardness = -1;
        [Header("--- ClashMotion ---")]
        [SerializeField, Min(0f)] float force = 5f;
        [SerializeField] List<ClashTorque> clashTorques = new List<ClashTorque>();
        [Header("--- Explode ---")]
        [SerializeField] EntityStatus.DefeatType defeatType = EntityStatus.DefeatType.destroy;
        [SerializeField] ES_Particle.ParticleCon particle = new ES_Particle.ParticleCon();

        // flags
        private bool clashed = false;
        private bool exploded = false;

        public Vector3 ForceToGive(Transform target) {
            return this.force * this.rb.velocity.normalized;
        }

        void Start() {
            // 撃破時に墜落する
            //this.status.OnDamageWithOther += g => Debug.Log($"damage:{g.name}");
            this.status.OnDefeat += () => Debug.Log("defeat");
            this.status.OnDefeatWithOther += g => Debug.Log($"defeat:{g.name}");
            this.status.OnDefeatWithOther += o => ClashMotion(o.GetComponent<ES_ClashMotion>());
            //this.status.OnDamage += () => Debug.Log("damage");
            
        }
        void OnValidate() {
            if (this.enabled) {
                if (this.rb == null) this.rb = this.gameObject.GetComponent<ContactlessRigidbody>();
                if (this.status == null) this.status = this.gameObject.GetComponent<EntityStatus>();
                this.status.defeatType = EntityStatus.DefeatType.none;
            }
        }
        void OnTriggerStay(Collider other) {
            OnCollisionOther(other);
        }
        void OnCollisionStay(Collision collision) {
            OnCollisionOther(collision.collider);
        }

        private void OnCollisionOther(Collider other) {
            if (!other.CompareTag(StageObjectTag)) return;
            if (this.clashed && !this.exploded) Explode();
        }
        private void ClashMotion(ES_ClashMotion other) {
            if (this.clashed) return;
            this.clashed = true;
            this.status.statusActive = false;
            if (this.hardnessPriority > MinHardness && (other == null || this.hardnessPriority >= other.blastPriority)) {
                // 吹っ飛ばされる力. 相手がES_ClashMotionを持っていれば返す
                Vector3 force = Vector3.zero;
                if (other != null) force = other.ForceToGive(this.transform);
                Vector3 torque = Vector3.zero;
                foreach (var t in this.clashTorques) {
                    torque += t.GetTorque(force);
                }
                this.rb.AddRelativeTorque(torque/*, ForceMode.VelocityChange*/);
                this.rb.useGravity = true;
            } else Explode();
        }
        private void Explode() {
            if (this.exploded) return;
            this.particle.Play(this.transform);
            switch (this.defeatType) {
                case EntityStatus.DefeatType.destroy:
                    Destroy(this.gameObject);
                    break;
                case EntityStatus.DefeatType.nonActive:
                    this.gameObject.SetActive(false);
                    break;
                case EntityStatus.DefeatType.none:
                    break;
            }
        }

        [System.Serializable]
        class ClashTorque {
            [SerializeField] Vector3 axis;
            [SerializeField] TorqueType torqueType;
            [SerializeField] float speed;
            enum TorqueType { left, right, dynamic }
            public Vector3 GetTorque(Vector3 force) {
                if (this.torqueType == TorqueType.left) return speed * axis.normalized;
                else return -speed * axis.normalized;
            }
        }
    }
}