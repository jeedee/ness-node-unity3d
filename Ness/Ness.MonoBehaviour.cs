using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ness
{
	public class MonoBehaviour : UnityEngine.MonoBehaviour {

		bool _isOwner = false;
		public bool isOwner
		{
			get
			{
				return _isOwner;
			}
			
			set
			{
				// If this entity is now owned by the local player, run a callback
				if (value)
					NessBecameOwner();
				
				_isOwner = value;
			}
		}

		public NessView nessView
		{
			get
			{
				return this.GetComponent<NessView>();
			}
		}
		
		public virtual void NessBecameOwner ()
		{
			
		}

		public virtual void NessUpdate (Dictionary<string, object> data)
		{
			
		}

		void OnDestroy ()
		{
			Ness.Engine.Instance.UnregisterView(this.nessView);
		}
	}
}
