// Allows user to update the exoskeleton/humanoid displacement manually

using UnityEngine;

public class Displacement : MonoBehaviour
{
    [SerializeField] private Vector3 displacement;
    public GameObject secondObject;

    private Vector3 startingPosition;
    private Vector3 secondStartingPosition;

    private void Start()
    {
        startingPosition = transform.localPosition;
        secondStartingPosition = secondObject.transform.localPosition;
    }

    public void SetDisplacement(Vector3 newDisplacement)
    {
        displacement = newDisplacement;
        UpdateDisplacement();
    }

    private void UpdateDisplacement() 
    {
        transform.localPosition = startingPosition + displacement;
        secondObject.transform.localPosition = secondStartingPosition + displacement;
    }
 
}
