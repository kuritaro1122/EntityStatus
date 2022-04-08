using UnityEngine;
using System;

namespace EntityBehavior.Status {
    [AddComponentMenu("EntityBehaviour/EntityStatus")]
    public /*abstract*/ class EntityStatus : MonoBehaviour {
        public enum EntityType { enemy, friend, obstacle }
        public enum DefeatType { destroy, nonActive, none }
        public const string commonTag = "Entity";
        [SerializeField] public bool statusActive = true;
        [Header("--- Entity Status ---")]
        [SerializeField] protected EntityType entityType;
        [SerializeField] protected bool isShot = false; //shot同士は属性に関わらず衝突しない。
        public enum DamageType { normal, takeNoDamage, keyOnly }
        [SerializeField] public DamageType damageType = DamageType.normal;
        [SerializeField, Tooltip("takeNoDamage == trueの時、keyが一致すればダメージが通る.")] public int key = -1;
        [SerializeField, Min(0f)] protected float maxHP = 20f;
        [SerializeField, Min(0f)] protected float hp = 20f; //shotの時は値の数だけ貫通する。
        [SerializeField, Min(0f)] protected float power = 5f;
        [SerializeField] DefeatType defeatType = DefeatType.destroy;
        private bool defeated = false;
        private System.Action damagedAction = () => { };
        private System.Action defeatedAction = () => { };
        private System.Action recoveredAction = () => { };
        public EntityType EntityType_ { get { return this.entityType; } }
        public float MaxHP { get { return this.maxHP; } }
        public float HP { get { return this.hp; } }
        public float Power { get { return this.power; } }
        public bool Defeated { get { return this.defeated; } }

        public EntityStatus Set(float? hp = null, float? power = null, EntityType? entityType = null, bool? isShot = null, DamageType? damageType = null) {
            this.hp = hp ?? this.HP;
            this.maxHP = hp ?? MaxHP;
            this.power = power ?? this.power;
            this.entityType = entityType ?? this.entityType;
            this.isShot = isShot ?? this.isShot;
            this.damageType = damageType ?? this.damageType;
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
            defeated = false;
        }
        protected virtual void OnEnable() {
            this.hp = maxHP;
            this.tag = commonTag;
            this.defeated = false;
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
            if (!this.statusActive || !otherStatus.statusActive) return;
            int id = this.gameObject.GetInstanceID();
            int otherId = otherStatus.gameObject.GetInstanceID();
            if (id < otherId) {
                //Debug.Log($"entitystatus/collision/{this.gameObject.name} {otherStatus.gameObject.name} frame:{Time.frameCount}");
                float otherPower = otherStatus.Power;
                if (this.Power >= 0f) otherStatus.Damage(this.Power, this.key);
                if (otherPower >= 0f) this.Damage(otherPower, otherStatus.key);
            }
        }
        public EntityStatus GetEffectionEntity(GameObject obj) {
            if (!obj.CompareTag(commonTag)) return null;
            EntityStatus entityStatus = obj.GetComponent<EntityStatus>();
            if (entityStatus == null) return null;
            bool differentType = (this.EntityType_ != entityStatus.EntityType_);
            bool bothAreShot = this.isShot && entityStatus.isShot;
            if (differentType && !bothAreShot) return entityStatus;
            return null;
        }
        protected bool EnableDamage(int key) {
            if (this.damageType == DamageType.takeNoDamage) return false;
            else if (this.damageType == DamageType.keyOnly) return this.key == key;
            else return true;
        }
        public virtual void Damage(float value, int key = -1) {
            if (this.isShot) value = 1; //###
            if (!EnableDamage(key)) {
                return;
            }
            this.hp -= !this.isShot ? value : 1f; //shotの時はダメージが1固定
            this.damagedAction();
            if (this.HP <= 0f) {
                if (defeated == false) {
                    defeated = true;
                    Defeat();
                } else {
                    if (defeatType == DefeatType.none) return;
                    //Debug.LogWarning("EntityStatus/already defeat.");
                }
            }
        }
        public virtual void Recover(float value) {
            this.hp += !isShot ? value : 1f;
            ClampHP();
            this.recoveredAction();
        }
        public virtual void RecoverFull(bool recoverAction = true) {
            this.hp = this.MaxHP;
            ClampHP();
            if (recoverAction) this.recoveredAction();
            this.defeated = false;
        }
        public virtual void Defeat() {
            this.power = 0f;
            this.defeatedAction();
            switch (this.defeatType) {
                case DefeatType.destroy:
                    Destroy(this.gameObject);
                    break;
                case DefeatType.nonActive:
                    this.gameObject.SetActive(false);
                    break;
                case DefeatType.none:
                    break;
            }
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
