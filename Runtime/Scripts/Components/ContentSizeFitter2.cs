using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinDev.UPM.UGUI.Components
{
    [MovedFrom(true, sourceNamespace: "ParkMin.MIN_Wallet.Components.General", sourceAssembly: "Assembly-CSharp", sourceClassName: "ContentSizeFitter2")]
    [ExecuteAlways, DisallowMultipleComponent, AddComponentMenu("Layout/Content Size Fitter 2")]
    public sealed class ContentSizeFitter2 : UnityEngine.UI.ContentSizeFitter
    {
        // - Public Methods -
        public override void SetLayoutHorizontal() {
            _tracker.Clear();
            LayoutGroup parent = GetParentLayoutGroup();
            DrivenTransformProperties properties = DrivenTransformProperties.None;

            // Parent layout ownership
            if (parent != null) {
                properties = DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition;
                if (parent is HorizontalOrVerticalLayoutGroup linear) {
                    if (linear.childControlWidth) properties |= DrivenTransformProperties.SizeDeltaX;
                    if (linear.childControlHeight) properties |= DrivenTransformProperties.SizeDeltaY;
                }
                else {
                    properties |= DrivenTransformProperties.SizeDelta;
                }
            }
            if (horizontalFit != FitMode.Unconstrained && horizontalFit != FitMode.Clamped) properties |= DrivenTransformProperties.SizeDeltaX;
            if (verticalFit != FitMode.Unconstrained && verticalFit != FitMode.Clamped) properties |= DrivenTransformProperties.SizeDeltaY;
            _tracker.Add(parent != null ? (Object)parent : this, RectTransform, properties);

            // Content sizing
            FitAlongAxis(0, parent);
        }

        public override void SetLayoutVertical() {
            FitAlongAxis(1, GetParentLayoutGroup());
        }

        // - Handler -
        protected override void OnDisable() {
            _tracker.Clear();
            base.OnDisable();
        }

        // - Private & Protected -
        RectTransform _rect;
        DrivenRectTransformTracker _tracker;

        RectTransform RectTransform => _rect != null ? _rect : _rect = GetComponent<RectTransform>();

        LayoutGroup GetParentLayoutGroup() {
            if (transform.parent == null) return null;
            LayoutGroup parent = transform.parent.GetComponent<LayoutGroup>();
            if (parent == null || !parent.isActiveAndEnabled) return null;
            if (!(parent is HorizontalOrVerticalLayoutGroup) && !(parent is GridLayoutGroup)) return null;
            ILayoutIgnorer[] ignorers = GetComponents<ILayoutIgnorer>();
            return ignorers.Length > 0 && ignorers.All(ignorer => ignorer.ignoreLayout) ? null : parent;
        }

        void FitAlongAxis(int axis, LayoutGroup parent) {
            // Parent-controlled dimensions
            if (parent is GridLayoutGroup) return;
            if (parent is HorizontalOrVerticalLayoutGroup linear && (axis == 0 ? linear.childControlWidth : linear.childControlHeight)) return;

            // Content-controlled dimensions
            FitMode fitting = axis == 0 ? horizontalFit : verticalFit;
            float size;
            switch (fitting) {
                case FitMode.MinSize:
                    size = LayoutUtility.GetMinSize(RectTransform, axis);
                    break;
                case FitMode.PreferredSize:
                    size = LayoutUtility.GetPreferredSize(RectTransform, axis);
                    break;
                case FitMode.Clamped:
                    size = Mathf.Clamp(RectTransform.rect.size[axis], LayoutUtility.GetMinSize(RectTransform, axis), LayoutUtility.GetMaxSize(RectTransform, axis));
                    break;
                default:
                    return;
            }
            RectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
        }
    }
}
