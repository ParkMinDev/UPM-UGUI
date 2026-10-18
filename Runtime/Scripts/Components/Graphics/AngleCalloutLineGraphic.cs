using ParkMinDev.UPM.Foundation.Constants;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ParkMinDev.UPM.UGUI.Components.Graphics
{
    [AddComponentMenu("UI/ParkMinDev/Angle Callout Line Graphic"), RequireComponent(typeof(CanvasRenderer))]
    public sealed class AngleCalloutLineGraphic : MaskableGraphic
    {
        // - Public Methods -
        public void SetReferences(AngleGraphic angle, RectTransform target) {
            _angle = angle;
            _target = target;
            Refresh();
        }

        public void Refresh() {
            bool visible = _angle != null && _angle.isActiveAndEnabled && _target != null && _target.gameObject.activeInHierarchy && Mathf.Abs(_angle.SweepAngle) > 0.001f;
            Vector2 start = default;
            Vector2 bend = default;
            Vector2 end = default;
            if (visible) {
                // Attach to the arc midpoint, including the source transform's scale
                float radians = (_angle.StartAngle + _angle.SweepAngle * 0.5f) * Mathf.Deg2Rad;
                Vector2 arcPoint = _angle.rectTransform.rect.center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * _angle.Radius;
                Vector3 worldStart = _angle.rectTransform.TransformPoint(arcPoint);
                start = rectTransform.InverseTransformPoint(worldStart);

                // Stop at the label boundary rather than passing underneath its content
                Rect targetRect = _target.rect;
                Vector2 direction = (Vector2)_target.InverseTransformPoint(worldStart) - targetRect.center;
                Vector2 edge;
                if (_useElbow) {
                    float side = direction.x < 0f ? -1f : 1f;
                    edge = targetRect.center + Vector2.right * (targetRect.width * 0.5f * side);
                    end = rectTransform.InverseTransformPoint(_target.TransformPoint(edge));
                    bend = rectTransform.InverseTransformPoint(_target.TransformPoint(edge + Vector2.right * (_endSegmentLength * side)));
                }
                else {
                    float extent = Mathf.Max(Mathf.Abs(direction.x) / Mathf.Max(targetRect.width * 0.5f, 0.001f), Mathf.Abs(direction.y) / Mathf.Max(targetRect.height * 0.5f, 0.001f));
                    edge = targetRect.center + direction / Mathf.Max(extent, 0.001f);
                    end = rectTransform.InverseTransformPoint(_target.TransformPoint(edge));
                    bend = end;
                }
                visible = !targetRect.Contains(_target.InverseTransformPoint(worldStart));
            }

            if (_visible == visible && _start == start && _bend == bend && _end == end)
                return;
            _visible = visible;
            _start = start;
            _bend = bend;
            _end = end;
            SetVerticesDirty();
        }

        // - Handler -
        protected override void OnPopulateMesh(VertexHelper vertexHelper) {
            vertexHelper.Clear();
            if (!_visible)
                return;
            AddSegment(vertexHelper, _start, _bend);
            if (_useElbow)
                AddSegment(vertexHelper, _bend, _end);
        }

        void LateUpdate() {
            Refresh();
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            raycastTarget = false;
            color = new Color(1f, 0.69f, 0.18f, 1f);
        }
#endif

        // - Private & Protected -
        [Title(Headers.Required)]
        [SerializeField, Required] AngleGraphic _angle;
        [SerializeField, Required] RectTransform _target;

        [Title(Headers.Settings)]
        [SerializeField, Min(0.1f)] float _thickness = 1.5f;
        [SerializeField] bool _useElbow = true;
        [SerializeField, Min(0f), ShowIf(nameof(_useElbow))] float _endSegmentLength = 18f;

        bool _visible;
        Vector2 _start;
        Vector2 _bend;
        Vector2 _end;

        void AddSegment(VertexHelper vertexHelper, Vector2 from, Vector2 to) {
            Vector2 direction = to - from;
            if (direction.sqrMagnitude < 0.0001f)
                return;
            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * (_thickness * 0.5f);
            int index = vertexHelper.currentVertCount;
            vertexHelper.AddVert(from - normal, color, Vector2.zero);
            vertexHelper.AddVert(from + normal, color, Vector2.zero);
            vertexHelper.AddVert(to + normal, color, Vector2.zero);
            vertexHelper.AddVert(to - normal, color, Vector2.zero);
            vertexHelper.AddTriangle(index, index + 1, index + 2);
            vertexHelper.AddTriangle(index, index + 2, index + 3);
        }
    }
}
