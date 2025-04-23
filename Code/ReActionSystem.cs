using UnityEngine;

namespace ReAction
{
#if SANDBOX
	public class ReActionSystem : GameObjectSystem<ReActionSystem>
	{
		public ReActionSystem(Scene scene) : base(scene)
		{
			ReAction.Init();
			Listen(Stage.StartUpdate, -10, ReAction.QueryInput, "ReActionQueryInput");
			Listen(Stage.StartFixedUpdate, -10, ReAction.QueryInput, "ReActionQueryInputFixedUpdate"); //surely this makes querying inputs in fixed updates consistent, right? idk
		}
	}
#elif UNITY_EDITOR || UNITY_STANDALONE
	/// <summary>
	/// This is the MonoBehaviour that handles calling <see cref="ReAction.Init"/> and <see cref="ReAction.QueryInput"/>, you'll want this somewhere in your scene, or just call those methods yourself in your component of choice, its has a <see cref="DefaultExecutionOrder"/> with the order set to -100, you can override this in your project settings though
	/// </summary>
	[DefaultExecutionOrder(-100)]
	public class ReActionSystem : MonoBehaviour
	{
		public static ReActionSystem Current
		{
			get;
			private set;
		}

		private void Awake()
		{
			Current = this;
			ReAction.Init();

			DontDestroyOnLoad(this);
		}

		private void FixedUpdate()
		{
			ReAction.QueryInput();
		}

		private void Update()
		{
			ReAction.QueryInput();
		}
	}
#endif
}