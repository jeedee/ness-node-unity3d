using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Ness
{
	public static class Helper
	{
		// This will sync received property with the target
		public static void SyncAll (Dictionary<string, object> data, object target, string[] skip = null)
		{
			foreach (KeyValuePair<string, object> property in data)
			{
				if (skip != null && skip.Contains(property.Key))
					continue;
				
				var prop = target.GetType().GetProperty(property.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (prop != null)
				{
					try
					{
						var value = System.Convert.ChangeType(property.Value, prop.PropertyType);
						prop.SetValue(target, value, null);
					}catch{
						Debug.LogWarning("Could not convert " + prop.Name);
					}
				}else{
					Debug.LogWarning("The target object does not have the property " + property.Key + ". Cannot set.");
				}
			}
		}
	}
}
