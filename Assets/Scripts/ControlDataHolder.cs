// Holds the data loaded in from the readers
// Updates the points based on commands from other scripts
// Forwards the connections for ExoskeletonLineRenderer

using System;
using System.Collections.Generic;
using DataParser;
using DataParser.Parsers;
using ExoskeletonInteraction;
using MarkerDataTypes;
using Markers;
using UI;
using UnityEngine;
using Utilities;

public class ControlDataHolder : MonoBehaviour
{
    /// <summary> Upon a successful load, provides number of frames. </summary>
    public event Action<int> OnSuccessfulLoad;

    [Header("Dependencies")]
    [SerializeField] private DataPlayer              dataPlayer;
    [SerializeField] private FileOpener              fileOpener;
    [SerializeField] private ExoskeletonLineRenderer lr;
    [SerializeField] private Transform               dataVisualParent;
    

    [SerializeField] private Material pointMaterial;
    
    [Header("Visual Attributes")]
    [SerializeField] private float pointSize = 0.035f;
    [SerializeField] private float bodyScale = 0.001f;
    [SerializeField] private int   viewOption;

    private readonly IParser parser = new CsvParser();
    
    private Dictionary<string, IMarkerData>     markers; // LFHD, RFHD, etc
    private Dictionary<IMarkerData, GameObject> points;
    
    private int  frameCount;
    private bool ready;

    private void OnEnable()
    {
        ready = false;

        dataPlayer.OnUpdatedFrame  += UpdatePositions;
        fileOpener.OnNewFileOpened += LoadDataset;
        
        markers = new Dictionary<string, IMarkerData>();
        points  = new Dictionary<IMarkerData, GameObject>();
    }

    private void OnDisable()
    {
        dataPlayer.OnUpdatedFrame  -= UpdatePositions;
        fileOpener.OnNewFileOpened -= LoadDataset;
    }

    private void LoadDataset(string directory)
    {
        ready = false;
        CleanOldPoints();

        var info   = parser.ParseFileAt(directory);
        // var titles = info.Titles;
        markers    = info.Markers;
        frameCount = info.FrameCount;

        CreatePoints();

        ready = true;
        OnSuccessfulLoad?.Invoke(frameCount);

        GetConnectsTransforms();
    }

    private void GetConnectsTransforms()
    {
        var connects   = ConnectionsParser.GetConnects();
        var transforms = new List<TransformConnection>();

        foreach(var (item1, item2) in connects)
        {
            var head = points[markers[item1]].transform;
            var tail = points[markers[item2]].transform;
            transforms.Add(new TransformConnection(head, tail));
        }

        lr.SetTerminals(transforms);
    }

    private void CleanOldPoints()
    {
        foreach(var kv in points)
            Destroy(kv.Value);

        markers.Clear();
        points.Clear();
    }

    public void UpdatePositions(int frameNum)
    {
        if(!ready)
            return;

        foreach(var marker in markers)
        {
            var markerData = marker.Value;
            if(!markerData.TryGetPositionAtFrame(frameNum, out var pos))
                continue;

            var pt = points[markerData];
            if (viewOption == 1)
            {
                pt.SetActive(true);

            }

            else if (viewOption == 0)
            {
                pt.SetActive(false);
            }
            pt.transform.localPosition = pos * bodyScale;
        }
    }

    private void CreatePoints()
    {
        foreach(var marker in markers)
        {
            var go = PointCreation.CreatePoint(dataVisualParent, pointSize, pointMaterial, marker.Value.GetName());
            go.SetActive(false); // in case of empty marker
            points.Add(marker.Value, go);
        }
    }

    public void SetViewOption(int option)
    {
        // The 0 value corresponds to humanoid model visualization
        // The 1 value corresponds to exoskeleton visualization
        viewOption = option;
    }

    public bool GetReady()
    {
        return ready;
    }

    public Vector3 GetPositionByName(string pointName)
    {
        bool hasValue = markers.TryGetValue( pointName, out var marker);
        if (hasValue)
        {
            bool hasPoint = points.TryGetValue(marker, out var point);
            if (hasPoint)
            {
                return point.transform.localPosition;
            }
            else
            {
                print("Error. Point not found");
            }
        }
        else
        {
            print("Error. Maker not found");
        }
        return Vector3.zero;
    }
}