#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations;
using ParkMinDev.UPM.UGUI.Enums;

namespace ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "UIDOTweenActiveAnimation")]
	public abstract class UIDOTweenActiveAnimation : ActiveAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			Tween tween = UIDOTweenAnimationUtility.ApplyUpdateSettings(CreateTween(), UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await tween.SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "UIDOTweenDeactivateAnimation")]
	public abstract class UIDOTweenDeactivateAnimation : DeactivateAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			Tween tween = UIDOTweenAnimationUtility.ApplyUpdateSettings(CreateTween(), UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await tween.SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}

	static class UIDOTweenAnimationUtility
	{
		public static Tween ApplyUpdateSettings(Tween tween, UIAnimationUpdateMode updateMode, bool ignoreTimeScale) {
			UpdateType updateType = updateMode switch
			{
				UIAnimationUpdateMode.Update => UpdateType.Normal,
				UIAnimationUpdateMode.LateUpdate => UpdateType.Late,
				_ => throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null)
			};
			return tween.SetUpdate(updateType, ignoreTimeScale);
		}
	}
}
#endif
