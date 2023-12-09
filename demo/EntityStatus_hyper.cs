//using System.Collections;
//using UnityEngine;
//using EntityBehavior.Status;
//using System;

//namespace HyperNova.Entity {
//    [AddComponentMenu("HyperNova/Entity/EntityStatus_hyper")]
//    public class EntityStatus_hyper : EntityStatus {
//        [Header("=== Entity Status ===")]
//        [SerializeField, Min(0)] protected int score;
//        public int Score => this.score;
//        [SerializeField] bool ignoreScoreLimit = false;
//        [SerializeField] bool ignoreScoreCombo = false;
//        [SerializeField] bool tough = false;
//        [Header("--- OffSet ---")]
//        [SerializeField] Vector3 offSet = Vector3.zero;
//        [Header("--- Effect ---")]
//        [SerializeField] protected GameObject eDestroyEffect;
//        [SerializeField] protected Vector3 effectScale = Vector3.one;
//        private float defaultEffectScale = 0.3f;
//        [SerializeField] Renderer[] bodysRenderer;
//        [SerializeField] Color damageColor = Color.red;
//        [Header("--- Drop Item ---")]
//        [SerializeField] ObjectSelectType selectType;
//        private enum ObjectSelectType { @Object, ObjectFunc }
//        [SerializeField] protected GameObject dropItem;
//        private Func<GameObject> dropItemFunc = () => { return null; };
//        private Coroutine colorEffectCoroutine = null;
//        [Header("--- Out of Screen ---")]
//        [SerializeField] Camera cam;
//        [SerializeField] bool destroyOverLeft = false;
//        [SerializeField] bool destroyOverRight = false;
//        [SerializeField] bool destroyOverUp = false;
//        [SerializeField] bool destroyOverDown = false;
//        [SerializeField] Vector2 excess = new Vector2(0, 0);
//        private struct ColorChanger {
//            [SerializeField] Material material;
//            [SerializeField] Color defaultColor;
//            public ColorChanger(Material material) {
//                this.material = material;
//                this.defaultColor = material.color;
//            }
//            public void SetColor(Color color) => this.material.color = color;
//            public void AddColor(Color color) => this.material.color += color;
//            public void ResetColor() => this.material.color = defaultColor;
//        }
//        private ColorChanger[] colorChangers = null;

//        protected override void Awake() {
//            base.Awake();
//            if (this.cam == null) this.cam = Camera.main;
//            InitColorChanger();
//        }
//        protected virtual void Update() {
//            Limit();
//            void Limit() {
//                Vector2 posInCam = this.cam.WorldToViewportPoint(this.transform.position);
//                Vector2 min = Vector2.zero - this.excess;
//                Vector2 max = Vector2.one + this.excess;
//                if (this.destroyOverLeft && posInCam.x < min.x) base.Defeat();
//                if (this.destroyOverRight && posInCam.x > max.x) base.Defeat();
//                if (this.destroyOverUp && posInCam.y > max.y) base.Defeat();
//                if (this.destroyOverDown && posInCam.y < min.y) base.Defeat();
//            }
//        }
//        protected override void OnValidate() {
//            base.OnValidate();
//        }
//        protected virtual void OnDrawGizmos() {
//            Gizmos.DrawWireSphere(this.transform.TransformPoint(offSet), 1f);
//        }

//        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null, GameObject dropItem = null, bool? isShot = null, DamageType? damageType = null) {
//            base.Set(HP, power, EntityType.enemy, isShot, damageType);
//            this.score = score ?? this.score;
//            Set(dropItem);
//            return this;
//        }
//        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null, Func<GameObject> dropItemFunc = null, bool? isShot = null, DamageType? damageType = null) {
//            base.Set(HP, power, EntityType.enemy, isShot, damageType);
//            this.score = score ?? this.score;
//            Set(dropItemFunc);
//            return this;
//        }
//        public EntityStatus_hyper Set(float? HP = null, float? power = null, int? score = null) {
//            base.Set(HP, power, EntityType.enemy, isShot);
//            this.score = score ?? this.score;
//            return this;
//        }
//        public EntityStatus_hyper Set(GameObject dropItem = null) {
//            this.selectType = ObjectSelectType.Object;
//            this.dropItem = dropItem;
//            return this;
//        }
//        public EntityStatus_hyper Set(Func<GameObject> dropItemFunc) {
//            this.selectType = ObjectSelectType.ObjectFunc;
//            this.dropItemFunc = dropItemFunc;
//            return this;
//        }
//        public EntityStatus_hyper Set(int score) {
//            this.score = score;
//            return this;
//        }
//        public EntityStatus_hyper Set(bool destroyOverLeft, bool destroyOverRight, bool destroyOverUp, bool destroyOverDown) {
//            this.destroyOverLeft = destroyOverLeft;
//            this.destroyOverRight = destroyOverRight;
//            this.destroyOverUp = destroyOverUp;
//            this.destroyOverDown = destroyOverDown;
//            return this;
//        }

//        public override void Damage(float value = 1, int key = -1) {
//            base.Damage(value, key);
//            if (base.EnableDamage(key)) {
//                Sound.SoundList.EnemyHitSE();
//                if (colorEffectCoroutine == null && this.gameObject != null && this.gameObject.activeInHierarchy == true)
//                    colorEffectCoroutine = StartCoroutine(DamageColorEffect());
//            }
//        }
//        public override void Defeat() {
//            Sound.SoundList.EnemyDestroySE();
//            InstantiateDestroyEffect();
//            InstantiateDropItemAndScore();
//            ResetColorAll();
//            base.Defeat();
//        }
//        protected void InstantiateDropItemAndScore() {
//            GameManager.ScoreAddition(this.score, this.transform.TransformPoint(this.offSet), this.ignoreScoreLimit, this.ignoreScoreCombo);
//            InstantiateDropItem();
//        }
//        public void CollisionWithPlayer() {
//            if (!tough) {
//                if (!base.EnableDamage(-1)) return;
//                Sound.SoundList.EnemyDestroySE();
//                InstantiateDestroyEffect();
//                base.Defeat();
//            }
//        }
//        protected void InstantiateDestroyEffect(Vector3 relativePos, Vector3 localScale) {
//            if (eDestroyEffect != null)
//                Instantiate(eDestroyEffect, this.transform.TransformPoint(relativePos), Quaternion.identity).transform.localScale = localScale;
//        }
//        protected void InstantiateDestroyEffect() {
//            InstantiateDestroyEffect(this.offSet, this.effectScale * this.defaultEffectScale);
//        }

//        private void InstantiateDropItem() {
//            switch (this.selectType) {
//                case ObjectSelectType.Object:
//                    if (dropItem != null) Instantiate(dropItem, transform.TransformPoint(this.offSet), Quaternion.identity);
//                    break;
//                case ObjectSelectType.ObjectFunc:
//                    GameObject obj = dropItemFunc();
//                    if (obj != null) Instantiate(obj, transform.TransformPoint(this.offSet), Quaternion.identity);
//                    break;
//            }
//        }
//        private void InitColorChanger() {
//            this.colorChangers = new ColorChanger[this.bodysRenderer.Length];
//            for (int i = 0; i < this.colorChangers.Length; i++)
//                this.colorChangers[i] = new ColorChanger(this.bodysRenderer[i].material);
//        }
//        private void ResetColorAll() {
//            if (this.colorChangers == null) return;
//            foreach (var c in this.colorChangers)
//                c.ResetColor();
//        }
//        private IEnumerator DamageColorEffect() {
//            if (bodysRenderer != null) {
//                Color _damageColor = this.damageColor;
//                foreach (var cc in this.colorChangers) cc.SetColor(_damageColor);
//                yield return new WaitForSeconds(0.1f);
//            }
//            ResetColorAll();
//            colorEffectCoroutine = null;
//        }
//    }
//}
