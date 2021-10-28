//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;

namespace EntityBehavior.Status {
    [AddComponentMenu("EntityBehaviour/EntityStatus")]
    public /*abstract*/ class EntityStatus : MonoBehaviour {
        public enum EntityType { enemy, friend, obstacle }
        public static readonly string commonTag = "Entity";

        [Header("--- Entity Status ---")]
        [SerializeField] protected EntityType entityType;
        [SerializeField] protected bool isShot = false; //shot同士は属性に関わらず衝突しない。
        [SerializeField] bool takeNoDamage = false; //Defeat()でのみ破壊可能になる。
        [SerializeField, Min(0f)] float maxHP = 20f;
        [SerializeField, Min(0f)] float hp = 20f; //shotの時は値の数だけ貫通する。
        [SerializeField, Min(0f)] float power = 5f;
        private bool defeat = false;
        //[Header("--- Event ---")]
        private System.Action damagedAction = () => { };
        private System.Action defeatedAction = () => { };
        private System.Action recoveredAction = () => { };

        public EntityType Type { get { return this.entityType; } }
        public float MaxHP { get { return this.maxHP; } }
        public float HP { get { return this.hp; } }
        public float Power { get { return this.power; } }

        public EntityStatus Set(float? hp, float? power, EntityType? entityType = null, bool? isShot = null, bool? takeNoDamage = null) {
            this.hp = hp ?? this.HP;
            this.power = power ?? this.power;
            this.entityType = entityType ?? this.entityType;
            this.isShot = isShot ?? this.isShot;
            this.takeNoDamage = takeNoDamage ?? this.takeNoDamage;
            return this;
        }
        public EntityStatus SetDamagedAction(System.Action action, bool reset = false) {
            if (reset) this.damagedAction = () => { };
            this.damagedAction += action;
            return this;
        }
        public EntityStatus SetDefeatedAction(System.Action action, bool reset = false) {
            if (reset) this.defeatedAction = () => { };
            this.defeatedAction += action;
            return this;
        }
        public EntityStatus SetRecoverdAction(System.Action action, bool reset = false) {
            if (reset) this.recoveredAction = () => { };
            this.recoveredAction += action;
            return this;
        }

        protected virtual void Awake() {
            this.hp = MaxHP;
            this.tag = commonTag;
            defeat = false;
        }
        protected virtual void OnValidate() {
            ClampHP();
        }

        protected virtual void OnTriggerEnter(Collider other) {
            CollisionOther(other);
        }
        protected virtual void OnCollisionEnter(Collision collision) {
            CollisionOther(collision.collider);
        }

        private void CollisionOther(Collider other) {
            EntityStatus otherStatus = GetEffectionEntity(other.gameObject);
            if (otherStatus != null) EntityCollision(otherStatus);
            //if (!ReferenceEquals(otherStatus, null)) EntityCollision(otherStatus);
        }
        private void EntityCollision(EntityStatus otherStatus) {
            int id = this.gameObject.GetInstanceID();
            int otherId = otherStatus.gameObject.GetInstanceID();
            if (id < otherId) {
                float otherPower = otherStatus.Power;
                if (this.Power > 0f) otherStatus.Damage(this.Power);
                if (otherPower > 0f) this.Damage(otherPower);
            }
        }

        public EntityStatus GetEffectionEntity(GameObject obj) {
            if (!obj.CompareTag(commonTag)) return null;
            EntityStatus entityStatus = obj.GetComponent<EntityStatus>();
            if (entityStatus == null) return null;
            bool differentType = (this.Type != entityStatus.Type);
            bool bothAreShot = this.isShot && entityStatus.isShot;
            if (differentType && !bothAreShot) return entityStatus;
            return null;
        }
        
        public virtual void Damage(float value = 1) {
            if (value <= 0f) {
                Debug.LogWarningFormat("Damage value <= 0f", this);
                return;
            }
            if (takeNoDamage) return;
            this.hp -= !isShot ? value : 1f; //shotの時はダメージが1固定
            this.damagedAction();
            if (this.HP <= 0f) {
                if (defeat == false) {
                    defeat = true;
                    Defeat();
                } else Debug.LogWarning("EntityStatus/already defeat.");
            }
        }
        public virtual void Recover(float value) {
            this.hp += !isShot ? value : 1f;
            ClampHP();
            this.recoveredAction();
        }
        public virtual void Defeat() {
            this.defeatedAction();
            Destroy(this.gameObject);
        }

        private void ClampHP() {
            this.hp = Mathf.Clamp(HP, 0f, MaxHP);
            if (MaxHP <= 0f) Debug.LogWarningFormat("MaxHP <= 0f", this);
        }
    }

    public static class EntityStatusComponenter {
        public static EntityStatus ComponentEntityStatus(this GameObject self) {
            return ComponentEntityStatus<EntityStatus>(self);
        }
        public static S ComponentEntityStatus<S>(this GameObject self) where S : EntityStatus {
            S entityStatus = self.GetComponent<S>();
            if (entityStatus == null) {
                entityStatus = self.AddComponent<S>();
                Debug.Log(entityStatus.GetType() + " component added.", self);
            }
            return entityStatus;
        }
    }
}
