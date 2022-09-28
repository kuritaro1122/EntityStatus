using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EntityBehavior.Status {
    [AddComponentMenu("EntityBehaviour/EntityStatus")]
    public class EntityStatus : MonoBehaviour {
        public enum EntityType { enemy, friend, obstacle }
        public enum DefeatType { destroy, nonActive, none }
        public const string commonTag = "Entity";
        [SerializeField] public bool statusActive = true;
        [SerializeField, Min(-1), Tooltip("計算優先度. 値が高いEntityのEntityCollisionを使う")] public int priority = 0;
        [Header("--- Entity Status ---")]
        [SerializeField] protected EntityType entityType = EntityType.enemy;
        [SerializeField] protected bool isShot = false; //shot同士は属性に関わらず衝突しない。
        [SerializeField, Tooltip("Damageを無効化する.（Keyによるダメージは除く）")] bool takeNoDamage = false;
        [SerializeField, Min(0f)] protected float maxHP = 20f;
        [SerializeField, Min(0f)] protected float hp = 20f; //shotの時は値の数だけ貫通する。
        [SerializeField, Min(0f)] protected float power = 5f;

        [Header("--- Defeat ---")]
        [SerializeField] public DefeatType defeatType = DefeatType.destroy;
        [Header("--- Key Option ---")]
        [SerializeField] public List<int> keys = new List<int>();
        /// <summary>
        /// keyによるのダメージ量と受けた時の処理
        /// </summary>
        private List<KeyDamageFunc> keyOnDamageFuncs = new List<KeyDamageFunc>();
        class KeyDamageFunc {
            private int key;
            /// <summary>
            /// getDamage(self, otherPower, damage)
            /// </summary>
            private Func<EntityStatus, float, float> getDamage;
            public KeyDamageFunc(int key, Func<EntityStatus, float, float> getDamage) {
                this.key = key;
                this.getDamage = getDamage;
            }
            public bool GetDamage(EntityStatus self, EntityStatus other, out float damage) {
                if (other.keys.Contains(this.key)) {
                    damage = getDamage(self, other.Power);
                    return true;
                }
                damage = 0f;
                return false;
            }
        }

        /// <summary>
        /// keyによる追加ダメージの関数を追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="getDamage"></param>
        /// <returns></returns>
        public EntityStatus SetKeyDamageFuncs(int key, Func<EntityStatus, float, float> getDamage) {
            this.keyOnDamageFuncs.Add(new KeyDamageFunc(key, getDamage));
            return this;
        }

        enum AttackType { normal, damage, defeat }

        private bool defeated = false;
        /// <summary>EntityStatusの接触によるDamage時に呼ばれる. OnDamageより前に実行される.</summary>
        public event System.Action<GameObject> OnDamageWithOther = t => { };
        /// <summary>Damage時に呼ばれる.</summary>
        public event System.Action OnDamage = () => { };
        /// <summary>EntityStatusの接触によるDefeat時に呼ばれる. OnDefeatより前に実行される.</summary>
        public event System.Action<GameObject> OnDefeatWithOther = t => { };
        /// <summary>Defeat時に呼ばれる.</summary>
        public event System.Action OnDefeat = () => { };
        public event System.Action OnRecover = () => { };
        public event System.Action<GameObject> OnCollideOtherStatus = t => { };
        public EntityType EntityType_ => this.entityType;
        public float MaxHP => this.maxHP;
        public float HP => this.hp;
        public float Power => this.power;
        public bool IsShot => this.isShot;
        public bool Defeated => this.defeated;

        public EntityStatus Set(float? hp = null, float? power = null, EntityType? entityType = null, bool? isShot = null, bool takeNoDamage = false) {
            this.hp = hp ?? this.HP;
            this.maxHP = hp ?? MaxHP;
            this.power = power ?? this.power;
            this.entityType = entityType ?? this.entityType;
            this.isShot = isShot ?? this.isShot;
            this.takeNoDamage = takeNoDamage;
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

        protected virtual void CollisionOther(Collider other) {
            EntityStatus otherStatus = GetEffectionEntity<EntityStatus>(other.gameObject);
            if (otherStatus != null) EntityCollision(otherStatus);
            //if (!ReferenceEquals(otherStatus, null)) EntityCollision(otherStatus);
        }
        protected virtual void EntityCollision(EntityStatus otherStatus) {
            if (!this.statusActive || !otherStatus.statusActive) return;
            this.OnCollideOtherStatus(otherStatus.gameObject);
            int id = this.gameObject.GetInstanceID();
            int otherId = otherStatus.gameObject.GetInstanceID();
            if (this.priority < otherStatus.priority) {
                return;
            }
            if (this.priority > otherStatus.priority || id < otherId) {
                if (this.Power >= 0f) otherStatus.Damage(this);
                if (otherStatus.Power >= 0f) this.Damage(otherStatus);
            } 
        }
        public T GetEffectionEntity<T>(GameObject obj) where T : EntityStatus {
            if (!obj.CompareTag(commonTag)) return null;
            T entityStatus = obj.GetComponent<T>();
            if (entityStatus == null) return null;
            bool differentType = (this.EntityType_ != entityStatus.EntityType_);
            bool bothAreShot = this.isShot && entityStatus.isShot;
            if (differentType && !bothAreShot) return entityStatus;
            return null;
        }
        public bool EnableDamage() {
            if (!this.statusActive) return false;
            else if (this.Defeated) return false;
            else return true;
        }
        public virtual void Damage(float value) {
            // ダメージが有効かチェック
            if (!EnableDamage()) return;
            this.hp -= value;
            this.OnDamage();
            // ダメージを与える
            if (this.HP <= 0f) Defeat();
        }
        public virtual void Damage(EntityStatus other) {
            // 基本ダメージ（shotの場合は1とする）
            float damageVal = this.IsShot ? 1 : other.Power;
            // keyによる追加ダメージ・追加処理
            foreach (var df in this.keyOnDamageFuncs) {
                float _damage;
                bool success = df.GetDamage(this, other, out _damage);
                if (success) damageVal += _damage;
            }
            this.hp -= damageVal;
            this.OnDamageWithOther(other.gameObject);
            this.OnDamage();
            if (this.HP <= 0f) Defeat(other);
        }
        public virtual void Recover(float value) {
            // 回復（shotの場合は1とする）
            this.hp += !isShot ? value : 1f;
            ClampHP();
            this.OnRecover();
        }
        public virtual void RecoverFull(bool recoverAction = true) {
            this.hp = this.MaxHP;
            ClampHP();
            if (recoverAction) this.OnRecover();
            this.defeated = false;
        }
        public virtual void Defeat(EntityStatus other = null) {
            if (this.defeated) return; 
            if (other != null) this.OnDefeatWithOther(other.gameObject);
            this.OnDefeat();
            this.defeated = true;
            this.hp = 0f;
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
