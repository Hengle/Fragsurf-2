using System;
using SourceUtils.ValveBsp.Entities;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.Entity;
using Fragsurf.Server;
using System.Linq;

namespace Fragsurf.BSP
{
	public class BspEntityMonoBehaviour : MonoBehaviour, IHasNetProps, IDamageable
	{
		public Entity Entity;
		public BspToUnity BspToUnity;
		public bool EntityEnabled { get; private set; } = true;
		public int UniqueId { get; set; }
		public bool HasAuthority => GameServer.Instance != null;

        public bool Dead => false;

        private List<BspEntityOutput> _pendingOutputs = new List<BspEntityOutput>();

		public IEnumerable<BspEntityMonoBehaviour> FindBspEntities(string targetName)
        {
			foreach(var entity in BspToUnity.Entities)
            {
				if(string.Equals(entity.Key.TargetName, targetName, StringComparison.OrdinalIgnoreCase))
                {
					yield return entity.Value;
                }
            }
        }

        private void Start()
        {
			EntityEnabled = Entity.StartDisabled == 0;

            if (!string.IsNullOrWhiteSpace(Entity.ParentName))
            {
				var parent = FindBspEntities(Entity.ParentName).FirstOrDefault();
				if(parent != null)
                {
					transform.SetParent(parent.transform);
                }
            }
			
			OnStart();
        }

        private void Update()
        {
			for(int i = _pendingOutputs.Count - 1; i >= 0; i--)
            {
				_pendingOutputs[i].Delay -= Time.deltaTime;
				if(_pendingOutputs[i].Delay <= 0)
                {
					Input(_pendingOutputs[i]);
					_pendingOutputs.RemoveAt(i);
                }
            }
			OnUpdate();
        }

		protected virtual void OnStart() { }
		protected virtual void OnUpdate() { }

		public void Input(BspEntityOutput output)
        {
			if(output.Delay > 0)
            {
				_pendingOutputs.Add(output);
				return;
			}

            switch (output.TargetInput.ToLower())
            {
				case "enable":
					EntityEnabled = true;
					break;
				case "disable":
					EntityEnabled = false;
					break;
            }

			_Input(output);
        }

		protected void Fire(string outputName)
		{
			foreach (var prop in Entity.PropertyNames)
			{
				if (prop.StartsWith(outputName, StringComparison.OrdinalIgnoreCase))
				{
					var output = BspEntityOutput.Parse(outputName, Entity.GetRawPropertyValue(prop));
					foreach(var ent in FindBspEntities(output.TargetEntity))
                    {
						ent.Input(output);
					}
				}
			}
		}

		protected virtual void _Input(BspEntityOutput output)
        {

        }

        public virtual void Damage(DamageInfo dmgInfo)
        {
			Fire("OnDamaged");
		}
    }

	public class GenericBspEntityMonoBehaviour<T> : BspEntityMonoBehaviour
		where T : Entity
	{
		public new T Entity => base.Entity as T;
	}
}