using System.Threading;
using Cysharp.Threading.Tasks;
using ParkMinDev.UPM.Foundation.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "UIAnimation")]
	[RequireComponent(typeof(UIActivator))]
	public abstract class UIAnimation : ExtendedBehaviour
	{
		internal void Capture() {
			CaptureValues();
		}
		internal void Restore() {
			RestoreCapturedValues();
		}
		public abstract UniTask ExecuteAsync(CancellationToken cancellationToken = default);

		public UIActivator UIActivator { get; private set; }

		public RectTransform Target
		{
			get { return _target; }
			set { _target = value; }
		}

		protected virtual void Awake() {
			UIActivator = GetComponent<UIActivator>();
		}
		protected virtual void Reset() {
			Target = GetComponent<RectTransform>();
		}
		protected abstract void CaptureValues();
		protected abstract void RestoreCapturedValues();

		[FormerlySerializedAs("target")]
		[SerializeField] RectTransform _target;
	}

	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "ActiveAnimation")]
	public abstract class ActiveAnimation : UIAnimation
	{
		protected override void OnEnable() {
			base.OnEnable();
			UIActivator.RegisterActiveAnimation(this);
		}
		protected override void OnDisable() {
			base.OnDisable();
			UIActivator.UnregisterActiveAnimation(this);
		}
	}
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "DeactivateAnimation")]
	public abstract class DeactivateAnimation : UIAnimation
	{
		protected override void OnEnable() {
			base.OnEnable();
			UIActivator.RegisterDeactivateAnimation(this);
		}
		protected override void OnDisable() {
			base.OnDisable();
			UIActivator.UnregisterDeactivateAnimation(this);
		}
	}
}
