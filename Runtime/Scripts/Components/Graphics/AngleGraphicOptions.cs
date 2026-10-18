using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ParkMinDev.UPM.UGUI.Components.Graphics
{
    [Serializable]
    public sealed class AngleGraphicOptions
    {
        // - Public Properties -
        public float Radius { get { return _radius; } set { _radius = value; } }
        public bool ShowArc { get { return _showArc; } set { _showArc = value; } }
        public float ArcThickness { get { return _arcThickness; } set { _arcThickness = value; } }
        public Color ArcColor { get { return _arcColor; } set { _arcColor = value; } }
        public bool ShowFill { get { return _showFill; } set { _showFill = value; } }
        public Color FillColor { get { return _fillColor; } set { _fillColor = value; } }
        public bool ShowGuides { get { return _showGuides; } set { _showGuides = value; } }
        public float GuideExtension { get { return _guideExtension; } set { _guideExtension = value; } }
        public float GuideThickness { get { return _guideThickness; } set { _guideThickness = value; } }
        public Color GuideColor { get { return _guideColor; } set { _guideColor = value; } }
        public bool DashedGuides { get { return _dashedGuides; } set { _dashedGuides = value; } }
        public float DashLength { get { return _dashLength; } set { _dashLength = value; } }
        public float DashGap { get { return _dashGap; } set { _dashGap = value; } }
        public bool ShowVertex { get { return _showVertex; } set { _showVertex = value; } }
        public float VertexRadius { get { return _vertexRadius; } set { _vertexRadius = value; } }
        public float VertexOutlineThickness { get { return _vertexOutlineThickness; } set { _vertexOutlineThickness = value; } }
        public Color VertexFillColor { get { return _vertexFillColor; } set { _vertexFillColor = value; } }
        public int Segments { get { return _segments; } set { _segments = value; } }

        // - Private & Protected -
        [SerializeField, Min(0f)] float _radius = 145f;

        [Title("Arc")]
        [SerializeField] bool _showArc = true;
        [SerializeField, Min(0f)] float _arcThickness = 4f;
        [SerializeField] Color _arcColor = new Color(1f, 0.69f, 0.18f, 1f);

        [Title("Sector Fill")]
        [SerializeField] bool _showFill = true;
        [SerializeField] Color _fillColor = new Color(1f, 0.69f, 0.18f, 0.12f);

        [Title("Guides")]
        [SerializeField] bool _showGuides = true;
        [SerializeField, Min(0f)] float _guideExtension = 40f;
        [SerializeField, Min(0f)] float _guideThickness = 1.5f;
        [SerializeField] Color _guideColor = new Color(0.93f, 0.97f, 0.94f, 1f);
        [SerializeField] bool _dashedGuides;
        [SerializeField, Min(0.1f)] float _dashLength = 10f;
        [SerializeField, Min(0f)] float _dashGap = 7f;

        [Title("Vertex Marker")]
        [SerializeField] bool _showVertex = true;
        [SerializeField, Min(0f)] float _vertexRadius = 7f;
        [SerializeField, Min(0f)] float _vertexOutlineThickness = 2f;
        [SerializeField] Color _vertexFillColor = Color.white;

        [FoldoutGroup("Advanced Settings"), SerializeField, Range(8, 256)] int _segments = 128;
    }
}
