using UnityEngine;

namespace MarkerDataTypes
{
    [CreateAssetMenu(fileName = "LineMarkerGroup", menuName = "New  Line Marker Group")]
    public class LineMarkerGroup : ScriptableObject
    {
        public string[] MarkerNames = new[] { "LineRenderer: LFHD - LBHD", "LineRenderer: LFHD - RBHD" };
    }
}