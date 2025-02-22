using Fragsurf.DataEditor;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    [DataEditor]
    [CreateAssetMenu(fileName = "Gun Equippable", menuName = "Fragsurf/Gun Equippable", order = 1)]
    public class GunEquippableData : BaseEquippableData
    {
        [Header("Audio")]
        public AudioClip FireSound;
        public AudioClip FireTailSound;
        public AudioClip ReloadSound;
        public AudioClip BoltActionSound;
        public AudioClip ScopeSound;
        public AudioClip DryFireSound;
        public float FireMaxDistance = 100f;
        public AnimationCurve FireRolloff = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0, 1.0f),
            new Keyframe(.02f, 1.0f),
            new Keyframe(.13f, .807f, -2.8657f, -2.8657f),
            new Keyframe(1f, 0),
        });
        public AnimationCurve FolleyRolloff = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0, 1.0f),
            new Keyframe(0.15f, 0.64f, -2f, -2f),
            new Keyframe(1, 0f)
        });
        [Header("Ammo")]
        public int RoundsPerClip = 30;
        public int MaxClips = 3;
        public float BulletRadius = .05f;
        public float BulletRange = 512;
        [Header("Firing")]
        public GunFiringMode FiringMode = GunFiringMode.SingleShot;
        public Vector3 AimPunchMin = new Vector3(-.1f, -1, -1);
        public Vector3 AimPunchMax = new Vector3(-1f, 1, 1);
        public int RoundsPerMinute = 400;
        public float ReloadTime = 4f;
        public float BoltActionTime = .65f;
        public float BoltActionDelay = .3f;
        public bool BoltActionAfterReload = false;
        public float ViewModelKickStrength = 1f;
        public bool FiringInterruptsReload = false;
        [Header("Scope")]
        public float[] ZoomLevels;
        public Texture2D ScopeTexture;
        public bool DisableCrosshair;
        [Header("Damage")]
        public int BaseDamage = 20;
        public List<AreaDamageDefinition> DamagePercents = new List<AreaDamageDefinition>()
        {
            new AreaDamageDefinition(HitboxArea.Head, 500),
            new AreaDamageDefinition(HitboxArea.Neck, 300),
            new AreaDamageDefinition(HitboxArea.UpperChest, 100),
            new AreaDamageDefinition(HitboxArea.LowerChest, 75),
            new AreaDamageDefinition(HitboxArea.UpperArm, 50),
            new AreaDamageDefinition(HitboxArea.LowerArm, 50),
            new AreaDamageDefinition(HitboxArea.Hand, 35),
            new AreaDamageDefinition(HitboxArea.UpperLeg, 65),
            new AreaDamageDefinition(HitboxArea.LowerLeg, 55),
            new AreaDamageDefinition(HitboxArea.Foot, 30)
        };

        public override ImpactType ImpactType => ImpactType.Bullet;
        public override Type ComponentType => typeof(GunEquippable);

        public float GetDamageMultiplier(HitboxArea hitbox)
        {
            foreach(var hb in DamagePercents)
            {
                if(hb.Area == hitbox)
                {
                    return hb.DamagePercent / 100f;
                }
            }
            return 1f;
        }
    }

    public enum GunFiringMode
    {
        SingleShot,
        BurstFire,
        FullyAutomatic,
        BoltAction
    }

}

