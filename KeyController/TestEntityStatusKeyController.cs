using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EntityBehavior.Status {
    public static class TestFlags {
        public enum Flags1 {
            none = 0x000,
            Black = 0x001,  //   1
            Red = 0x002,  //   2
            Green = 0x004,  //   4
            Blue = 0x008,  //   8
            Orange = 0x010,  //  16
            Pink = 0x020,  //  32
            Purple = 0x040,  //  64
            White = 0x080   // 128
        }
    }

    public class TestEntityStatusKeyController : BaseEntityStatusKeyController<TestFlags.Flags1> {
        protected override Func<EntityStatus, EntityStatus, float, float> GetEffectOnDamageWithFlag(TestFlags.Flags1 effectiveFlag) {
            void PrintDamageInfo(EntityStatus self, EntityStatus other, float damage) {
                Debug.Log($"{self.name} was Damaged from {other.name}({effectiveFlag}) val:{damage}");
            }
            return effectiveFlag switch {
                TestFlags.Flags1.Black => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.Red => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.Green => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.Blue => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.Orange => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.Pink => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.Purple => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                TestFlags.Flags1.White => (s, o, d) => {
                    PrintDamageInfo(s, o, d);
                    return 2 * d;
                },
                _ => null,
            };
        }
        protected override void UpdateEffectiveFlagFromSelfFlag(TestFlags.Flags1 selfFlag, ref TestFlags.Flags1 effectiveFlag) {
            if (FlagIsOn(selfFlag, TestFlags.Flags1.Black)) {
                effectiveFlag |= TestFlags.Flags1.White;
                effectiveFlag &= ~TestFlags.Flags1.Black;
            }
            if (FlagIsOn(selfFlag, TestFlags.Flags1.White)) {
                effectiveFlag |= TestFlags.Flags1.Black;
                effectiveFlag &= ~TestFlags.Flags1.White;
            }
        }
    }

    [CustomEditor(typeof(TestEntityStatusKeyController))]
    class EntityStatusKeyControllerEditor : BaseEntityStatusKeyControllerEditor<TestEntityStatusKeyController> {
    }

    
}