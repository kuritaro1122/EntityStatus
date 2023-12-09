using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace EntityBehavior.Status {
    public interface IEntityStatusKeyController {
        Array GetFlagValues();
        string[] GetFlagNames();
        int GetSelfFlagBit();
        void SetSelfFlagBit(int flag);
        int GetEffectiveFlagBit();
        void SetEffectiveFlagBit(int flag);
        void OnUpdateEditor();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="F">Bit Flag</typeparam>
    public abstract class BaseEntityStatusKeyController<F> : MonoBehaviour, IEntityStatusKeyController where F : System.Enum {
        [SerializeField] EntityStatus status = null;
        [SerializeField] public int effectiveFlag;
        public F BitFlag { get; set; }
        public Array GetFlagValues() => Enum.GetValues(typeof(F));
        public string[] GetFlagNames() => Enum.GetNames(typeof(F));
        public int GetSelfFlagBit() => this.status.bitFlag;
        public void SetSelfFlagBit(int flag) => this.status.bitFlag = flag;
        public int GetEffectiveFlagBit() => this.effectiveFlag;
        public void SetEffectiveFlagBit(int flag) => this.effectiveFlag = flag;

        // Start is called before the first frame update
        void Start() {
            SetEffectOnDamageWithFlag();
            UpdateEffectiveFlagFromSelfFlag();
        }
        void OnValidate() {
            if (this.status == null) this.status = this.GetComponent<EntityStatus>();
            UpdateEffectiveFlagFromSelfFlag();
        }
        
        private void SetEffectOnDamageWithFlag() {
            this.status.ClearEffectOnDamageWithFlag();
            foreach (int bit in GetFlagValues()) {
                if (!FlagIsOn(GetEffectiveFlagBit(), bit)) continue;
                F flag = (F)Enum.ToObject(typeof(F), bit);
                var func = GetEffectOnDamageWithFlag(flag);
                if (func != null) this.status.AddEffectOnDamageWithFlag(bit, func);
            }
        }
        private void UpdateEffectiveFlagFromSelfFlag() {
            F _effectiveFlag = (F)Enum.ToObject(typeof(F), GetEffectiveFlagBit());
            F _selfFlag = (F)Enum.ToObject(typeof(F), GetSelfFlagBit());
            UpdateEffectiveFlagFromSelfFlag(_selfFlag, ref _effectiveFlag);
            SetEffectiveFlagBit((int)(object)_effectiveFlag);
        }
        public void OnUpdateEditor() {
            UpdateEffectiveFlagFromSelfFlag();
        }
        /// <summary>
        /// Func(self, other, currentDamage) { return damage; }
        /// </summary>
        /// <param name="effectiveFlag"></param>
        /// <returns></returns>
        protected abstract Func<EntityStatus, EntityStatus, float, float> GetEffectOnDamageWithFlag(F effectiveFlag);
        //protected abstract void UpdateSelfFlag(ref );
        protected abstract void UpdateEffectiveFlagFromSelfFlag(F selfFlag, ref F effectiveFlag);
        protected static bool FlagIsOn(int bit, int flag) => (bit & flag) == flag;
        protected static bool FlagIsOn(F bit, F flag) => FlagIsOn((int)(object)bit, (int)(object)flag);
    }

#if UNITY_EDITOR
    //[CustomEditor(typeof(EntityStatusKeyController<F>))]
    public class BaseEntityStatusKeyControllerEditor<T> : Editor where T : class, IEntityStatusKeyController {
        protected bool openSelfFlag = false;
        protected bool openEffectiveFlag = false;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            var _target = target as T;
            string[] flagNames = _target.GetFlagNames();
            Array flagValues = _target.GetFlagValues();
            this.openSelfFlag = EditorGUILayout.Foldout(this.openSelfFlag, "Self Flag");
            if (this.openSelfFlag) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < flagNames.Length; i++) {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(flagNames[i]);
                    int bitFlag = _target.GetSelfFlagBit();
                    int bit = (int)flagValues.GetValue(i);
                    bool isOn = (bitFlag & bit) == bit;
                    isOn = EditorGUILayout.Toggle(isOn);
                    if (isOn) _target.SetSelfFlagBit(bitFlag | bit);
                    else _target.SetSelfFlagBit(bitFlag & ~bit);
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            this.openEffectiveFlag = EditorGUILayout.Foldout(this.openEffectiveFlag, "Effective Flag");
            if (this.openEffectiveFlag) {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Effective Flag");
                for (int i = 0; i < flagNames.Length; i++) {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(flagNames[i]);
                    int bitFlag = _target.GetEffectiveFlagBit();
                    int bit = (int)flagValues.GetValue(i);
                    bool isOn = (bitFlag & bit) == bit;
                    isOn = EditorGUILayout.Toggle(isOn);
                    if (isOn) _target.SetEffectiveFlagBit(bitFlag | bit);
                    else _target.SetEffectiveFlagBit(bitFlag & ~bit);
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck()) {
                _target.OnUpdateEditor();
            }
        }
    }
#endif
}