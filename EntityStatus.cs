//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;

namespace EntityBehavior.Status {
    [AddComponentMenu("EntityBehavior/EntityStatus")]
    public /*abstract*/ class EntityStatus : MonoBehaviour {
        public enum EntityType { enemy, friend, obstacle }
        private static readonly string commonTag = "Entity";

        [Header("--- Entity Status ---")]
        [SerializeField] protected EntityType entityType;
        [SerializeField] protected bool isShot = false; //shot同士は属性に関わらず衝突しない。
        [SerializeField] bool takeNoDamage = false; //Defeat()でのみ破壊可能になる。
        [SerializeField, Min(0f)] float MaxHP = 20f;
        [SerializeField, Min(0f)] float HP = 20f; //shotの時は値の数だけ貫通する。
        [SerializeField, Min(0f)] float power = 5f;

        //[SerializeField] UnityEvent damagedEvents;
        //[SerializeField] UnityEvent defeatedEvents;

        public EntityType Type { get { return this.entityType; } }
        public float Power { get { return this.power; } }
        public float GetHP() => this.HP;
        public float GetMaxHP() => this.MaxHP;

        public void Set(float? HP, float? power, EntityType? entityType = null, bool? isShot = null, bool? takeNoDamage = null) {
            this.HP = HP ?? this.HP;
            this.power = power ?? this.power;
            this.entityType = entityType ?? this.entityType;
            this.isShot = isShot ?? this.isShot;
            this.takeNoDamage = takeNoDamage ?? this.takeNoDamage;
        }

        protected virtual void Awake() {
            HP = MaxHP;
            this.tag = commonTag;
            defeat = false;
        }
        protected virtual void OnValidate() {
            ClampHP();
        }
        protected virtual void OnTriggerEnter(Collider other) {
            CollisionConfirmation(other);
        }
        protected virtual void OnCollisionEnter(Collision collision) {
            CollisionConfirmation(collision.collider);
        }

        private void CollisionConfirmation(Collider other) {
            if (other.gameObject.tag == commonTag) {
                EntityStatus otherStatus = other.gameObject.GetComponent<EntityStatus>();
                if (otherStatus == null) return;
                bool differentType = (this.Type != otherStatus.Type);
                bool bothAreShot = this.isShot && otherStatus.isShot;
                if (differentType && !bothAreShot) OtherEntityCollision(otherStatus);
            }
        }
        private void OtherEntityCollision(EntityStatus otherStatus) {
            int id = this.gameObject.GetInstanceID();
            int otherId = otherStatus.gameObject.GetInstanceID();
            if (id < otherId) {
                float otherPower = otherStatus.Power;
                if (this.Power > 0f) otherStatus.Damage(this.Power);
                if (otherPower > 0f) this.Damage(otherPower);
            }
        }
        private bool defeat = false;
        public virtual void Damage(float value = 1) {
            if (value <= 0f) {
                Debug.LogWarningFormat("Damage value <= 0f", this);
                return;
            }
            if (takeNoDamage) return;
            HP -= !isShot ? value : 1f; //shotの時はダメージが1固定
            //damagedEvents.Invoke();
            if (this.HP <= 0f) {
                if (defeat == false) {
                    defeat = true;
                    Defeat();
                } else Debug.LogWarning("EntityStatus/already defeat.");
            }
        }
        public virtual void Recover(float value) {
            HP += !isShot ? value : 1f;
            ClampHP();
        }
        public virtual void Defeat() {
            //defeatedEvents.Invoke();
            Destroy(this.gameObject);
        }

        private void ClampHP() {
            HP = Mathf.Clamp(HP, 0f, MaxHP);
            if (MaxHP <= 0f) Debug.LogWarningFormat("MaxHP <= 0f", this);
        }
    }
}
