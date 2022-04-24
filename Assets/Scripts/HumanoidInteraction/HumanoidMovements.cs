// Handles the movements of model's bones. The movements are related to the movements of specific points.
//For example to move the head are used RFHD,LFHD,LBHD,RBHD points as reference.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkerDataTypes;

namespace HumanoidInteraction
{
    public class HumanoidMovements : MonoBehaviour
    {

      
        public GameObject head;
        public GameObject hips;
        public GameObject leftWrist;
        public GameObject rightWrist;
        public GameObject leftElbow;
        public GameObject rightElbow;
        public GameObject leftArm;
        public GameObject rightArm;
        public GameObject leftKnee;
        public GameObject rightKnee;
        public GameObject leftFoot;
        public GameObject rightFoot;

        private Dictionary<GameObject, string> matches;
        [SerializeField] private MarkerGroup markerGroup;
        [SerializeField] private Transform referenceParent;

        private void Start()
        {
            // set the matches between points and body parts
            matches = new Dictionary<GameObject, string>();
            SetMatches();
        }

        private void Update()
        {
            CenterPosition();
            GameObject key;
            int count = 0;
            List<string> values = new List<string>();
            foreach (var instance in matches)
            {
                key = instance.Key;
                matches.TryGetValue(key, out var val);
            
                foreach (string marker in val.Split(","))
                    values.Add(marker);
            }
            foreach (string markerName in values)
            {
                var t = referenceParent.transform.Find(markerName);
                if (!t)
                    return;
                else if (t.localPosition.x == 0 && t.localPosition.y == 0 && t.localPosition.z == 0)
                    count+=1;
            }
            if (count == values.Count)
            {
                return;
            }
            UpdateMovements();

        }

        private void CenterPosition()
        {
            var transforms = new List<Transform>();
            foreach (var markerName in markerGroup.MarkerNames)
            {
                var t = referenceParent.transform.Find(markerName);
                if (!t)
                    return;

                transforms.Add(t.transform);
            }
            var avgPos = Vector3.zero;
            foreach (var t in transforms)
                avgPos += t.localPosition;

            avgPos /= transforms.Count;

            referenceParent.transform.localPosition = new Vector3(-avgPos.x, 0f, -avgPos.z);
        }

        private void SetMatches()
        {

            // Head
            matches.Add(head, "RFHD,LFHD,LBHD,RBHD");

            // Wrists
            matches.Add(leftWrist, "LWRA,LWRB");
            matches.Add(rightWrist, "RWRA,RWRB");

            // Elbows
            matches.Add(leftElbow, "LELB,LUPA");
            matches.Add(rightElbow, "RELB,RUPA");

            // Arms
            matches.Add(leftArm, "RSHO");
            matches.Add(rightArm, "LSHO");

            // Hips
            matches.Add(hips, "RASI,LASI,RPSI,LPSI");

            // Knees
            matches.Add(leftKnee, "LKNE");
            matches.Add(rightKnee, "RKNE");

            // Feet
            matches.Add(leftFoot, "LANK");
            matches.Add(rightFoot, "RANK");
        }

        private Vector3 GetMeanVector(List<Vector3> positions)
        {
            if (positions.Count == 0)
                return Vector3.zero;

            float x = 0f;
            float y = 0f;
            float z = 0f;

            foreach (Vector3 pos in positions)
            {
                x += pos.x;
                y += pos.y;
                z += pos.z;
            }
            return new Vector3(x / positions.Count, y / positions.Count, z / positions.Count);
        }

        private void UpdateMovements()
        {
            // update the positions of all body parts
            List<Vector3> positions = new List<Vector3>();
            List<string> values = new List<string>();
            GameObject key;
            foreach (var instance in matches)
            {
                key = instance.Key;
                positions.Clear();
                values.Clear();
                if (!matches.TryGetValue(key, out var val))
                {
                    print("ERROR. No value found in dictionary");
                    continue;
                }
                foreach (string marker in val.Split(","))
                    values.Add(marker);
                foreach (string markerName in values)
                {
                    var t = referenceParent.transform.Find(markerName);
                    if (!t)
                        return;
                    positions.Add(t.transform.position);
                }
                Vector3 avg = GetMeanVector(positions);
                key.transform.position = new Vector3(avg.x, avg.y, avg.z);
            }

        }
    }
}
