using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ToucanApp.Data
{
    public class SubtitleContent : AbstractContent<SubtitleData>
    {
        // Array instead of list for better readability in editor
        [System.Serializable]
        public class SubtitlesEvent : UnityEvent<Subtitle[]> { }

        [System.Serializable]
        public class Subtitle
        {
            [SerializeField]
            private int index = 0;
            public int Index
            {
                get { return index; }
                set { index = value; }
            }

            [SerializeField]
            private float timeFrom = 0;
            public float TimeFrom
            {
                get { return timeFrom; }
                set { timeFrom = value; }
            }

            [SerializeField]
            private float timeTo = 0;
            public float TimeTo
            {
                get { return timeTo; }
                set { timeTo = value; }
            }

            [SerializeField]
            private List<string> textLines = new List<string>();
            public List<string> TextLines
            {
                get { return textLines; }
                set { textLines = value; }
            }
        }

        [SerializeField]
        private List<Subtitle> subtitles = new List<Subtitle>();

        public SubtitlesEvent OnSubtitlesLoaded = new SubtitlesEvent();

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);
            ContentHandler.Resources.Load(Data, OnLoadResource, languageId);
        }

        #region IResource implementation

        public void OnLoadResource(ResourceInfo resource)
        {
            if (resource.www == null || resource.www.downloadHandler == null)
                return;

            string rawFileText = resource.www.downloadHandler.text;
            var parsedSubtitles = SRTParser.Load(rawFileText);

            subtitles.Clear();

            foreach (var ps in parsedSubtitles)
            {
                var sub = new Subtitle();

                sub.Index = ps.Index;
                sub.TimeFrom = (float)ps.From;
                sub.TimeTo = (float)ps.To;
                sub.TextLines = SRTParser.SplitLines(ps.Text).ToList();

                subtitles.Add(sub);
            }

            OnSubtitlesLoaded.Invoke(subtitles.ToArray());
        }

        #endregion
    }
}
