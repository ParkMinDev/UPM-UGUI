using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ParkMinDev.UPM.Foundation.Components;
using ParkMinDev.UPM.Foundation.Extensions;
using ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations;
using ParkMinDev.UPM.UGUI.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DOTweenAnimations = ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations.DOTweens;
#endif
#if LITMOTION_SUPPORT
using LitMotionAnimations = ParkMinDev.UPM.UGUI.Components.UIActivatorAnimations.LitMotions;
#endif

namespace ParkMinDev.UPM.UGUI.Components
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ParkMinPackages.UGUI.Components", sourceAssembly: "ParkMinPackages.UGUI", sourceClassName: "UIActivator")]
	[RequireComponent(typeof(CanvasGroup))]
	[DefaultExecutionOrder(-100)]
	[DisallowMultipleComponent]
	public class UIActivator : TreeNode<UIActivator>
	{
		// ===================== Public API =====================

		public async UniTask ActiveAsync(
			CancellationToken cancellationToken,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete,
			bool throwIfTransitioning = true
		) {
			cancellationToken.ThrowIfCancellationRequested();
			if (_isTransitioning) {
				if (throwIfTransitioning) {
					throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
				}
				return;
			}
			if (_state == UIActivationState.Active) return;

			SetLayoutIgnored(false);
			ActiveAnimation[] activeAnimations = _activeAnimations.ToArray();
			_isTransitioning = true;
			UpdateRaycastable();

			try {
				Render = true;
				Capture(activeAnimations);
				UniTask[] animationTasks = ExecuteAnimations(activeAnimations, cancellationToken);
				await UniTask.WhenAll(animationTasks);
				cancellationToken.ThrowIfCancellationRequested();
				Restore(activeAnimations);
				_state = UIActivationState.Active;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
				ApplyActiveCancellation(activeAnimations, animationCancelBehaviour);
				throw;
			}
			catch {
				Restore(activeAnimations);
				Render = false;
				throw;
			}
			finally {
				_isTransitioning = false;
				SetLayoutIgnored(_state == UIActivationState.Inactive);
				UpdateRaycastable();
			}
		}
		public async UniTask DeactivateAsync(
			CancellationToken cancellationToken,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete,
			bool throwIfTransitioning = true
		) {
			cancellationToken.ThrowIfCancellationRequested();
			if (_isTransitioning) {
				if (throwIfTransitioning) {
					throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
				}
				return;
			}
			if (_state == UIActivationState.Inactive) return;

			DeactivateAnimation[] deactivateAnimations = _deactivateAnimations.ToArray();
			Capture(deactivateAnimations);
			_isTransitioning = true;
			UpdateRaycastable();

			try {
				Render = true;
				UniTask[] animationTasks = ExecuteAnimations(deactivateAnimations, cancellationToken);
				await UniTask.WhenAll(animationTasks);
				cancellationToken.ThrowIfCancellationRequested();
				Restore(deactivateAnimations);
				Render = false;
				_state = UIActivationState.Inactive;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
				ApplyDeactivateCancellation(deactivateAnimations, animationCancelBehaviour);
				throw;
			}
			catch {
				Restore(deactivateAnimations);
				Render = true;
				throw;
			}
			finally {
				_isTransitioning = false;
				SetLayoutIgnored(_state == UIActivationState.Inactive);
				UpdateRaycastable();
			}
		}
		public void ActiveImmediate() {
			if (_isTransitioning) {
				throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
			}
			SetLayoutIgnored(false);
			Render = true;
			_state = UIActivationState.Active;
			UpdateRaycastable();
		}
		public void DeactivateImmediate() {
			if (_isTransitioning) {
				throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
			}
			Render = false;
			_state = UIActivationState.Inactive;
			SetLayoutIgnored(true);
			UpdateRaycastable();
		}

		public async UniTask ActiveWithChildrenAsync(
			CancellationToken cancellationToken,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete
		) {
			cancellationToken.ThrowIfCancellationRequested();
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			UniTask[] tasks = new UniTask[uiActivators.Length];

			for (int i = 0; i < uiActivators.Length; i++) {
				tasks[i] = uiActivators[i].ActiveAsync(cancellationToken, animationCancelBehaviour);
			}
			await UniTask.WhenAll(tasks);
		}
		public async UniTask DeactivateWithChildrenAsync(
			CancellationToken cancellationToken,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete
		) {
			cancellationToken.ThrowIfCancellationRequested();
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			UniTask[] tasks = new UniTask[uiActivators.Length];

			for (int i = 0; i < uiActivators.Length; i++) {
				tasks[i] = uiActivators[i].DeactivateAsync(cancellationToken, animationCancelBehaviour);
			}
			await UniTask.WhenAll(tasks);
		}
		public void ActiveWithChildrenImmediate() {
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			for (int i = 0; i < uiActivators.Length; i++) {
				uiActivators[i].ActiveImmediate();
			}
		}
		public void DeactivateWithChildrenImmediate() {
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			for (int i = 0; i < uiActivators.Length; i++) {
				uiActivators[i].DeactivateImmediate();
			}
		}

		internal void RegisterActiveAnimation(ActiveAnimation activeAnimation) {
			_activeAnimations.Add(activeAnimation);
		}
		internal void UnregisterActiveAnimation(ActiveAnimation activeAnimation) {
			_activeAnimations.Remove(activeAnimation);
		}
		internal void RegisterDeactivateAnimation(DeactivateAnimation deactivateAnimation) {
			_deactivateAnimations.Add(deactivateAnimation);
		}
		internal void UnregisterDeactivateAnimation(DeactivateAnimation deactivateAnimation) {
			_deactivateAnimations.Remove(deactivateAnimation);
		}

		// ===================== Public Property =====================

		public bool Render
		{
			get { return _render; }
			private set
			{
				_render = value;

				if (Canvas != null) {
					Canvas.enabled = _render;
				}

				UpdateVisibleAndFade();
				UpdateRaycastable();
				UpdateInteractable();
			}
		}
		public bool Visible
		{
			get { return _visible; }
			set
			{
				_visible = value;
				UpdateVisibleAndFade();
			}
		}
		public bool Raycastable
		{
			get { return _raycastable; }
			set
			{
				_raycastable = value;
				UpdateRaycastable();
			}
		}
		public bool Interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				UpdateInteractable();
			}
		}
		public float Fade
		{
			get { return _fade; }
			set
			{
				_fade = Mathf.Clamp(value, 0.0f, 1.0f);
				UpdateVisibleAndFade();
			}
		}
		public bool DisableRaycastWhileAnimation
		{
			get { return _disableRaycastWhileAnimation; }
			set
			{
				_disableRaycastWhileAnimation = value;
				UpdateRaycastable();
			}
		}
		public UIAnimationUpdateMode UpdateMode
		{
			get { return _updateMode; }
			set { _updateMode = value; }
		}
		public bool IgnoreTimeScale
		{
			get { return _ignoreTimeScale; }
			set { _ignoreTimeScale = value; }
		}

		public UIActivationState State
		{
			get { return _state; }
		}
		public bool IsActive
		{
			get { return _state == UIActivationState.Active; }
		}
		public bool IsTransitioning
		{
			get { return _isTransitioning; }
		}

		// - Components -
		public CanvasGroup CanvasGroup
		{
			get
			{
				if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
				return _canvasGroup;
			}
		}
		public Canvas Canvas
		{
			get
			{
				if (_canvas == null) _canvas = GetComponent<Canvas>();
				return _canvas;
			}
		}
		// ===================== Handlers =====================

		protected override void Awake() {
			base.Awake();
			
			CanvasGroup.ignoreParentGroups = false;
			
			if (_overrideStartAnchoredPosition) {
				((RectTransform)transform).anchoredPosition3D = _startAnchoredPosition;
			}

			Render = _startActiveState;
			_state = _startActiveState ? UIActivationState.Active : UIActivationState.Inactive;
			_layoutElement = GetComponent<LayoutElement>();
			SetLayoutIgnored(_state == UIActivationState.Inactive);
			_fade = Mathf.Clamp01(_fade);
			UpdateVisibleAndFade();
			UpdateInteractable();
			UpdateRaycastable();
		}
		// ===================== Internals =====================
		// - Components -
		CanvasGroup _canvasGroup;
		Canvas _canvas;
		LayoutElement _layoutElement;
		readonly List<ActiveAnimation> _activeAnimations = new List<ActiveAnimation>();
		readonly List<DeactivateAnimation> _deactivateAnimations = new List<DeactivateAnimation>();

		// - Core -
		[Title("Initial State")]
		[DisableInPlayMode, SerializeField] bool _startActiveState = true;
		[DisableInPlayMode, SerializeField, FormerlySerializedAs("_overrideStartLocalPosition")] bool _overrideStartAnchoredPosition;
		[DisableInPlayMode, ShowIf(nameof(_overrideStartAnchoredPosition)), SerializeField, FormerlySerializedAs("_startLocalPosition")] Vector3 _startAnchoredPosition;

		[Title("Visibility")]
		[OnValueChanged(nameof(UpdateVisibleAndFade)), SerializeField] bool _visible = true;
		[PropertyRange(0f, 1f), OnValueChanged(nameof(UpdateVisibleAndFade)), SerializeField] float _fade = 1f;

		[Title("Interaction")]
		[OnValueChanged(nameof(UpdateRaycastable)), SerializeField] bool _raycastable = true;
		[OnValueChanged(nameof(UpdateInteractable)), SerializeField] bool _interactable = true;
		[OnValueChanged(nameof(UpdateRaycastable)), SerializeField] bool _disableRaycastWhileAnimation = true;

		[Title("Layout")]
		[OnValueChanged(nameof(UpdateLayoutParticipation)), FormerlySerializedAs("_manageLayoutElement"), SerializeField] bool _excludeFromLayoutWhenInactive = true;

		[Title("Animation Update")]
		[SerializeField] UIAnimationUpdateMode _updateMode;
		[SerializeField] bool _ignoreTimeScale;

		bool _render;
		UIActivationState _state;
		bool _isTransitioning;

		static void Capture(UIAnimation[] animations) {
			for (int i = 0; i < animations.Length; i++) {
				animations[i].Capture();
			}
		}
		static void Restore(UIAnimation[] animations) {
			for (int i = 0; i < animations.Length; i++) {
				animations[i].Restore();
			}
		}
		static UniTask[] ExecuteAnimations(
			UIAnimation[] animations,
			CancellationToken cancellationToken
		) {
			UniTask[] animationTasks = new UniTask[animations.Length];
			for (int i = 0; i < animations.Length; i++) {
				animationTasks[i] = animations[i].ExecuteAsync(cancellationToken);
			}
			return animationTasks;
		}
		void ApplyActiveCancellation(
			ActiveAnimation[] activeAnimations,
			AnimationCancelBehaviour animationCancelBehaviour
		) {
			Restore(activeAnimations);
			switch (animationCancelBehaviour) {
				case AnimationCancelBehaviour.Complete:
					Render = true;
					_state = UIActivationState.Active;
					break;
				case AnimationCancelBehaviour.ResetToStart:
					Render = false;
					_state = UIActivationState.Inactive;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(animationCancelBehaviour), animationCancelBehaviour, null);
			}
		}
		void ApplyDeactivateCancellation(
			DeactivateAnimation[] deactivateAnimations,
			AnimationCancelBehaviour animationCancelBehaviour
		) {
			Restore(deactivateAnimations);
			switch (animationCancelBehaviour) {
				case AnimationCancelBehaviour.Complete:
					Render = false;
					_state = UIActivationState.Inactive;
					break;
				case AnimationCancelBehaviour.ResetToStart:
					Render = true;
					_state = UIActivationState.Active;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(animationCancelBehaviour), animationCancelBehaviour, null);
			}
		}
		static void EnsureNoneTransitioning(UIActivator[] uiActivators) {
			for (int i = 0; i < uiActivators.Length; i++) {
				if (uiActivators[i]._isTransitioning) {
					throw new InvalidOperationException($"{nameof(UIActivator)} '{uiActivators[i].name}' is already transitioning.");
				}
			}
		}
		void UpdateVisibleAndFade() {
			CanvasGroup.alpha = _render && _visible ? _fade : 0f;
		}
		void UpdateRaycastable() {
			CanvasGroup.blocksRaycasts =
				_render &&
				_raycastable &&
				(_disableRaycastWhileAnimation == false || _isTransitioning == false);
		}
		void UpdateInteractable() {
			CanvasGroup.interactable = _render && _interactable;
		}
		void UpdateLayoutParticipation() {
			if (_layoutElement == null)
				_layoutElement = GetComponent<LayoutElement>();

			SetLayoutIgnored(_state == UIActivationState.Inactive);
		}
		void SetLayoutIgnored(bool ignored) {
			if (_layoutElement != null)
				_layoutElement.ignoreLayout = _excludeFromLayoutWhenInactive && ignored;
		}

#if UNITY_EDITOR
		[PropertyOrder(-112), ButtonGroup("Component Utility", Order = -110), Button("Add Canvas"), DisableInPlayMode, DisableIf("@Canvas != null")] void AddCanvasFromInspector() {
			if (Canvas != null) return;
			Undo.AddComponent<Canvas>(gameObject);
		}
		[PropertyOrder(-111), ButtonGroup("Component Utility", Order = -110), Button("Add GraphicRaycaster"), PropertyTooltip("Canvas가 없으면 Canvas도 함께 추가됩니다."), DisableInPlayMode, DisableIf("@this.GetComponent<UnityEngine.UI.GraphicRaycaster>() != null")] void AddGraphicRaycasterFromInspector() {
			if (GetComponent<UnityEngine.UI.GraphicRaycaster>() != null) return;

			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Add UI Graphic Raycaster");
			if (Canvas == null) Undo.AddComponent<Canvas>(gameObject);
			Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(gameObject);
			Undo.CollapseUndoOperations(undoGroup);
		}

		[PropertyOrder(-102), ButtonGroup("Preview Utility", Order = -100), Button("Solo Active"), DisableInPlayMode, DisableIf(nameof(IsTransitioning))] void SoloActiveFromInspector() {
			Transform parent = transform.parent;
			UIActivator[] siblingActivators = parent == null
				? Array.Empty<UIActivator>()
				: Enumerable.Range(0, parent.childCount)
				            .Select(index => parent.GetChild(index).GetComponent<UIActivator>())
				            .Where(uiActivator => uiActivator != null && uiActivator != this)
				            .ToArray();
			UIActivator[] affectedActivators = siblingActivators.Append(this).ToArray();
			EnsureNoneTransitioning(affectedActivators);
			UnityEngine.Object[] undoTargets = affectedActivators
			                                  .SelectMany(uiActivator => uiActivator.GetComponents<Component>())
			                                  .Cast<UnityEngine.Object>()
			                                  .ToArray();
			Undo.RecordObjects(undoTargets, "Solo Active UI");

			for (int i = 0; i < siblingActivators.Length; i++) {
				siblingActivators[i].DeactivateImmediate();
			}
			ActiveImmediate();

			foreach (UnityEngine.Object undoTarget in undoTargets) {
				PrefabUtility.RecordPrefabInstancePropertyModifications(undoTarget);
				EditorUtility.SetDirty(undoTarget);
			}
			if (gameObject.scene.IsValid()) {
				EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
		}
		[PropertyOrder(-100), ButtonGroup("Preview Utility", Order = -100), Button("Active"), DisableIf(nameof(IsTransitioning))] void ActiveFromInspector() {
			if (Application.isPlaying)
				ActiveAsync(cancellationToken: Application.exitCancellationToken).Forget();
			else
				ActiveImmediate();

			EditorUtility.SetDirty(gameObject);
		}
		[PropertyOrder(-99), ButtonGroup("Preview Utility", Order = -100), Button("Deactivate"), DisableIf(nameof(IsTransitioning))] void DeactivateFromInspector() {
			if (Application.isPlaying)
				DeactivateAsync(cancellationToken: Application.exitCancellationToken).Forget();
			else
				DeactivateImmediate();

			EditorUtility.SetDirty(gameObject);
		}

#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/DOTween")]
		[ButtonGroup("Add Animation Utility/DOTween/Buttons"), Button("Fade")] void AddDOTweenFadeAnimations() {
			AddAnimationPair<DOTweenAnimations.UIFadeActiveAnimation, DOTweenAnimations.UIFadeDeactivateAnimation>();
		}
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/DOTween")]
		[ButtonGroup("Add Animation Utility/DOTween/Buttons"), Button("Scale")] void AddDOTweenScaleAnimations() {
			AddAnimationPair<DOTweenAnimations.UIScaleActiveAnimation, DOTweenAnimations.UIScaleDeactivateAnimation>();
		}
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/DOTween")]
		[ButtonGroup("Add Animation Utility/DOTween/Buttons"), Button("Slide")] void AddDOTweenSlideAnimations() {
			AddAnimationPair<DOTweenAnimations.UISlideActiveAnimation, DOTweenAnimations.UISlideDeactivateAnimation>();
		}
#endif
#if LITMOTION_SUPPORT
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Fade")] void AddLitMotionFadeAnimations() {
			AddAnimationPair<LitMotionAnimations.UIFadeActiveAnimation, LitMotionAnimations.UIFadeDeactivateAnimation>();
		}
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Scale")] void AddLitMotionScaleAnimations() {
			AddAnimationPair<LitMotionAnimations.UIScaleActiveAnimation, LitMotionAnimations.UIScaleDeactivateAnimation>();
		}
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Slide")] void AddLitMotionSlideAnimations() {
			AddAnimationPair<LitMotionAnimations.UISlideActiveAnimation, LitMotionAnimations.UISlideDeactivateAnimation>();
		}
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Move From")] void AddLitMotionMoveFromAnimations() {
			AddAnimationPair<LitMotionAnimations.UIMoveFromActiveAnimation, LitMotionAnimations.UIMoveFromDeactivateAnimation>();
		}
#endif

		void AddAnimationPair<TActiveAnimation, TDeactivateAnimation>() where TActiveAnimation : Component where TDeactivateAnimation : Component {
			gameObject.GetOrAddComponent<TActiveAnimation>();
			gameObject.GetOrAddComponent<TDeactivateAnimation>();
			EditorUtility.SetDirty(gameObject);
		}
#endif
	}
}
