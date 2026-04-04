using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Weapon data and default combat timing for the demo's melee loadouts.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Ashenveil/Combat/Weapon Data")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Base Properties")]
        [SerializeField] private string _displayName = "Runtime Weapon";
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(0f)] private float _baseDamage = 15f;
        [SerializeField, Min(0.01f)] private float _attackSpeed = 1f;
        [SerializeField, Min(0f)] private float _range = 1.5f;
        [SerializeField] private WeaponType _weaponType = WeaponType.Sword;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private AudioClip _swingSound;
        [SerializeField] private AudioClip _hitSound;

        [Header("Combat Timing")]
        [SerializeField] private AttackStepDefinition[] _lightAttackCombo = new AttackStepDefinition[3];
        [SerializeField] private AttackStepDefinition _heavyAttack = AttackStepDefinition.CreateHeavyAttackStep();
        [SerializeField] private BlockSettings _blockSettings = BlockSettings.CreateDefault();
        [SerializeField] private DodgeSettings _dodgeSettings = DodgeSettings.CreateDefault();

        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float BaseDamage => _baseDamage;
        public float AttackSpeed => _attackSpeed;
        public float Range => _range;
        public WeaponType Type => _weaponType;
        public GameObject Prefab => _prefab;
        public AudioClip SwingSound => _swingSound;
        public AudioClip HitSound => _hitSound;
        public IReadOnlyList<AttackStepDefinition> LightAttackCombo => _lightAttackCombo;
        public AttackStepDefinition HeavyAttack => _heavyAttack;
        public BlockSettings BlockSettings => _blockSettings;
        public DodgeSettings DodgeSettings => _dodgeSettings;

        private void OnEnable()
        {
            EnsureDefaults();
        }

        private void OnValidate()
        {
            EnsureDefaults();
        }

        public static WeaponData CreateRuntimeDefaults()
        {
            WeaponData weaponData = CreateInstance<WeaponData>();
            weaponData.hideFlags = HideFlags.HideAndDontSave;
            weaponData.EnsureDefaults();
            return weaponData;
        }

        public AttackStepDefinition GetLightAttackStep(int index)
        {
            EnsureDefaults();

            if (_lightAttackCombo == null || _lightAttackCombo.Length == 0)
            {
                return AttackStepDefinition.CreateLightAttackStep(0);
            }

            int clampedIndex = Mathf.Clamp(index, 0, _lightAttackCombo.Length - 1);
            AttackStepDefinition attackStep = _lightAttackCombo[clampedIndex];
            return attackStep != null ? attackStep : AttackStepDefinition.CreateLightAttackStep(clampedIndex);
        }

        private void EnsureDefaults()
        {
            if (_lightAttackCombo == null || _lightAttackCombo.Length != 3)
            {
                Array.Resize(ref _lightAttackCombo, 3);
            }

            for (int index = 0; index < _lightAttackCombo.Length; index++)
            {
                if (_lightAttackCombo[index] == null)
                {
                    _lightAttackCombo[index] = AttackStepDefinition.CreateLightAttackStep(index);
                }
            }

            if (_heavyAttack == null)
            {
                _heavyAttack = AttackStepDefinition.CreateHeavyAttackStep();
            }

            if (_blockSettings == null)
            {
                _blockSettings = BlockSettings.CreateDefault();
            }

            if (_dodgeSettings == null)
            {
                _dodgeSettings = DodgeSettings.CreateDefault();
            }
        }
    }
}
