using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExoskeletonInteraction;

public class Synchronization : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DataPlayer dataPlayer;
    [SerializeField] private ExoskeletonLineRenderer control, user;

    [SerializeField] private float defaultStrictness = 6f;

    private float strictness;
    private bool synch;

    // Start is called before the first frame update
    void Start()
    {
        strictness = defaultStrictness;
    }

    // Update is called once per frame
    public void SetActive(bool active)
    {
        synch = active;
    }
    
    void Update()
    {
        if (!synch)
        {
            return;
        }
        bool stop;
        if (control.ConnectionsByName == null || user.ConnectionsByName == null)
            return;

        stop = true;
        foreach (var controlPair in control.ConnectionsByName)
        {
            
            var controlName = controlPair.Key;
            var controlConnection = controlPair.Value;

            var userConnection = user.ConnectionsByName[controlName];
            var userLR = user.LrsByConnection[userConnection];

            var controlDispl = controlConnection.Tail.localPosition - controlConnection.Head.localPosition;
            var userDispl = userConnection.Tail.localPosition - userConnection.Head.localPosition;

            var dot = Vector3.Dot(controlDispl.normalized, userDispl.normalized);
            if(dot < strictness)
            {
                stop = false;
            }
        }
        dataPlayer.SetPlay(stop);
    }
}
