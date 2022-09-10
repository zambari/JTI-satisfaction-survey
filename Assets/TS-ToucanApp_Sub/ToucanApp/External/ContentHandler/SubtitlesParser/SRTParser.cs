using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public class SRTParser
    {
        private enum eReadState
        {
            Index,
            Time,
            Text
        }

        public class SubtitleBlock
        {
            private static SubtitleBlock blank;
            public static SubtitleBlock Blank
            {
                get { return blank ?? (blank = new SubtitleBlock(0, 0, 0, string.Empty)); }
            }

            public int Index { get; private set; }
            public double Length { get; private set; }
            public double From { get; private set; }
            public double To { get; private set; }
            public string Text { get; private set; }

            public SubtitleBlock(int index, double from, double to, string text)
            {
                this.Index = index;
                this.From = from;
                this.To = to;
                this.Length = to - from;
                this.Text = text;
            }
        }

        public static string[] SplitLines(string text)
        {
            return Regex.Split(text, "\r\n|\r|\n");
        }

        public static List<SubtitleBlock> Load(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("Subtitle file is null");
                return null;
            }

            var lines = SplitLines(content);
            var currentState = eReadState.Index;
            var subs = new List<SubtitleBlock>();

            int currentIndex = 0;
            double currentFrom = 0, currentTo = 0;
            var currentText = string.Empty;
            for (var l = 0; l < lines.Length; l++)
            {
                var line = lines[l];

                switch (currentState)
                {
                    case eReadState.Index:

                        int index;
                        if (Int32.TryParse(line, out index))
                        {
                            currentIndex = index;
                            currentState = eReadState.Time;
                        }
                        break;

                    case eReadState.Time:

                        line = line.Replace(',', '.');
                        var parts = line.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);

                        // Parse the timestamps
                        if (parts.Length == 2)
                        {
                            TimeSpan fromTime;
                            if (TimeSpan.TryParse(parts[0], out fromTime))
                            {
                                TimeSpan toTime;
                                if (TimeSpan.TryParse(parts[1], out toTime))
                                {
                                    currentFrom = fromTime.TotalSeconds;
                                    currentTo = toTime.TotalSeconds;
                                    currentState = eReadState.Text;
                                }
                            }
                        }
                        break;

                    case eReadState.Text:

                        if (currentText != string.Empty && line != string.Empty)
                            currentText += "\r\n";

                        currentText += line;

                        // When we hit an empty line, consider it the end of the text
                        if (string.IsNullOrEmpty(line) || l == lines.Length - 1)
                        {
                            // Create the SubtitleBlock with the data we've aquired 
                            subs.Add(new SubtitleBlock(currentIndex, currentFrom, currentTo, currentText));

                            // Reset stuff so we can start again for the next block
                            currentText = string.Empty;
                            currentState = eReadState.Index;
                        }
                        break;
                }
            }

            return subs;
        }
    }
}
