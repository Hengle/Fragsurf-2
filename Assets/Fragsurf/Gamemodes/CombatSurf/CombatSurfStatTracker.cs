using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class CombatSurfStatTracker : FSSharedScript
    {

        private PlayerProps _props;
        private const string _killsLabel = "Kills";
        private const string _deathsLabel = "Deaths";
        private const string _damageLabel = "Damage";

        protected override void _Start()
        {
            _props = Game.Get<PlayerProps>();

            var rm = Game.Get<RoundManager>();
            rm.OnMatchStart += Rm_OnMatchStart;
        }

        protected override void _Destroy()
        {
            base._Destroy();

            var rm = Game.Get<RoundManager>();
            if(rm) rm.OnMatchStart -= Rm_OnMatchStart;
        }

        private void Rm_OnMatchStart()
        {
            if (!Game.IsHost)
            {
                return;
            }

            foreach(var player in Game.PlayerManager.Players)
            {
                _props.SetProp(player.ClientIndex, _killsLabel, 0);
                _props.SetProp(player.ClientIndex, _deathsLabel, 0);
                _props.SetProp(player.ClientIndex, _damageLabel, 0);
            }
        }

        public int GetKills(int clientIndex) => (int)_props.GetProp(clientIndex, _killsLabel);
        public int GetDeaths(int clientIndex) => (int)_props.GetProp(clientIndex, _deathsLabel);
        public int GetDamage(int clientIndex) => (int)_props.GetProp(clientIndex, _damageLabel);

        protected override void OnHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            if (Game.IsHost)
            {
                var killer = Game.EntityManager.FindEntity<Human>(dmgInfo.AttackerEntityId);

                // Attacker
                if(killer != null)
                {
                    if (dmgInfo.ResultedInDeath)
                    {
                        _props.IncrementProp(killer.OwnerId, _killsLabel, 1);
                    }
                    _props.IncrementProp(killer.OwnerId, _damageLabel, dmgInfo.Amount);
                }

                // Victim
                if (dmgInfo.ResultedInDeath)
                {
                    _props.IncrementProp(hu.OwnerId, _deathsLabel, 1);
                }
            }
        }

    }
}

