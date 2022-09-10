using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// Labels are graphics that display text.
    /// </summary>

    [AddComponentMenu("UI/SmartText", -1)]
    public class SmartText : Text
    {
        SmartTextGenerator m_textGenerator;
        SmartTextGenerator m_textGeneratorForLayout;

        [NonSerialized]
        int m_isFilling;

        [SerializeField]
        bool m_EnableSmartWrap = false;

        [SerializeField]
        bool m_EnableJustify = false;

        [SerializeField]
        bool m_EnableLayoutProperties = true;

        [SerializeField]
        float m_BendStrength = 0;

        [SerializeField]
        float m_CharSpacing = 0;

        [SerializeField]
        int m_EnableCapitals = 0;

        [SerializeField]
        Font[] m_Fonts = null;

        //[SerializeField]
        //Material _customMaterial;

        public bool enableJustify
        {
            get { return m_EnableJustify; }
            set { m_EnableJustify = value; SetAllDirty(); }
        }

        public bool enableSmartWrap
        {
            get { return m_EnableSmartWrap; }
            set { m_EnableSmartWrap = value; SetAllDirty(); }
        }

        public float bendStrength
        {
            get { return m_BendStrength; }
            set { m_BendStrength = value; SetAllDirty(); }
        }

        public float charSpacing
        {
            get { return m_CharSpacing; }
            set { m_CharSpacing = value; SetAllDirty(); }
        }

        public int enableCapitals
        {
            get { return m_EnableCapitals; }
            set { m_EnableCapitals = value; SetAllDirty(); }
        }

        SmartTextGenerator textGenerator
        {
            get { return (m_textGenerator != null ? m_textGenerator : m_textGenerator = new SmartTextGenerator()); }
        }

        SmartTextGenerator textGeneratorForLayout
        {
            get { return (m_textGeneratorForLayout != null ? m_textGeneratorForLayout : m_textGeneratorForLayout = new SmartTextGenerator()); }
        }

        protected SmartText()
        {}

        protected override void OnEnable()
        {
            base.OnEnable();
            m_isFilling = 0;
            //_customMaterial = null;
            textGenerator.Invalidate();
        }

        protected override void OnDisable()
        {
            //
            base.OnDisable();
        }

        protected override void UpdateGeometry()
        {
            base.UpdateGeometry();
        }

        protected virtual void Update()
        {
            if (m_Material == null)
            {
                base.material = (Material)Resources.Load("SmartFontMaterial");
                //Debug.Log("Material created: " + _customMaterial.shader);
            }
        }
        /// <summary>
        /// Draw the Text.
        /// </summary>
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
       

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
//            List<UIVertex> vbo = new List<UIVertex>();
            toFill.Clear();
            //toFill.GetUIVertexStream(vbo);
            //vbo.Clear();

            if (font == null)
                return;

            if (m_isFilling >= 1)
            {
                return;
            }

            m_isFilling += 1;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            textGenerator.Populate(m_Text, settings, m_EnableSmartWrap, m_EnableJustify, pixelsPerUnit, m_Fonts, m_BendStrength, m_CharSpacing, m_EnableCapitals);

            Rect inputRect = rectTransform.rect;

            // get the text alignment anchor point for the text in local space
            Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
            Vector2 refPoint = Vector2.zero;
            refPoint.x = (textAnchorPivot.x == 1 ? inputRect.xMax : inputRect.xMin);
            refPoint.y = (textAnchorPivot.y == 0 ? inputRect.yMin : inputRect.yMax);

            // Determine fraction of pixel to offset text mesh.
            Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

            // Apply the offset to the vertices
            IList<UIVertex> verts = textGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    UIVertex uiv = verts[i];
                    uiv.position *= unitsPerPixel;
                    uiv.position.x += roundingOffset.x;
                    uiv.position.y += roundingOffset.y;
                    toFill.AddVert(uiv);
                }
            }
            else
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    UIVertex uiv = verts[i];
                    uiv.position *= unitsPerPixel;
                    toFill.AddVert(uiv);
                }
            }

            for (int i = 4; i <=verts.Count; i+=4)
            {
                toFill.AddTriangle(i- 4, i - 3, i - 2);
                toFill.AddTriangle(i - 2, i - 1, i - 4);
            }


            m_isFilling = Mathf.Max(0, m_isFilling - 1);

            if ((m_Material != null) && (m_Fonts != null))
            {
                for (var i = 0; i < m_Fonts.Length; i += 1)
                {
                    m_Material.SetTexture("_ExtraTex" + i, m_Fonts[i].material.mainTexture);
                }
            }

            /*var tex = mainTexture as Texture2D;
            if (tex != null)
            {
                Debug.Log("format: " + tex.format + ", size: " + tex.width + "/" + tex.height + ", mips: " + tex.mipmapCount + " ");
            }*/

           // base.OnPopulateMesh(toFill);
        }

        public override void CalculateLayoutInputHorizontal() {}
        public override void CalculateLayoutInputVertical() { }

        public override float minWidth
        {
            get { return 0; }
        }

        public override float preferredWidth
        {
            get
            {
                var settings = GetGenerationSettings(Vector2.zero);
                return textGeneratorForLayout.GetPreferredWidth(m_Text, settings, m_EnableLayoutProperties, pixelsPerUnit, m_Fonts, m_CharSpacing, m_EnableCapitals) / pixelsPerUnit;
            }
        }

        public override float flexibleWidth { get { return -1; } }

        public override float minHeight
        {
            get { return 0; }
        }

        public override float preferredHeight
        {
            get
            {
                var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
                return textGeneratorForLayout.GetPreferredHeight(m_Text, settings, m_EnableLayoutProperties, m_EnableSmartWrap, pixelsPerUnit, m_Fonts, m_CharSpacing, m_EnableCapitals) / pixelsPerUnit;
            }
        }

        public override float flexibleHeight { get { return -1; } }

        public override int layoutPriority { get { return 0; } }

/*#if UNITY_EDITOR
        public override void OnRebuildRequested()
        {
            // After a Font asset gets re-imported the managed side gets deleted and recreated,
            // that means the delegates are not persisted.
            // so we need to properly enforce a consistent state here.
            FontUpdateTracker.UntrackText(this);
            FontUpdateTracker.TrackText(this);

            // Also the textgenerator is no longer valid.
            _textGenerator.Invalidate();

            //base.OnRebuildRequested();
        }

#endif // if UNITY_EDITOR*/
    }
}
