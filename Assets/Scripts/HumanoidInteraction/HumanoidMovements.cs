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

        [SerializeField] private MarkerGroup headMarkers;
        [SerializeField] private MarkerGroup hipsMarkers;
        [SerializeField] private MarkerGroup leftWristMarkers;
        [SerializeField] private MarkerGroup rightWristMarkers;
        [SerializeField] private MarkerGroup leftElbowMarkers;
        [SerializeField] private MarkerGroup rightElbowMarkers;
        [SerializeField] private MarkerGroup leftArmMarkers;
        [SerializeField] private MarkerGroup rightArmMarkers;
        [SerializeField] private MarkerGroup leftKneeMarkers;
        [SerializeField] private MarkerGroup rightKneeMarkers;
        [SerializeField] private MarkerGroup leftFootMarkers;
        [SerializeField] private MarkerGroup rightFootMarkers;


        private Dictionary<GameObject, MarkerGroup> matches;
        [SerializeField] private MarkerGroup markerGroup;
        [SerializeField] private Transform referenceParent;

        private void Start()
        {
            // set the matches between points and body parts
            matches = new Dictionary<GameObject, MarkerGroup>();
            SetMatches();
        }

        private void Update()
        {
            CenterPosition();
            GameObject key;
            MarkerGroup markers;
            foreach (var instance in matches)
            {
                int count = 0;
                key = instance.Key;
                matches.TryGetValue(key, out markers);
                foreach (var markerName in markers.MarkerNames)
                {
                var t = referenceParent.transform.Find(markerName);
                if (!t)
                    return;
                else if (t.localPosition.x == 0 && t.localPosition.y == 0 && t.localPosition.z == 0)
                    count+=1;
                }
                if (count == markers.MarkerNames.Length)
                {
                    return;
                }
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
            matches.Add(head, headMarkers);

            // Wrists
            matches.Add(leftWrist, leftWristMarkers);
            matches.Add(rightWrist, rightWristMarkers);

            // Elbows
            matches.Add(leftElbow, leftElbowMarkers);
            matches.Add(rightElbow, rightElbowMarkers);

            // Arms
            matches.Add(leftArm, leftArmMarkers);
            matches.Add(rightArm, rightArmMarkers);

            // Hips
            matches.Add(hips, hipsMarkers);

            // Knees
            matches.Add(leftKnee, leftKneeMarkers);
            matches.Add(rightKnee, rightKneeMarkers);

            // Feet
            matches.Add(leftFoot, leftFootMarkers);
            matches.Add(rightFoot, rightFootMarkers);
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
            GameObject key;
            MarkerGroup markers;
            foreach (var instance in matches)
            {
                key = instance.Key;
                positions.Clear();
                if (!matches.TryGetValue(key, out markers))
                {
                    print("ERROR. No value found in dictionary");
                    continue;
                }
                foreach (string markerName in markers.MarkerNames)
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
