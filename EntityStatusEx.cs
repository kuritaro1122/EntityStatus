//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace EntityBehavior.Status {
//    [AddComponentMenu("EntityBehaviour/EntityStatusEx")]
//    public class EntityStatusEx : EntityStatus {
//        private enum StayDamageType { none, oneSide, both };
//        [Header("--- [Ex] Attack On Stay ---")]
//        [SerializeField] StayDamageType attackOnStay = StayDamageType.none;
//        [SerializeField] bool causeDamageIgnoreTakeDamageOnStay = false;
//        [SerializeField, Min(0f)] float powerOnStay = 1f;
//        [SerializeField, Min(0.1f)] float span = 1f;
//        private float count = 0f;
//        private readonly List<EntityStatus> attackedEntity = new List<EntityStatus>();
//        [Header("--- [Ex] Invincible time ---")]
//        [SerializeField] bool invincibleTime = false;
//        [SerializeField, Min(0f)] float time = 1f;
//        private float remainTime = 0f;
//        public bool Invincible { get { return this.invincibleTime && this.remainTime > 0f; } }
//        public float InvincibleTime { get { return this.invincibleTime ? this.time : 0f; } }
//        public float InvincibleRemainTime { get { return Mathf.Max(this.remainTime, 0f); } }
//        public float InvincibleRemainTimeRate { get { return (this.InvincibleTime > 0f) ? (this.InvincibleRemainTime / this.InvincibleTime) : 0f; } }

//        protected virtual void Update() {
//            if (attackOnStay != StayDamageType.none) UpdateAttackCount();
//            if (Invincible) this.remainTime -= Time.deltaTime;
//        }
//        protected virtual void OnTriggerStay(Collider other) {
//            CollisionOtherStay(other);
//        }
//        protected virtual void OnCollisionStay(Collision collision) {
//            CollisionOtherStay(collision.collider);
//        }

//        public override void Damage(float value = 1, int key = -1) {
//            if (!this.Invincible) {
//                base.Damage(value, key);
//                InvincibleStart();
//            }
//        }

//        private void InvincibleStart() {
//            if (this.invincibleTime) this.remainTime = this.InvincibleTime;
//        }
//        private void UpdateAttackCount() {
//            if (this.count < this.span) {
//                this.count += Time.deltaTime;
//            } else {
//                this.attackedEntity.Clear();
//                this.count = 0f;
//            }
//        }
//        private void CollisionOtherStay(Collider other) {
//            GameObject obj = other.gameObject;
//            if (obj == null) return;
//            foreach (var status in attackedEntity) {
//                if (status != null && obj == status.gameObject) return;
//            }
//            EntityStatus otherStatus = base.GetEffectionEntity(obj);
//            if (otherStatus != null) EntityCollision(otherStatus);
//        }
//        private void EntityCollision(EntityStatus otherStatus) {
//            switch (attackOnStay) {
//                default:
//                case StayDamageType.none:
//                    break;
//                case StayDamageType.both:
//                    this.Damage(otherStatus.Power);
//                    goto case StayDamageType.oneSide;
//                case StayDamageType.oneSide:
//                    otherStatus.Damage(this.powerOnStay, this.key);
//                    break;
//            }
//            this.attackedEntity.Add(otherStatus);
//        }
//    }
//}
