// It changes the visualization mode from exoskeleton to humanoid and vice versa
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ControlDataHolder))]
public class VisualizationMode : MonoBehaviour
{
    public GameObject exoskeletonView;
    public GameObject humanoidView;

    public void SetView(int option)
    {
        GetComponent<ControlDataHolder>().SetViewOption(option);

        if (option == 0)
        {
            humanoidView.SetActive(true);
            exoskeletonView.SetActive(false);
        }
        else if(option == 1)
        {
            humanoidView.SetActive(false);
            exoskeletonView.SetActive(true);
        }
    }
}
