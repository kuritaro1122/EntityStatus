using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityBehavior.Status;
using HyperNova.Game;
using System;

namespace HyperNova.Entity {
    [AddComponentMenu("HyperNova/Entity/Entity_Status")]
    public class EntityStatus_hyper : EntityStatus {
        [Header("=== Entity Status ===")]
        [SerializeField, Min(0)] protected int score;
        [SerializeField] bool ignoreScoreLimit = false;
        [SerializeField] bool tough = false;
        [Header("--- OffSet ---")]
        [SerializeField] Vector3 offSet = Vector3.zero;
        [Header("--- Effect ---")]
        [SerializeField] protected GameObject eDestroyEffect;
        [SerializeField] protected Vector3 effectScale = Vector3.one;
        private float defaultEffectScale = 0.3f;
        [SerializeField] Renderer[] bodysRenderer;
        [SerializeField] Color damageColor = Color.red;
        [Header("--- Drop Item ---")]
        [SerializeField] ObjectSelectType selectType;
        private enum ObjectSelectType { @Object, ObjectFunc }
        [SerializeField] protected GameObject dropItem;
        //[SerializeField] Vector3 itemOffset = Vector3.zero;
        private Func<GameObject> dropItemFunc = () => { return null; };
        private Coroutine colorEffectCoroutine = null;
        [Header("--- Out of Screen ---")]
        [SerializeField] Camera cam;
        [SerializeField] bool destroyOverLeft = false;
        [SerializeField] bool destroyOverRight = false;
        [SerializeField] bool destroyOverUp = false;
        [SerializeField] bool destroyOverDown = false;
        [SerializeField] Vector2 excess = new Vector2(0, 0);

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

        protected override void Awake() {
            base.Awake();
            if (this.cam == null) this.cam = Camera.main;
        }
        protected virtual void Update() {
            Limit();
            void Limit() {
                Vector2 posInCam = this.cam.WorldToViewportPoint(this.transform.position);
                Vector2 min = Vector2.zero - this.excess;
                Vector2 max = Vector2.one + this.excess;
                if (this.destroyOverLeft && posInCam.x < min.x) base.Defeat();
                if (this.destroyOverRight && posInCam.x > max.x) base.Defeat();
                if (this.destroyOverUp && posInCam.y > max.y) base.Defeat();
                if (this.destroyOverDown && posInCam.y < min.y) base.Defeat();
            }
        }
        protected override void OnValidate() {
            base.OnValidate();
        }

        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null, GameObject dropItem = null, bool? isShot = null, bool? takeNoDamage = null) {
            base.Set(HP, power, EntityType.enemy, isShot, takeNoDamage);
            this.score = score ?? this.score;
            Set(dropItem);
            return this;
        }
        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null, Func<GameObject> dropItemFunc = null, bool? isShot = null, bool? takeNoDamage = null) {
            base.Set(HP, power, EntityType.enemy, isShot, takeNoDamage);
            this.score = score ?? this.score;
            Set(dropItemFunc);
            return this;
        }

        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null) {
            base.Set(HP, power, EntityType.enemy, isShot);
            this.score = score ?? this.score;
            return this;
        }
        public EntityStatus_hyper Set(GameObject dropItem = null) {
            this.selectType = ObjectSelectType.Object;
            this.dropItem = dropItem;
            return this;
        }
        public EntityStatus_hyper Set(Func<GameObject> dropItemFunc) {
            this.selectType = ObjectSelectType.ObjectFunc;
            this.dropItemFunc = dropItemFunc;
            return this;
        }

        public EntityStatus_hyper Set(bool destroyOverLeft, bool destroyOverRight, bool destroyOverUp, bool destroyOverDown) {
            this.destroyOverLeft = destroyOverLeft;
            this.destroyOverRight = destroyOverRight;
            this.destroyOverUp = destroyOverUp;
            this.destroyOverDown = destroyOverDown;
            return this;
        }

        public override void Damage(float value = 1) {
            if (!base.takeNoDamage) {
                base.Damage(value);
                ADX2LEManager.Instance.PlaySE("EnemyHit");
                if (colorEffectCoroutine == null && this.gameObject != null && this.gameObject.activeInHierarchy == true)
                    colorEffectCoroutine = StartCoroutine(DamageColorEffect());
            }
        }
        public override void Defeat() {
            ADX2LEManager.Instance.PlaySE("Destroy2");
            GameManager.ScoreAddition(this.score, this.transform.TransformPoint(this.offSet), this.ignoreScoreLimit);
            InstantiateDestroyEffect();
            InstantiateDropItem();
            base.Defeat();
        }
        public void CollisionWithPlayer() {
            InstantiateDestroyEffect();
            if (!tough) {
                //SoundManager.enemyDestroySESource.Play();
                if (base.takeNoDamage) return;
                ADX2LEManager.Instance.PlaySE("Destroy2");
                //Destroy(this.gameObject);
                base.Defeat();
            }
        }

        private void InstantiateDestroyEffect(Vector3 relativePos) {
            if (eDestroyEffect != null)
                Instantiate(eDestroyEffect, this.transform.TransformPoint(relativePos), Quaternion.identity).transform.localScale = this.effectScale * this.defaultEffectScale;
        }
        private void InstantiateDestroyEffect() {
            InstantiateDestroyEffect(this.offSet);
        }
        private void InstantiateDropItem() {
            switch (this.selectType) {
                case ObjectSelectType.Object:
                    if (dropItem != null) Instantiate(dropItem, transform.TransformPoint(this.offSet), Quaternion.identity);
                    break;
                case ObjectSelectType.ObjectFunc:
                    GameObject obj = dropItemFunc();
                    if (obj != null) Instantiate(obj, transform.TransformPoint(this.offSet), Quaternion.identity);
                    break;
            }
        }
        private IEnumerator DamageColorEffect() {
            if (bodysRenderer == null) yield break;
            ColorChanger[] colorChangers = new ColorChanger[this.bodysRenderer.Length];
            for (int i = 0; i < colorChangers.Length; i++)
                colorChangers[i] = new ColorChanger(this.bodysRenderer[i].material);
            Color _damageColor = this.damageColor;
            foreach (var cc in colorChangers) cc.SetColor(_damageColor);
            yield return new WaitForSeconds(0.1f);
            foreach (var cc in colorChangers) cc.ResetColor();
            colorEffectCoroutine = null;
        }
    }
}
