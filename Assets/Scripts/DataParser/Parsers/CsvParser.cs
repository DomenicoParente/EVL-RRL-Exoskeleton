﻿// This was written particularly to interpret the format of the CSV / ASCII files
// (blance_T1.csv, blance_T2.csv, sqaut_1_2.csv, sqaut_1_3.csv [sic] in the data directory)
// With one section of additional data
// If another format emerges, use the interface and swap out this class.

using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using Markers;
using UnityEngine;

namespace DataParser.Parsers
{
    public class CsvParser : IParser
    {
        public ParserInfo ParseFileAt(string loc)
        {
            var titles  = new List<string>();
            var markers = new Dictionary<string, IMarkerData>();

            var emptyCount = 0;
            var frameCount = 0;

            var reader = new StreamReader(loc);

            // Get through junk & read marker titles
            while(!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if(line.Length < 2)
                {
                    ++emptyCount;
                    continue;
                }

                if(emptyCount < 2)
                    continue;

                reader.ReadLine();
                line = reader.ReadLine();

                PopulateTitles(titles, line);

                reader.ReadLine();
                break;
            }

            // Read marker positions
            while(!reader.EndOfStream)
            {
                var line     = reader.ReadLine();
                var numSplit = line.Split(',');

                int frame = -1;
                if(int.TryParse(numSplit[0], out var data))
                    frame = data;

                if(frame < 1)
                    continue;

                // Probably will remain unused, but titled just in case
                int subFrame = -1;
                if(int.TryParse(numSplit[1], out var data2))
                    subFrame = data2;

                frameCount = Mathf.Max(frame, frameCount);
            
                ParsePositionalData(titles, numSplit, markers, frame);
            }

            return new ParserInfo()
            {
                Titles     = titles,
                Markers    = markers,
                FrameCount = frameCount
            };
        }

        private static void ParsePositionalData(List<string> titles, string[] numSplit, Dictionary<string, IMarkerData> markers, int frame)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            for (int i = 0; i < titles.Count; i++)
            {
                var validX = float.TryParse(numSplit[2 + (i * 3)], NumberStyles.Any, culture, out var x);
                var validY = float.TryParse(numSplit[3 + (i * 3)], NumberStyles.Any, culture, out var y);
                var validZ = float.TryParse(numSplit[4 + (i * 3)], NumberStyles.Any, culture, out var z);

                if(!validX || !validY || !validZ)
                    continue;

                var markerTitle = titles[i];
                if(!markers.ContainsKey(markerTitle))
                    markers.Add(markerTitle, new MarkerDataDictionary(markerTitle));

                // x, z, y in order to match the flipped coordinate systems between VICON and Unity
                var frameData = new FrameData() { Position = new Vector3(x, z, y) };
                markers[markerTitle].Add(frame, frameData);
            }
        }

        private static void PopulateTitles(List<string> titles, string titlesLine)
        {
            var titleSplit = titlesLine.Split(',');

            foreach(var s in titleSplit)
            {
                if(s.Length < 2)
                    continue;

                titles.Add(s.Split(':')[1]);
            }
        }
    }
}