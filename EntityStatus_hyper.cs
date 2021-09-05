using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityBehavior.Status;
using HyperNova.Game;

namespace HyperNova.Entity {
    [AddComponentMenu("HyperNova/Entity/Entity_Status")]
    public class EntityStatus_hyper : EntityStatus {
        [Header("=== Entity Status ===")]
        [SerializeField, Min(0)] protected int score;
        [SerializeField] bool ignoreScoreLimit = false;
        [SerializeField] bool tough = false;
        [Header("--- Effect ---")]
        [SerializeField] protected GameObject eDestroyEffect;
        [Header("--- Drop Item ---")]
        [SerializeField] protected GameObject dropItem;

        protected override void OnValidate() {
            base.OnValidate();
        }

        //引数がnullでも良いように
        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null, GameObject dropItem = null, bool? isShot = null, bool? takeNoDamage = null) {
            base.Set(HP, power, EntityType.enemy, isShot, takeNoDamage);
            this.score = score ?? this.score;
            this.dropItem = dropItem;
            return this;
        }

        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null) {
            base.Set(HP, power, EntityType.enemy, isShot);
            this.score = score ?? this.score;
            return this;
        }


        public EntityStatus_hyper Set(GameObject dropItem = null) {
            this.dropItem = dropItem;
            return this;
        }

        public override void Damage(float value = 1) {
            base.Damage(value);
            SoundManager.enemyHitSESource.Play();
        }

        public override void Defeat() {
            SoundManager.enemyDestroySESource.Play();
            GameManager.Instance.ScoreAddition(score, ignoreScoreLimit);
            InstantiateDestroyEffect();
            InstantiateDropItem();
            base.Defeat();
        }

        public void CollisionWithPlayer() {
            InstantiateDestroyEffect();
            if (!tough) {
                SoundManager.enemyDestroySESource.Play();
                Destroy(this.gameObject);
            }
        }

        private void InstantiateDestroyEffect(Vector3 relativePos) {
            if (eDestroyEffect != null)
                Instantiate(eDestroyEffect, transform.position + relativePos, Quaternion.identity);
        }
        private void InstantiateDestroyEffect() => InstantiateDestroyEffect(Vector3.zero);
        private void InstantiateDropItem() {
            if (dropItem != null)
                Instantiate(dropItem, transform.position, Quaternion.identity);
        }
    }
}
