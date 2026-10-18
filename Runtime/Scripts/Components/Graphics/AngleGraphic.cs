using System;
using ParkMinDev.UPM.Foundation.Constants;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ParkMinDev.UPM.UGUI.Components.Graphics
{
    [AddComponentMenu("UI/ParkMinDev/Angle Graphic"), RequireComponent(typeof(CanvasRenderer))]
    public sealed class AngleGraphic : UnityEngine.UI.MaskableGraphic
    {
        // - Public Methods -
        public void SetAngle(float startAngleDegrees, float sweepAngleDegrees) {
            ValidateFinite(startAngleDegrees, nameof(startAngleDegrees));
            ValidateFinite(sweepAngleDegrees, nameof(sweepAngleDegrees));
            float start = Mathf.Repeat(startAngleDegrees, 360f);
            float sweep = Mathf.Clamp(sweepAngleDegrees, -360f, 360f);
            if (Mathf.Approximately(_startAngle, start) && Mathf.Approximately(_sweepAngle, sweep))
                return;

            _startAngle = start;
            _sweepAngle = sweep;
            SetVerticesDirty();
        }

        public bool SetAngle(Vector2 firstPoint, Vector2 vertex, Vector2 lastPoint) {
            Vector2 firstDirection = firstPoint - vertex;
            Vector2 lastDirection = lastPoint - vertex;
            if (firstDirection.sqrMagnitude <= Mathf.Epsilon || lastDirection.sqrMagnitude <= Mathf.Epsilon) {
                SetAngle(0f, 0f);
                return false;
            }

            SetAngle(Mathf.Atan2(firstDirection.y, firstDirection.x) * Mathf.Rad2Deg, Vector2.SignedAngle(firstDirection, lastDirection));
            return true;
        }

        public void ApplyOptions(AngleGraphicOptions options) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            SetVerticesDirty();
        }

        // - Public Properties -
        public AngleGraphicOptions Options { get { return _options; } }

        public float StartAngle
        {
            get { return _startAngle; }
            set { SetAngle(value, _sweepAngle); }
        }

        public float SweepAngle
        {
            get { return _sweepAngle; }
            set { SetAngle(_startAngle, value); }
        }

        public float Radius
        {
            get { return _options.Radius; }
            set {
                ValidateFinite(value, nameof(value));
                float radius = Mathf.Max(0f, value);
                if (Mathf.Approximately(_options.Radius, radius))
                    return;
                _options.Radius = radius;
                SetVerticesDirty();
            }
        }

        public float ArcThickness
        {
            get { return _options.ArcThickness; }
            set {
                ValidateFinite(value, nameof(value));
                float thickness = Mathf.Max(0f, value);
                if (Mathf.Approximately(_options.ArcThickness, thickness))
                    return;
                _options.ArcThickness = thickness;
                SetVerticesDirty();
            }
        }

        // - Handler -
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vertices) {
            vertices.Clear();
            Vector2 center = rectTransform.rect.center;
            float radius = Mathf.Max(0f, _options.Radius);
            float sweep = Mathf.Clamp(_sweepAngle, -360f, 360f);
            int segments = Mathf.Clamp(_options.Segments, 8, 256);
            int arcSegments = Mathf.Max(1, Mathf.CeilToInt(segments * Mathf.Abs(sweep) / 360f));

            // Sector
            if (_options.ShowFill && radius > 0f && Mathf.Abs(sweep) > Mathf.Epsilon)
                AddSector(vertices, center, radius, _startAngle, sweep, arcSegments, _options.FillColor * color);

            // Guides
            if (_options.ShowGuides && _options.GuideThickness > 0f) {
                float length = radius + Mathf.Max(0f, _options.GuideExtension);
                int guideCount = Mathf.Approximately(sweep, 0f) || Mathf.Approximately(Mathf.Abs(sweep), 360f) ? 1 : 2;
                for (int index = 0; index < guideCount; index++) {
                    Vector2 direction = Direction(_startAngle + sweep * index);
                    if (_options.DashedGuides) {
                        float dashLength = Mathf.Max(0.1f, _options.DashLength);
                        float step = Mathf.Max(dashLength + Mathf.Max(0f, _options.DashGap), length / MaxGuideDashes);
                        for (int dash = 0; dash < MaxGuideDashes; dash++) {
                            float start = dash * step;
                            if (start >= length)
                                break;
                            AddLine(vertices, center + direction * start, center + direction * Mathf.Min(start + dashLength, length), _options.GuideThickness, _options.GuideColor * color);
                        }
                    }
                    else {
                        AddLine(vertices, center, center + direction * length, _options.GuideThickness, _options.GuideColor * color);
                    }
                }
            }

            // Arc
            if (_options.ShowArc && radius > 0f && _options.ArcThickness > 0f && Mathf.Abs(sweep) > Mathf.Epsilon)
                AddBand(vertices, center, Mathf.Max(0f, radius - _options.ArcThickness * 0.5f), radius + _options.ArcThickness * 0.5f, _startAngle, sweep, arcSegments, _options.ArcColor * color);

            // Vertex marker
            if (_options.ShowVertex && _options.VertexRadius > 0f) {
                AddSector(vertices, center, _options.VertexRadius, 0f, 360f, segments, _options.VertexFillColor * color);
                if (_options.VertexOutlineThickness > 0f)
                    AddBand(vertices, center, Mathf.Max(0f, _options.VertexRadius - _options.VertexOutlineThickness), _options.VertexRadius, 0f, 360f, segments, _options.ArcColor * color);
            }
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            raycastTarget = false;
        }
#endif

        // - Private & Protected -
        [Title(Headers.Settings)]
        [SerializeField, Range(-360f, 360f), Tooltip("Degrees counterclockwise from the local right direction.")] float _startAngle = 35f;
        [SerializeField, Range(-360f, 360f), Tooltip("Positive is counterclockwise; negative is clockwise.")] float _sweepAngle = 110f;

        [Title("Appearance")]
        [SerializeField, InlineProperty, HideLabel] AngleGraphicOptions _options = new AngleGraphicOptions();

        void AddSector(UnityEngine.UI.VertexHelper vertices, Vector2 center, float radius, float startAngle, float sweepAngle, int segments, Color32 tint) {
            int first = vertices.currentVertCount;
            vertices.AddVert(center, tint, Vector2.zero);
            for (int index = 0; index <= segments; index++)
                vertices.AddVert(center + Direction(startAngle + sweepAngle * index / segments) * radius, tint, Vector2.zero);
            for (int index = 0; index < segments; index++)
                vertices.AddTriangle(first, first + index + 1, first + index + 2);
        }

        void AddBand(UnityEngine.UI.VertexHelper vertices, Vector2 center, float innerRadius, float outerRadius, float startAngle, float sweepAngle, int segments, Color32 tint) {
            int first = vertices.currentVertCount;
            for (int index = 0; index <= segments; index++) {
                Vector2 direction = Direction(startAngle + sweepAngle * index / segments);
                vertices.AddVert(center + direction * innerRadius, tint, Vector2.zero);
                vertices.AddVert(center + direction * outerRadius, tint, Vector2.zero);
            }
            for (int index = 0; index < segments; index++) {
                int offset = first + index * 2;
                vertices.AddTriangle(offset, offset + 1, offset + 3);
                vertices.AddTriangle(offset + 3, offset + 2, offset);
            }
        }

        void AddLine(UnityEngine.UI.VertexHelper vertices, Vector2 start, Vector2 end, float thickness, Color32 tint) {
            Vector2 direction = end - start;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;
            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * (thickness * 0.5f);
            int first = vertices.currentVertCount;
            vertices.AddVert(start - normal, tint, Vector2.zero);
            vertices.AddVert(start + normal, tint, Vector2.zero);
            vertices.AddVert(end + normal, tint, Vector2.zero);
            vertices.AddVert(end - normal, tint, Vector2.zero);
            vertices.AddTriangle(first, first + 1, first + 2);
            vertices.AddTriangle(first + 2, first + 3, first);
        }

        // - Private Statics -
        const int MaxGuideDashes = 512;

        static Vector2 Direction(float degrees) {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        static void ValidateFinite(float value, string parameterName) {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "An angle graphic requires finite values.");
        }
    }
}
