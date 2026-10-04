#if LITMOTION_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations;
using ParkMinDev.UPM.UGUI.Enums;

namespace ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "UILitMotionActiveAnimation")]
	public abstract class UILitMotionActiveAnimation : ActiveAnimation
	{
		public abstract MotionHandle CreateMotion(IMotionScheduler scheduler);
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			IMotionScheduler scheduler = UILitMotionAnimationUtility.GetScheduler(UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await CreateMotion(scheduler).ToUniTask(LitMotion.CancelBehavior.Cancel, cancellationToken);
		}
	}

	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "UILitMotionDeactivateAnimation")]
	public abstract class UILitMotionDeactivateAnimation : DeactivateAnimation
	{
		public abstract MotionHandle CreateMotion(IMotionScheduler scheduler);
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			IMotionScheduler scheduler = UILitMotionAnimationUtility.GetScheduler(UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await CreateMotion(scheduler).ToUniTask(LitMotion.CancelBehavior.Cancel, cancellationToken);
		}
	}

	static class UILitMotionAnimationUtility
	{
		public static IMotionScheduler GetScheduler(UIAnimationUpdateMode updateMode, bool ignoreTimeScale) {
			return updateMode switch
			{
				UIAnimationUpdateMode.Update => ignoreTimeScale ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update,
				UIAnimationUpdateMode.LateUpdate => ignoreTimeScale ? MotionScheduler.PreLateUpdateIgnoreTimeScale : MotionScheduler.PreLateUpdate,
				_ => throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null)
			};
		}
	}
}
#endif
