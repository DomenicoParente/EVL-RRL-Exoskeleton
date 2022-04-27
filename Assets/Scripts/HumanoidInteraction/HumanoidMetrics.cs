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

        [SerializeField] private LineMarkerGroup headMarkers;
        [SerializeField] private LineMarkerGroup hipsMarkers;
        [SerializeField] private LineMarkerGroup leftWristMarkers;
        [SerializeField] private LineMarkerGroup rightWristMarkers;
        [SerializeField] private LineMarkerGroup leftElbowMarkers;
        [SerializeField] private LineMarkerGroup rightElbowMarkers;
        [SerializeField] private LineMarkerGroup leftArmMarkers;
        [SerializeField] private LineMarkerGroup rightArmMarkers;
        [SerializeField] private LineMarkerGroup leftKneeMarkers;
        [SerializeField] private LineMarkerGroup rightKneeMarkers;
        [SerializeField] private LineMarkerGroup leftFootMarkers;
        [SerializeField] private LineMarkerGroup rightFootMarkers;

        private List<Transform> arrowTransforms;
        private Dictionary<GameObject, LineMarkerGroup> matches;

        private float strictness;

        private void Start()
        {
            // set the matches between points and body marks
            matches = new Dictionary<GameObject, LineMarkerGroup>();
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
            referenceControl.gameObject.GetComponent<ExoskeletonInteraction.ExoskeletonLineRenderer>().UpdateLines();
            referenceUser.gameObject.GetComponent<ExoskeletonInteraction.ExoskeletonLineRenderer>().UpdateLines();

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

        private void Update()
        {


            List<string> values = new List<string>();
            GameObject key;
            LineMarkerGroup markers;
            foreach (var instance in matches)
            {
                key = instance.Key;
                matches.TryGetValue(key, out markers);
                foreach (var markerName in markers.MarkerNames)
                {
                    var t1 = referenceControl.transform.Find(markerName);
                    var t2 = referenceUser.transform.Find(markerName);

                    if (!t1 || !t2)
                        return;
                }
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
            List<Vector3> controlLines = new List<Vector3>();
            List<Vector3> userLines = new List<Vector3>();
            GameObject marker;
            LineMarkerGroup lines;
            foreach (var instance in matches)
            {
                marker = instance.Key;
                matches.TryGetValue(marker, out lines);
                controlLines.Clear();
                userLines.Clear();

                foreach (string lineName in lines.MarkerNames)
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
