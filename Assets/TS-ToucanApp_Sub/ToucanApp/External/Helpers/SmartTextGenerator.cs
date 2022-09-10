#pragma warning disable CS0618

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

//public enum SmartWrapRuleSet ??
namespace UnityEngine.UI
{
    public class SmartTextGenerator
    {
        const int MaxWordLengthOnEndOfLine = 4;

        class CustomCharInfo
        {
            public CustomCharInfo(char c, int position, int advance, int styleId, bool breaksAfter, bool isWhiteSpace)
            {
                Char = c;
                Position = position;
                Advance = advance;
                StyleId = styleId;
                BreaksAfter = breaksAfter;
                IsWhiteSpace = isWhiteSpace;
            }

            public readonly char Char;
            public readonly int Position;
            public readonly int StyleId;
            public readonly bool IsWhiteSpace;

            public int Advance;
            public bool BreaksAfter;
        };

        struct FontKey
        {
            public readonly Font Font;
            public readonly int Size;
            public readonly FontStyle Style;
            public readonly int BaseSize;
            public readonly float ScaleFactor;

            public FontKey(Font font, FontStyle style, int baseSize, float scaleFactor)
            {
                Font = font;
                var realFontSize = baseSize * scaleFactor;
                Size = Mathf.FloorToInt(realFontSize);
                Style = style;
                BaseSize = baseSize;
                ScaleFactor = scaleFactor;
            }

            public override bool Equals(object obj)
            {
                var other = (FontKey)obj;
                if(Font != other.Font) { return false; }
                if(Size != other.Size) { return false; }
                if(Style != other.Style) { return false; }
                if(BaseSize != other.BaseSize) { return false; }
                if(ScaleFactor != other.ScaleFactor) { return false; }
                return true;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + Font.GetHashCode();
                hash = hash * 31 + Size.GetHashCode();
                hash = hash * 31 + Style.GetHashCode();
                hash = hash * 31 + BaseSize.GetHashCode();
                hash = hash * 31 + ScaleFactor.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                return "" + Font + " [" + BaseSize + ":" + Size + "][" + Style + "]";
            }
        };



        class FontMetrics
        {
            public int BaseY { get; private set; }
            public int BaseLineHeight { get; private set; }

            public FontMetrics()
            {
            }

            public void Calc(FontKey fontKey)
            {
                float maxY = float.MinValue;
                float minY = float.MaxValue;
                var font = fontKey.Font;
                var size = fontKey.Size;
                var style = fontKey.Style;

                for (var i = 0; i < _mainASCIICharacters.Length; i += 1)
                {
                    CharacterInfo info;
                    if (!font.GetCharacterInfo(_mainASCIICharacters[i], out info, size, style))
                    {
                        Debug.LogError("Missing character: " + _mainASCIICharacters[i]);
                        continue;
                    }
                    maxY = Mathf.Max(maxY, info.vert.y);
                    minY = Mathf.Min(minY, info.vert.y + info.vert.height);

                    //Debug.Log(_mainASCIICharacters[i] + ": " + info.vert + ", maxY: " + info.vert.y + ", : minY: " + (info.vert.y + info.vert.height));
                }
                BaseLineHeight = Mathf.RoundToInt(-minY + maxY);
                BaseY = Mathf.RoundToInt(maxY);
                //Debug.Log("BasY: " + baseY + ", Base line height: " + baseLineHeight + ", minY: " + minY + ", maxY: " + maxY);
            }
        };

        class CustomStyle
        {
            public readonly Color Color;
            public readonly int FontId;
            public readonly FontKey FontKey;
            public readonly FontMetrics FontMetrics;
            public readonly int LineHeight;
            public readonly float LineSpacing;
//            public readonly float CharSpacing;

            /*public static void Reset()
            {
                _fontMetricsCache.Clear();
            }*/

            public CustomStyle(string text, int fontId, FontKey fontKey, Color color, float lineSpacing, float charSpacing)
            {
                FontId = fontId;
                FontKey = fontKey;
                if (!_fontMetricsCache.TryGetValue(fontKey, out FontMetrics))
                {
                    //Debug.Log("Adding style cache: " + FontKey);
                    FontMetrics = new FontMetrics();
                    _fontMetricsCache.Add(fontKey, FontMetrics);

                    if (fontKey.Font.dynamic)
                    {
                        fontKey.Font.RequestCharactersInTexture(_mainASCIICharacters, fontKey.Size, fontKey.Style);
                    }

                    FontMetrics.Calc(fontKey);
                    //Debug.Log("Added: " + FontKey);
                }

                fontKey.Font.RequestCharactersInTexture(text, fontKey.Size, fontKey.Style);

                Color = color;
                //LineHeight = Mathf.RoundToInt((FontMetrics.BaseLineHeight + 1) * lineSpacing);
                LineHeight = Mathf.RoundToInt(fontKey.Size * 1.2f * lineSpacing);
                LineSpacing = lineSpacing;
            }

            public bool GetCharacterInfo(char c, out CharacterInfo info)
            {
                return FontKey.Font.GetCharacterInfo(c, out info, FontKey.Size, FontKey.Style);
            }
        };

        struct LineInfo
        {
            public bool Inited;
            public int MaxHeight;
            public int Width;
            public int MaxBaseY;
            public int MinLineAddedYSpace;
            public int OffsetX;
            public int WhiteSpaceWidth;
            public int LastCharAdvance;
            public bool ForcedLineBreak;
        };

        static readonly Dictionary<FontKey, FontMetrics> _fontMetricsCache = new Dictionary<FontKey, FontMetrics>();

        readonly List<CustomStyle> _styles = new List<CustomStyle>();
        readonly List<int> _stylesStack = new List<int>();
        readonly List<UIVertex> m_verts = new List<UIVertex>();
        readonly List<CustomCharInfo> m_characters = new List<CustomCharInfo>();
        readonly static string _mainASCIICharacters = "aAbBdDiIWHT[]|ygjq";
        readonly List<LineInfo> _tempLineInfo = new List<LineInfo>();
        Font[] _fontsSet;
        int _maxLineWidth;
        int _totalHeight;

        bool m_enableSmartWrap;
        float m_bendStrength;
        float m_charSpacing;

        string m_normalText;
        string m_upperText;

        int m_enableCapitals;

        int _areaWidth;
        int _areaHeight;
        int _baseX;
        int _baseY;
        float _alignX;
        float _alignY;

        public IList<UIVertex> verts
        {
            get { return m_verts; }
        }

        public void Invalidate()
        {
            m_verts.Clear();
            m_characters.Clear();
            m_normalText = null;
            m_upperText = null;
            //CustomStyle.Reset();
            _styles.Clear();
            _stylesStack.Clear();
            //Debug.LogWarning("text generator cleared");
        }

        public string GetFormattedText()
        {
            return (m_enableCapitals != 0) ? m_upperText : m_normalText;
        }

        static void SwapX(ref Vector2 a, ref Vector2 b)
        {
            var x = a.x;
            a.x = b.x;
            b.x = x;
        }

        static void SwapY(ref Vector2 a, ref Vector2 b)
        {
            var y = a.y;
            a.y = b.y;
            b.y = y;
        }

        public float GetPreferredWidth(string text, TextGenerationSettings settings, bool enableLayoutProperties, float pixelsPerUnit, Font[] fontsSet,
            float charSpacing, int enableCapitals)
        {
            if (!enableLayoutProperties) { return 0; }

            settings.textAnchor = TextAnchor.UpperLeft;
            settings.horizontalOverflow = HorizontalWrapMode.Overflow;
            settings.verticalOverflow = VerticalWrapMode.Overflow;

            InitTextProperties(text, settings, false, false, pixelsPerUnit, fontsSet, charSpacing, enableCapitals);

            FillCharactersInfo(settings.richText);
            int maxOffsetX = 0;
            LayoutText(settings.horizontalOverflow, ref maxOffsetX);
            return maxOffsetX;
        }

        public float GetPreferredHeight(string text, TextGenerationSettings settings, bool enableLayoutProperties, bool enableSmartWrap, float pixelsPerUnit,
            Font[] fontsSet, float charSpacing, int enableCapitals)
        {
            if (!enableLayoutProperties) { return 0; }

            settings.textAnchor = TextAnchor.UpperLeft;
            settings.verticalOverflow = VerticalWrapMode.Overflow;

            InitTextProperties(text, settings, enableSmartWrap, false, pixelsPerUnit, fontsSet, charSpacing, enableCapitals);

            FillCharactersInfo(settings.richText);
            int maxOffsetX = 0;
            LayoutText(settings.horizontalOverflow, ref maxOffsetX);
            CalculateLinesInfo();
            return _totalHeight;
        }

        public void Populate(string text, TextGenerationSettings settings, bool enableSmartWrap, bool enableJustify, float pixelsPerUnit, Font[] fontsSet,
            float bendStrength, float charSpacing, int enableCapitals)
        {
            if (settings.resizeTextForBestFit)
            {
                Debug.LogWarning("Best fit is not supported yet :( ");
            }

            //var t = Time.realtimeSinceStartup;
            m_bendStrength = bendStrength * 0.1f / pixelsPerUnit;

            InitTextProperties(text, settings, enableSmartWrap, enableJustify, pixelsPerUnit, fontsSet, charSpacing, enableCapitals);

            FillCharactersInfo(settings.richText);
            int maxOffsetX = 0;
            LayoutText(settings.horizontalOverflow, ref maxOffsetX);
            CalculateLinesInfo();
            FillVertices(enableJustify, settings.verticalOverflow == VerticalWrapMode.Truncate);

            //Debug.Log("Populate time: " + (Time.realtimeSinceStartup - t).ToString("0.000"));
        }

        void InitTextProperties(string text, TextGenerationSettings settings, bool enableSmartWrap, bool enableJustify, float pixelsPerUnit, Font[] fontsSet,
            float charSpacing, int enableCapitals)
        {
            _fontsSet = fontsSet;
            m_enableSmartWrap = enableSmartWrap;
            m_normalText = text;
            if (enableCapitals != 0)
            {
                m_upperText = text.ToUpper();
            }
            m_charSpacing = charSpacing * pixelsPerUnit;
            m_enableCapitals = enableCapitals;

            ResetToDefaultStyle(settings, pixelsPerUnit);

            _areaWidth = Mathf.RoundToInt(settings.generationExtents.x * pixelsPerUnit);
            _areaHeight = Mathf.RoundToInt(settings.generationExtents.y * pixelsPerUnit);
            _baseX = Mathf.RoundToInt(-settings.pivot.x * _areaWidth);
            _baseY = Mathf.RoundToInt((1 - settings.pivot.y) * _areaHeight);
            _alignX = 0;
            _alignY = 0;
            CalcTextAlign(settings.textAnchor, ref _alignX, ref _alignY, enableJustify);
        }

        void ResetToDefaultStyle(TextGenerationSettings settings, float pixelsPerUnit)
        {
            //CustomStyle.Reset();

            var fontKey = new FontKey(settings.font, settings.fontStyle, settings.fontSize, pixelsPerUnit);
            _styles.Clear();
            _styles.Add(new CustomStyle(GetFormattedText(), 0, fontKey, settings.color, settings.lineSpacing, m_charSpacing)); // added default style
        }

        int SearchFirstLetterOfWord(int wordCharId)
        {
            var startChar = m_characters[wordCharId];
            int i = wordCharId - 1;
            for (; i >= 0; i -= 1)
            {
                var c = m_characters[i];
                if (c.IsWhiteSpace)
                {
                    i += 1;
                    break;
                }
                if (startChar.BreaksAfter)
                {
                    Debug.LogError("Line change inside word! [" + GetTextSubstring(i, wordCharId) + "]");
                    i += 1;
                    break;
                }
            }
            return Mathf.Max(i, 0);
        }

        int SearchLastLetterOfWord(int wordCharId)
        {
            int i = wordCharId + 1;
            for (; i < m_characters.Count; i += 1)
            {
                var c = m_characters[i];
                if (c.IsWhiteSpace)
                {
                    i -= 1;
                    break;
                }
            }
            return Mathf.Max(wordCharId, Mathf.Min(i, m_characters.Count - 1));
        }

        bool IsFirstWordInLine(int firstWordCharId)
        {
            var firstChar = m_characters[firstWordCharId];
            return (firstWordCharId == 0) || m_characters[firstWordCharId - 1].BreaksAfter;
        }

        void LayoutText(HorizontalWrapMode horizontalWrap, ref int maxOffsetX)
        {
            int offsetX = 0;
            maxOffsetX = 0;

            for (var i = 0; i < m_characters.Count;)
            {
                var c = m_characters[i];
                offsetX += c.Advance;

                if ((horizontalWrap == HorizontalWrapMode.Wrap) && (offsetX > _areaWidth) && !c.IsWhiteSpace)
                {
                    i = MoveWordToNewLine(i, ref offsetX);
                }

                if (c.BreaksAfter)
                {
                    maxOffsetX = Mathf.Max(offsetX, maxOffsetX);
                    offsetX = 0;
                }

                i += 1;
            }

            maxOffsetX = Mathf.Max(offsetX, maxOffsetX);
        }

        int? SearchFragmentToMoveDown(int lastCharIdInLine)
        {
            var i = lastCharIdInLine;
            for (; i >= 0; i -= 1)
            {
                var c = m_characters[i];
                var val = c.Char;
                if (val == '\n')
                {
                    i += 1;
                    break;
                }
                if (c.BreaksAfter)
                {
                    i += 1;
                    break;
                }
                if (!c.IsWhiteSpace)
                {
                    break;
                }
            }
            if (i <= 0)
            {
                return null;
            }
            if (i > lastCharIdInLine)
            {
                return null;
            }
            var lastWordCharId = i;
            var firstWordCharId = SearchFirstLetterOfWord(i);
            if ((lastWordCharId - firstWordCharId + 1) >= MaxWordLengthOnEndOfLine)
            {
                return null;
            }

            // check last character of word
            if (IsTextSubstringEqual(lastWordCharId, lastWordCharId, ","))
            {
                return null;
            }
            if (IsTextSubstringEqual(lastWordCharId, lastWordCharId, "."))
            {
                return null;
            }
            if (IsTextSubstringEqual(lastWordCharId, lastWordCharId, ":"))
            {
                return null;
            }
            if (IsTextSubstringEqual(lastWordCharId, lastWordCharId, ";"))
            {
                return null;
            }
            if (IsTextSubstringEqual(lastWordCharId, lastWordCharId, ")"))
            {
                return null;
            }

            var result = SearchFragmentToMoveDown(firstWordCharId - 1);
            return result ?? firstWordCharId;
        }

        bool IsTextSubstringEqual(int start, int end, string val)
        {
            if (end < start)
            {
                return false;
            }
            if ((end - start + 1) != val.Length)
            {
                return false;
            }

            int j = 0;
            for (var i = start; i <= end; i += 1, j += 1)
            {
                var c = m_characters[i].Char;
                if (c != val[j])
                {
                    return false;
                }
            }
            return true;
        }

        // returns last processed char id
        int MoveWordToNewLine(int i, ref int offsetX)
        {
            var firstWordCharId = SearchFirstLetterOfWord(i);
            var lastWordCharId = SearchLastLetterOfWord(i);

            if (IsFirstWordInLine(firstWordCharId))
            {
                // go to the end of word, cannot move down
                for (var j = i + 1; j <= lastWordCharId; j += 1)
                {
                    var jc = m_characters[j];
                    offsetX += jc.Advance;
                }
                return lastWordCharId;
            }

            if (m_enableSmartWrap)
            {
                int lastMoveId = firstWordCharId - 1;
                int? firstMoveId = SearchFragmentToMoveDown(lastMoveId);

                if (firstMoveId.HasValue && !IsFirstWordInLine(firstMoveId.Value))
                {
                    firstWordCharId = firstMoveId.Value;
                }
            }

            return MoveFragmentDown(i, ref offsetX, firstWordCharId, lastWordCharId);
        }

        // slow, use mainly for debugging purposes
        string GetTextSubstring(int charIdStart, int charIdEnd)
        {
            if (charIdStart > charIdEnd)
            {
                return string.Empty;
            }

            return m_normalText.Substring(m_characters[charIdStart].Position,
                    m_characters[charIdEnd].Position - m_characters[charIdStart].Position + 1);
        }

        // move characters one line down and update offsetX
        int MoveFragmentDown(int i, ref int offsetX, int firstWordCharId, int lastWordCharId)
        {
            //Debug.Log("Moving down: [" + GetTextSubstring(firstWordCharId, lastWordCharId) + "]");

            if (firstWordCharId > 0)
            {
                var c = m_characters[firstWordCharId - 1];
                c.BreaksAfter = true;
            }

            offsetX = 0;

            for (var j = firstWordCharId; j <= lastWordCharId; j += 1)
            {
                var jc = m_characters[j];
                offsetX += jc.Advance;
            }

            return lastWordCharId;
        }

        // mix baseStyle with style
        FontStyle ApplyStyle(FontStyle baseStyle, FontStyle style)
        {
            if (style == FontStyle.Normal)
            {
                return FontStyle.Normal;
            }
            if (style == FontStyle.Bold)
            {
                if ((baseStyle == FontStyle.Italic) || (baseStyle == FontStyle.BoldAndItalic))
                {
                    return FontStyle.BoldAndItalic;
                }
                return FontStyle.Bold;
            }
            if (style == FontStyle.Italic)
            {
                if ((baseStyle == FontStyle.Bold) || (baseStyle == FontStyle.BoldAndItalic))
                {
                    return FontStyle.BoldAndItalic;
                }
                return FontStyle.Italic;
            }
            return style;
        }

        // create new style with applied non-null values and add to _styles & _stylesStack
        void ApplyStyleTag(FontStyle? tagStyle, int? tagSize = null, int? fontId = null, Color? color = null)
        {
            var baseStyle = _styles[_stylesStack[_stylesStack.Count - 1]];
            var baseFontKey = baseStyle.FontKey;

            Font font = null;
            if (fontId.HasValue && (fontId > 0))
            {
                fontId = Mathf.Min(fontId.Value, _fontsSet.Length);
                font = _fontsSet[fontId.Value - 1];
            }

            if (font == null)
            {
                font = baseFontKey.Font;
                fontId = baseStyle.FontId;
            }

            var fontKey = new FontKey(
                font,
                (tagStyle.HasValue ? ApplyStyle(baseFontKey.Style, tagStyle.Value) : baseFontKey.Style),
                (tagSize.HasValue ? tagSize.Value : baseFontKey.BaseSize),
                baseFontKey.ScaleFactor
            );
            var style = new CustomStyle(
                GetFormattedText(),
                fontId.Value,
                fontKey,
                (color.HasValue ? color.Value : baseStyle.Color),
                baseStyle.LineSpacing,
                m_charSpacing
            );

            _stylesStack.Add(_styles.Count);
            _styles.Add(style);
        }

        // parse rich text tag and apply style, if returns true, i points to last tag char ('>')
        bool ParseTag(ref int i)
        {
            if (m_normalText[i] != '<')
            {
                return false;
            }
            if ((i + 2) >= m_normalText.Length)
            {
                return false;
            }
            int endCharId = -1;
            const int maxTagLength = 20;
            for (var j = 1; (j < maxTagLength) && ((j + i) < m_normalText.Length); j += 1)
            {
                if (m_normalText[j + i] == '>')
                {
                    endCharId = j + i;
                    break;
                }
            }
            if (endCharId == -1)
            {
                return false;
            }

            if (m_normalText[i + 1] == '/')
            {
                //Debug.Log("Tag end");
                if (_stylesStack.Count <= 1)
                {
                    Debug.LogWarning("mismatched tags (too many close tags)");
                }
                else
                {
                    _stylesStack.RemoveAt(_stylesStack.Count - 1);
                }
                i = endCharId;
                return true;
            }

            var tagContent = m_normalText.Substring(i + 1, endCharId - (i + 1));
            var tagArr = tagContent.Split(new char[] { '=' }, 2);
            var tagKey = (tagArr.Length > 0 ? tagArr[0] : "");
            var tagValue = (tagArr.Length > 1 ? tagArr[1] : "").Trim();

            //Debug.Log("Tag: " + tagKey + "=" + tagValue);

            bool valid = false;
            if (tagKey == "b")
            {
                ApplyStyleTag(FontStyle.Bold, null);
                valid = true;
            }
            else if (tagKey == "i")
            {
                ApplyStyleTag(FontStyle.Italic, null);
                valid = true;
            }
            else if ((tagKey == "bi") || (tagKey == "ib"))
            {
                ApplyStyleTag(FontStyle.BoldAndItalic, null);
                valid = true;
            }
            else if (tagKey == "size")
            {
                int size;
                if (!int.TryParse(tagValue, out size))
                {
                    //Debug.LogWarning("Invalid size value: " + size);
                    return false;
                }
                ApplyStyleTag(null, size);
                valid = true;
            }
            else if (tagKey == "font")
            {
                int fontId;
                if (!int.TryParse(tagValue, out fontId))
                {
                    //Debug.LogWarning("Invalid font id: " + tagValue);
                    return false;
                }
                if (fontId < 0)
                {
                    fontId = 0;
                }
                if (_fontsSet.Length > 0)
                {
                    ApplyStyleTag(null, null, fontId, null);
                }
                valid = true;
            }
            else if (tagKey == "color")
            {
                if ((tagValue.Length == 7) && (tagValue[0] == '#'))
                {
                    byte r = 0, g = 0, b = 0;
                    bool correct = true;
                    correct = correct && byte.TryParse(tagValue.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out r);
                    correct = correct && byte.TryParse(tagValue.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out g);
                    correct = correct && byte.TryParse(tagValue.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out b);
                    if (correct)
                    {
                        ApplyStyleTag(null, null, null, new Color(r / 255f, g / 255f, b / 255f));
                        valid = true;
                    }
                }
            }

            if (valid)
            {
                i = endCharId;
                return true;
            }

            return false;
        }

        // update m_characters with correct info, characters has style set
        void FillCharactersInfo(bool richText)
        {
            m_characters.Clear();
            _stylesStack.Clear();
            _stylesStack.Add(0);

            for (var i = 0; i < m_normalText.Length; i += 1)
            {
                CharacterInfo charInfo;
                var c = m_normalText[i];
                if (c == '\r')
                {
                    continue;
                }

                var styleId = _stylesStack[_stylesStack.Count - 1];
                if (_styles.Count <= styleId)
                {
                    Debug.LogError("Invalid style id: " + styleId);
                    continue;
                }

                if (c == '\n')
                {
                    m_characters.Add(new CustomCharInfo('\n', i, 0, styleId, true, true));
                    continue;
                }

                if (richText && (c == '<'))
                {
                    if (ParseTag(ref i))
                    {
                        continue;
                    }
                }

                bool popStyle = false;
                if(m_enableCapitals != 0)
                {
                    if(char.IsUpper(c))
                    {
                        popStyle = true;
                        var baseStyle = _styles[_stylesStack[_stylesStack.Count - 1]];
                        ApplyStyleTag(null, baseStyle.FontKey.BaseSize + m_enableCapitals);

                        styleId = _stylesStack[_stylesStack.Count - 1];
                    }
                    
                    c = m_upperText[i];
                }

                var style = _styles[styleId];
                if (style.GetCharacterInfo(c, out charInfo))
                {
                    m_characters.Add(new CustomCharInfo(c, i, Mathf.RoundToInt(charInfo.width) + Mathf.RoundToInt(m_charSpacing), styleId, false, char.IsWhiteSpace(c)));
                }
                else
                {
                    Debug.LogWarning("Failed to get info for character: " + c + " (position in text: " + i + ", code: " + (int)c + ", size: " + style.FontKey.Size);
                }

                if(popStyle)
                {
                    _stylesStack.RemoveAt(_stylesStack.Count - 1);
                }
            }
        }

        // update m_verts with text mesh info created from m_characters
        void FillVertices(bool enableJustify, bool truncateVertical)
        {
            m_verts.Clear();

            if (_tempLineInfo.Count == 0)
            {
                return;
            }

            int offsetX = 0;
            int offsetY = -Mathf.RoundToInt((_areaHeight - _totalHeight) * _alignY);
            //int startOffsetY = offsetY;
            int lineInfoId = 0;
            var currentInfo = _tempLineInfo[0];
            float rest = 0;

            for (var i = 0; i < m_characters.Count; i += 1)
            {
                var customInfo = m_characters[i];
                var c = customInfo.Char;
                var style = _styles[customInfo.StyleId];
                var baseYDiff = currentInfo.MaxBaseY - style.FontMetrics.BaseY;

                if (!customInfo.IsWhiteSpace)
                {
                    var cx = offsetX + currentInfo.OffsetX;
                    var cy = offsetY - style.FontMetrics.BaseY - baseYDiff;

                    if (truncateVertical)
                    {
                        if ((offsetY - currentInfo.MaxHeight) < -_areaHeight)
                        {
                            break;
                        }
                    }

                    if (!truncateVertical || (offsetY <= 1))
                    {
                        CreateCharVertices(c, style, _baseX + cx, _baseY + cy);
                    }
                }

                if (enableJustify && !currentInfo.ForcedLineBreak)
                {
                    var freeSpace = Mathf.Max(0, _areaWidth - currentInfo.Width);
                    var w = currentInfo.Width - currentInfo.LastCharAdvance;
                    if (w > 0)
                    {
                        const float WhiteSpaceWeight = 50;
                        var nonWhiteSpaceWidth = Mathf.Max(0, w - currentInfo.WhiteSpaceWidth);

                        float weightedSum = currentInfo.WhiteSpaceWidth * WhiteSpaceWeight + nonWhiteSpaceWidth;
                        float weightedAdvance = customInfo.Advance * (customInfo.IsWhiteSpace ? WhiteSpaceWeight : 1);

                        var offset = rest + ((weightedAdvance / weightedSum) * freeSpace);
                        var intOffset = Mathf.FloorToInt(offset);
                        rest = offset - intOffset;
                        offsetX += intOffset;
                    }
                }

                offsetX += customInfo.Advance;
                if (customInfo.BreaksAfter)
                {
                    offsetX = 0;
                    offsetY -= currentInfo.MaxHeight;
                    rest = 0;

                    lineInfoId += 1;
                    if (lineInfoId < _tempLineInfo.Count)
                    {
                        currentInfo = _tempLineInfo[lineInfoId];
                    }
                    else
                    {
                        currentInfo = new LineInfo();
                    }
                }
            }
        }

        static void CalcTextAlign(TextAnchor anchor, ref float alignX, ref float alignY, bool enableJustify)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: break;
                case TextAnchor.UpperCenter: alignX = 0.5f; break;
                case TextAnchor.UpperRight: alignX = 1.0f; break;

                case TextAnchor.MiddleLeft: alignY = 0.5f; break;
                case TextAnchor.MiddleCenter: alignX = 0.5f; alignY = 0.5f; break;
                case TextAnchor.MiddleRight: alignX = 1.0f; alignY = 0.5f; break;

                case TextAnchor.LowerLeft: alignY = 1.0f; break;
                case TextAnchor.LowerCenter: alignX = 0.5f; alignY = 1.0f; break;
                case TextAnchor.LowerRight: alignX = 1.0f; alignY = 1.0f; break;
            }

            if (enableJustify)
            {
                alignX = 0;
            }
        }

        // update _tempLineInfo with lines info and calc total height, which is later used for correct aligning
        void CalculateLinesInfo()
        {
            _tempLineInfo.Clear();

            LineInfo info = new LineInfo();
            _maxLineWidth = 0;
            _totalHeight = 0;

            for (var i = 0; i < m_characters.Count; i += 1)
            {
                var customInfo = m_characters[i];
                var c = customInfo.Char;
                var style = _styles[customInfo.StyleId];
                bool forcedBreak = false;

                if (c != '\n')
                {
                    if (!info.Inited)
                    {
                        info.Inited = true;
                        if(style.FontMetrics == null)
                        {
                            Debug.LogError("Empty: " + style.FontKey);
                        }
                        info.MaxBaseY = style.FontMetrics.BaseY;
                        info.MinLineAddedYSpace = Mathf.Max(0, style.LineHeight - style.FontMetrics.BaseLineHeight);
                        info.MaxHeight = style.LineHeight;
                    }
                    else
                    {
                        info.MaxBaseY = Mathf.Max(info.MaxBaseY, style.FontMetrics.BaseY);
                        info.MinLineAddedYSpace = Mathf.Min(info.MinLineAddedYSpace, Mathf.Max(0, style.LineHeight - style.FontMetrics.BaseLineHeight));
                        info.MaxHeight = Mathf.Max(info.MaxHeight, style.LineHeight);
                    }
                }
                else
                {
                    forcedBreak = true;
                }

                info.Width += customInfo.Advance;

                if (customInfo.IsWhiteSpace)
                {
                    info.WhiteSpaceWidth += customInfo.Advance;
                }
                else
                {
                    info.LastCharAdvance = customInfo.Advance;
                }

                bool isLastChar = ((i + 1) == m_characters.Count);
                if (customInfo.BreaksAfter || isLastChar)
                {
                    info.ForcedLineBreak = forcedBreak || isLastChar;

                    // remove white space from end of line
                    for (var j = i; j >= 0; j -= 1)
                    {
                        var jinfo = m_characters[j];
                        if (jinfo.BreaksAfter && (i != j))
                        {
                            break;
                        }
                        if (!jinfo.IsWhiteSpace)
                        {
                            info.LastCharAdvance = jinfo.Advance;
                            break;
                        }

                        info.Width -= jinfo.Advance;
                        info.WhiteSpaceWidth -= jinfo.Advance;
                        jinfo.Advance = 0;
                    }

                    if (!info.Inited)
                    {
                        info.Inited = true;
                        info.MaxBaseY = style.FontMetrics.BaseY;
                        info.MinLineAddedYSpace = Mathf.Max(0, style.LineHeight - style.FontMetrics.BaseLineHeight);
                        info.MaxHeight = style.LineHeight;
                    }

                    _maxLineWidth = Mathf.Max(info.Width, _maxLineWidth);

                    if (isLastChar)
                    {
                        info.MaxHeight -= info.MinLineAddedYSpace;
                        //Debug.Log(info.MinLineAddedYSpace);
                    }

                    _totalHeight += info.MaxHeight;

                    info.OffsetX = Mathf.RoundToInt((_areaWidth - info.Width) * _alignX);
                    _tempLineInfo.Add(info);

                    info = new LineInfo();
                }
            }
        }

        static Vector2 RotatePointAround(Vector2 center, float angle, Vector2 p)
        {
            float s = Mathf.Sin(angle);
            float c = Mathf.Cos(angle);

            // translate point back to origin:
            p -= center;

            // rotate point
            float xnew = p.x * c - p.y * s;
            float ynew = p.x * s + p.y * c;

            // translate point back:
            p.x = xnew + center.x;
            p.y = ynew + center.y;
            return p;
        }

        Vector2 RotateVertex(Vector3 rectCenter, Vector3 pos, Vector3 pivot)
        {
            var angle = rectCenter.x * m_bendStrength;
            return Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * RotatePointAround(rectCenter, angle * Mathf.Deg2Rad, pos);// *pos;
        }

        // create single character vertices
        void CreateCharVertices(char c, CustomStyle style, int x, int y)
        {
            CharacterInfo charInfo;
            if (!style.GetCharacterInfo(c, out charInfo))
            {
                Debug.LogError("Character not found while creating vertices: " + c);
                return;
            }

            var uv = charInfo.uv;
            var uvMin = new Vector2(uv.xMin, uv.yMax);
            var uvMax = new Vector2(uv.xMax, uv.yMin);

            var cx = Mathf.RoundToInt(charInfo.vert.x);
            var cy = Mathf.CeilToInt(charInfo.vert.y);
            var cw = Mathf.RoundToInt(charInfo.vert.width);
            var ch = Mathf.RoundToInt(charInfo.vert.height);

            var vertMin = new Vector2(cx + x, cy + y);
            var vertMax = vertMin + new Vector2(cw, ch);

            var uv00 = new Vector2(uvMin.x, uvMin.y);
            var uv01 = new Vector2(uvMin.x, uvMax.y);
            var uv10 = new Vector2(uvMax.x, uvMin.y);
            var uv11 = new Vector2(uvMax.x, uvMax.y);

            if (charInfo.flipped)
            {
                var newuv00 = uv10;
                var newuv01 = uv00;
                var newuv10 = uv11;
                var newuv11 = uv01;

                uv00 = newuv00;
                uv01 = newuv01;
                uv10 = newuv10;
                uv11 = newuv11;

                SwapY(ref uv00, ref uv10);
                SwapY(ref uv01, ref uv11);
            }
            
            var vert00 = new UIVertex();
            var vert01 = new UIVertex();
            var vert10 = new UIVertex();
            var vert11 = new UIVertex();

            vert00.color = style.Color;
            vert00.uv0 = uv00;
            vert01.color = style.Color;
            vert01.uv0 = uv01;
            vert10.color = style.Color;
            vert10.uv0 = uv10;
            vert11.color = style.Color;
            vert11.uv0 = uv11;

            var pivot = new Vector2(0, m_bendStrength);
            var rectCenter = new Vector2((vertMin.x + vertMax.x) * 0.5f, (vertMin.y + vertMax.y) * 0.5f);

            vert00.position = ((m_bendStrength != 0) ? RotateVertex(rectCenter, new Vector2(vertMin.x, vertMin.y), pivot) : new Vector2(vertMin.x, vertMin.y));
            vert00.uv1 = new Vector2(style.FontId + Mathf.Clamp01(vert00.position.x / _maxLineWidth) * 0.5f, Mathf.Clamp01((vert00.position.y - _baseY) / -_totalHeight));

            vert01.position = ((m_bendStrength != 0) ? RotateVertex(rectCenter, new Vector2(vertMin.x, vertMax.y), pivot) : new Vector2(vertMin.x, vertMax.y));
            vert01.uv1 = new Vector2(style.FontId + Mathf.Clamp01(vert01.position.x / _maxLineWidth) * 0.5f, Mathf.Clamp01((vert01.position.y - _baseY) / -_totalHeight));

            vert10.position = ((m_bendStrength != 0) ? RotateVertex(rectCenter, new Vector2(vertMax.x, vertMin.y), pivot) : new Vector2(vertMax.x, vertMin.y));
            vert10.uv1 = new Vector2(style.FontId + Mathf.Clamp01(vert10.position.x / _maxLineWidth) * 0.5f, Mathf.Clamp01((vert10.position.y - _baseY) / -_totalHeight));

            vert11.position = ((m_bendStrength != 0) ? RotateVertex(rectCenter, new Vector2(vertMax.x, vertMax.y), pivot) : new Vector2(vertMax.x, vertMax.y));
            vert11.uv1 = new Vector2(style.FontId + Mathf.Clamp01(vert11.position.x / _maxLineWidth) * 0.5f, Mathf.Clamp01((vert11.position.y - _baseY) / -_totalHeight));

            m_verts.Add(vert00);
            m_verts.Add(vert01);
            m_verts.Add(vert11);
            m_verts.Add(vert10);
        }
    }
}