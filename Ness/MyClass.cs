using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using WebSocketSharp;

namespace Ness
{	
	public class Engine : MonoBehaviour {
		public static Engine Instance;

		// Socket
		WebSocket socket;
		public string url;

		// Reflection binding flags
		BindingFlags reflectionBinding = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

		// Network prefabs
		public GameObject[] NetworkPrefabs;

		// Task executor
		TaskExecutor tasker;

		// Active views
		Dictionary<string, NessView> views = new Dictionary<string, NessView>();

		void Awake () {
			Application.runInBackground = true;
			Ness.Engine.Instance = this;

			views = new Dictionary<string, NessView>();
		}

		void Start () {
			// Creates the socket
			socket = new WebSocket(url);

			// Setup the task executor
			tasker = new TaskExecutor();

			// Bind events
			socket.OnOpen += onConnect;
			socket.OnMessage += onMessage;
			socket.OnClose += onClose ;
		}

		void Update () {
			tasker.Update();
		}

		// Connect to server
		public void Connect () {
			socket.Connect();
		}

		public GameObject PrefabWithName (string name)
		{
			foreach (GameObject g in NetworkPrefabs)
			{
				if (g.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase))
					return g;
			}

			return null;
		}

		#region Views
		public void RegisterView (NessView view) {
			Debug.Log ("View is registering with id " + view.viewID);
			views.Add(view.viewID, view);
		}

		public void UnregisterView (NessView view) {
			views.Remove(view.viewID);
		}
		#endregion

		#region CRUDE
		void Create (Dictionary<string, object> data)
		{
			tasker.ScheduleTask(new Task(delegate {
				string id = data["id"].ToString();
				GameObject target = GameObject.Find(id);

				if (target)
				{
					Debug.LogWarning("Received create for an existing entity!");
				}else{
					// Instantiate entity
					string entityType = data["type"].ToString();
					target = (GameObject)Instantiate(PrefabWithName(entityType));
					target.name = id;

					if (target.GetComponent<NessView>() != null)
					{
						target.GetComponent<NessView>()._viewID = id;
					}else{
						Debug.LogError("The entity about to be instantiated does not have a NessView!");
					}

					// Send read request to update
					update (data);
				}

			}));
		}

		void update (Dictionary<string, object> data)
		{
			tasker.ScheduleTask(new Task(delegate {
				if (views.ContainsKey(data["id"].ToString()))
				{
					NessView view = views[data["id"].ToString()];
					view.NessUpdate(data);
				}else{
					Debug.LogWarning("Trying to set values on a non-existing entity");
				}


			}));
		}

		void Delete (Dictionary<string, object> data)
		{
			tasker.ScheduleTask(new Task(delegate {
				if (views.ContainsKey(data["id"].ToString()))
				{
					NessView view = views[data["id"].ToString()];
					{
						view.NessRemove();
					}
				}else{
					Debug.LogWarning("No matching view ID could be found for delete (" + data["id"] + ")");
				}			
			}));
		}

		void Execute (Dictionary<string, object> data)
		{
			tasker.ScheduleTask(new Task(delegate {
				if (views.ContainsKey(data["id"].ToString()))
				{
					NessView view = views[data["id"].ToString()];

					// Unserialize method and parameter
					string method = data["method"].ToString();
					List<object> parameters = (List<object>)data["parameters"];

					foreach (Ness.MonoBehaviour behaviour in view.GetComponents<Ness.MonoBehaviour>()) {
						if (behaviour.GetType().GetMethod(method, reflectionBinding) != null)
						{
							MethodInfo methodInfo = behaviour.GetType().GetMethod(method, reflectionBinding);
							foreach (object p in parameters)
							{
								Debug.Log (p.GetType());
							}
							// If has the executable attribute
							if (methodInfo.GetCustomAttributes (typeof (Ness.NessExecutable), true).Any())
							{
								methodInfo.Invoke(behaviour, parameters.ToArray());
							}else{
								Debug.LogWarning("Found a method but it is not flagged [Ness.Executable]");
							}
						}
					}
				}else{
					Debug.LogWarning("Trying to execute an non-existing entity (" + data["id"].ToString() + ")");
				}
			}));
		}

		public void RemoteExecute (string method, params object[] args)
		{
			Dictionary<string, object> request = new Dictionary<string, object>();
			Dictionary<string, object> requestData = new Dictionary<string, object>();

			// Op code
			request.Add("op", Ness.Op.EXECUTE);

			// Create the data dict
			requestData.Add("method", method);
			requestData.Add("parameters", args);

			// Add it to the request
			request.Add("data", requestData);

			socket.Send(request.toJson());
		}

		#endregion

		#region Socket events
		void onConnect (object sender, System.EventArgs e)
		{
			Debug.Log ("Connected!");
		}

		void onMessage (object sender, MessageEventArgs e)
		{
			Debug.Log (e.Data);
			Dictionary<string, object> message = MiniJsonExtensions.dictionaryFromJson(e.Data);

			// Operation & Data
			string operation = message["op"].ToString();		
			Dictionary<string, object> data = (Dictionary<string, object>)message["data"];

			Debug.Log ("[NESS] operation is '" + operation + "'");
			// Debug stuff

			//Debug.Log (e.Data);
			// Parse JSON

			/*			
			foreach(KeyValuePair<string, object> entry in messageData)
			{
				Debug.Log (entry.Key);
				Debug.Log (entry.Value);
			}
			*/
			//----

			switch (operation[0])
			{
			// Create
			case Ness.Op.CREATE:
				Create(data);
				break;
				// Read
			case Ness.Op.UPDATE:
				update(data);
				break;
				// Update
				// --
				// Delete
			case Ness.Op.DELETE:
				Delete (data);
				break;
				// Execute
			case Ness.Op.EXECUTE:
				Execute(data);
				break;
			}
		}

		void onClose (object sender, CloseEventArgs e) {
			Debug.LogError("Connection closed!");
		}
		#endregion

	}
}
