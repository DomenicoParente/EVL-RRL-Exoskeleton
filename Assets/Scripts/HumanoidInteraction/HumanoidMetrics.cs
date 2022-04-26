// Colors the markers on the humanoid model based on severity of off angle positions
// Sensitivity is controlled by strictness (exposed in UI)
// Creates and controls the corrective action arrows as well

using System.Collections.Generic;
using MarkerDataTypes;
using UnityEngine;

namespace HumanoidInteraction
{
    public class HumanoidMetrics : MonoBehaviour
    {
        [SerializeField] private float defaultStrictness = 6f;

        [Header("Marker Renderer")]
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color astrayColor = Color.red;

        [Header("Arrows")]
        [SerializeField] private GameObject arrowPrefab;
        [Tooltip("Modified at runtime by the arrow button\n" +
                 "Enter pairs of connected markers for arrow feedback.")]
        [SerializeField] private List<MarkerSet> arrowMarkers;

        [Tooltip("t values under this number will draw an arrow")]
        [SerializeField, Range(0f, 1f)] private float tThreshold = 0.51f;

        [SerializeField] private Transform referenceControl;
        [SerializeField] private Transform referenceUser;

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

        private List<Transform> arrowTransforms;
        private Dictionary<GameObject, string> matches;

        private float strictness;

        private void Start()
        {
            // set the matches between points and body marks
            matches = new Dictionary<GameObject, string>();
            SetMatches();
            strictness = defaultStrictness;
            arrowTransforms = new List<Transform>();
        }

        public void AdjustStrictness(string input)
            => strictness = float.TryParse(input, out var value) ? value : defaultStrictness;

        public void SetMarkerSets(List<MarkerSet> newSets)
            => arrowMarkers = newSets;

        private void SetMatches()
        {

            // Head
            matches.Add(head, "LineRenderer: LFHD - RFHD," +
                "LineRenderer: LFHD - LBHD," +
                "LineRenderer: LFHD - RBHD," +
                "LineRenderer: RFHD - LBHD," +
                "LineRenderer: RFHD - RBHD," +
                "LineRenderer: LBHD - RBHD");

            // Wrists
            matches.Add(leftWrist, "LineRenderer: LELB - LWRB");
            matches.Add(rightWrist, "LineRenderer: RELB - RWRB");

            // Elbows
            matches.Add(leftElbow, "LineRenderer: LSHO - LELB");
            matches.Add(rightElbow, "LineRenderer: RSHO - RELB");

            // Arms
            matches.Add(leftArm, "LineRenderer: CLAV - RSHO");
            matches.Add(rightArm, "LineRenderer: CLAV - LSHO");

            // Hips
            matches.Add(hips, "LineRenderer: LASI - RPSI,LineRenderer: RASI - LPSI");

            // Knees
            matches.Add(leftKnee, "LineRenderer: LASI - LKNE,LineRenderer: LPSI - LKNE");
            matches.Add(rightKnee, "LineRenderer: RASI - RKNE,LineRenderer: RPSI - RKNE");

            // Feet
            matches.Add(leftFoot, "LineRenderer: LKNE - LANK");
            matches.Add(rightFoot, "LineRenderer: RKNE - RANK");
        }

        private void Update()
        {
            List<string> values = new List<string>();
            GameObject key;
            foreach (var instance in matches)
            {
                key = instance.Key;
                matches.TryGetValue(key, out var val);

                foreach (string marker in val.Split(","))
                    values.Add(marker);
            }
            foreach (string markerName in values)
            {
                var t1 = referenceControl.transform.Find(markerName);
                var t2 = referenceUser.transform.Find(markerName);

                if (!t1 || !t2)
                    return;
            }
            // var numMarkers = arrowMarkers.Count;
            // if (arrowTransforms.Count != numMarkers)
            //    CreateNewArrows(numMarkers);

            UpdateVisualMetrics();
        }

        private void CreateNewArrows(int numMarkers)
        {
            foreach (var arrowTransform in arrowTransforms)
                Destroy(arrowTransform.gameObject);

            arrowTransforms.Clear();

            for (int i = 0; i < numMarkers; ++i)
            {
                var arrow = Instantiate(arrowPrefab).transform;
                arrow.SetParent(transform, true);
                arrowTransforms.Add(arrow);
            }
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

        private void UpdateVisualMetrics()
        {
            List<string> lines = new List<string>();
            List<Vector3> controlLines = new List<Vector3>();
            List<Vector3> userLines = new List<Vector3>();
            GameObject marker;
            foreach (var instance in matches)
            {
                marker = instance.Key;
                matches.TryGetValue(marker, out var val);
                lines.Clear();
                controlLines.Clear();
                userLines.Clear();
                foreach (string lineName in val.Split(","))
                    lines.Add(lineName);
                foreach (string lineName in lines)
                {
                    LineRenderer controlLine = (LineRenderer) referenceControl.transform.Find(lineName).gameObject.GetComponent(typeof(LineRenderer));
                    LineRenderer userLine = (LineRenderer) referenceUser.transform.Find(lineName).gameObject.GetComponent(typeof(LineRenderer));
                    if (controlLine == null || userLine == null)
                        return;

                    var controlDispl = controlLine.GetPosition(1) - controlLine.GetPosition(0);
                    var userDispl = userLine.GetPosition(1) - userLine.GetPosition(0);
                    controlLines.Add(controlDispl);
                    userLines.Add(userDispl);
                }
                Vector3 avgControl = GetMeanVector(controlLines);
                Vector3 avgUser = GetMeanVector(userLines);
                var dot = Vector3.Dot(avgControl.normalized, avgUser.normalized);
                // Update mark renderer color
                var t = Mathf.Pow(dot, strictness);
                var color = Color.Lerp(astrayColor, goodColor, t);
                var markerRenderer = marker.GetComponent<Renderer>();
                markerRenderer.material.SetColor("_BaseColor", color);
                markerRenderer.material.SetColor("_EmissionColor", color);
                Light light = (Light) marker.transform.GetChild(0).GetComponent(typeof(Light));
                light.color = color;

                // Update the arrow of a particular body part
                //UpdateArrow(t, avgControl, avgUser);

            }
        }

        private void UpdateArrow(float t, Vector3 avgControl, Vector3 avgUser, GameObject arrow)
        {
             var dif = avgControl - avgUser;
             if (dif.sqrMagnitude == 0f)
                 return;
        }
    }
}
