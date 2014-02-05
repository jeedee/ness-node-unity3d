using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NessView : MonoBehaviour {
	// TODO: Build custom editor to be able to assign private var
	public string _viewID = "0";

	public string viewID
	{
		get
		{
			return _viewID;
		}

		set
		{
			// Unregister, set the new ID then register again
			UnregisterView();

			_viewID = value;

			// Register again
			RegisterView();
		}
	}

	void Start ()
	{
		if (viewID != "0")
			RegisterView();
	}

	public void RegisterView ()
	{
		Ness.Engine.Instance.RegisterView(this);
	}

	public void UnregisterView ()
	{
		Ness.Engine.Instance.UnregisterView(this);
	}

	public void NessExecute (string method, params object[] args)
	{
		Ness.Engine.Instance.RemoteExecute(method, args);
	}

	public virtual void NessUpdate (Dictionary<string, object> data)
	{
		foreach (Ness.MonoBehaviour behaviour in this.gameObject.GetComponents<Ness.MonoBehaviour>())
		{
			behaviour.NessUpdate(data);
		}
	}
	
	public virtual void NessRemove ()
	{
		Destroy (this.gameObject);
	}
}
