# EntityStatus

GameObjectにHPと攻撃力を付与し、それらのステータスを管理する。
Entity同士が接触した時に自動的に相互にダメージを与える。

<!--# DEMO

-->


# Requirement

* UnityEngine
* System

# Usage

① EntityStatus.cs を任意のGameObjectにコンポーネントして、Tagに「Entity」を追加\
② RigidBodyとColliderをコンポーネント\
③ EntityStatusのパラメータを調整

※「Entity」タグが追加されていれば、Start()時に自動的にタグが変更されます。\
※ RigidBodyのisTriggerはtrueでもfalseでも問題なく動作します。

# Contains

## Inspector

--

## Public Variable
```
bool statusActive
DamageType damageType
EntityType EntityType_ { get; }
float MaxHP { get; }
float HP { get; }
float Power { get; }
bool Defeated { get; }
```
## public Function
```
EntityStatus Set(float? hp, float? power, EntityType? entityType, bool? isShot, DamageType? damageType)
EntityStatus SetDamagedAction(System.Action action, bool reset = false)
EntityStatus SetDefeatedAction(System.Action action, bool reset = false)
EntityStatus SetRecoverdAction(System.Action action, bool reset = false)
void Damage(float value, int key = -1)
void Recover(float value)
void RecoverFull(bool recoverAction = true)
void Defeat()
```

## protected Variable
```
EntityType entityType
bool isShot
float maxHP
float hp
float power
```

## virtual Funtion
```
void Awake()
void OnEnable()
void OnValidate()
void OnTriggerEnter(Collider other)
void OnCollisionEnter(Collision collision)
void Damage(float value, int key = -1)
void Recover(float value)
void RecoverFull(bool recoverAction = true)
void Defeat()
```

## Enum
```
EntityType { enemy, friend, obstacle }
DefeatType { destroy, nonActive, none }
```

# Note

注意点などがあれば書く

# License

"EntityStatus" is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).
