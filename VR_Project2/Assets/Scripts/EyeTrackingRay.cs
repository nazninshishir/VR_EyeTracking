using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer))]

public class EyeTrackingRay : MonoBehaviour
{
    [SerializeField]
    private float rayDistance = 1.0f;

    [SerializeField]
    private float rayWidth = 0.01f;

    [SerializeField]
    private LayerMask layersToInclude;

    [SerializeField]
    private Color rayColorDefaultState = Color.clear;

    [SerializeField]
    private Color rayColorHoverState = Color.clear;

    [SerializeField]
    private OVRHand handUsedForPinchSelection;

    [SerializeField]
    private OVRHand mockhandUsedForPinchSelection;

    private bool intercepting;

    private bool allowPinchSelection;

    private LineRenderer lineRenderer;

    private Dictionary<int, EyeInteractable> interactables = new Dictionary<int, EyeInteractable>();

    private EyeInteractable lastEyeInteractable;

    private float grabTime = 0f;

    private Vector3 previousPosition; // Variable to store the previous position of the object
    private bool isObjectMoved = false;

    private float sceneStartTime = 0f;







    void Start()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
        lineRenderer = GetComponent<LineRenderer>();
        allowPinchSelection = handUsedForPinchSelection != null;
        SetupRay();
        previousPosition = Vector3.zero;



    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        // Reset time when a new scene is loaded
        grabTime = 0f;
        
        isObjectMoved = false;
        sceneStartTime = Time.time;
    }




    void SetupRay()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = Color.clear;
        lineRenderer.endColor = Color.clear;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, transform.position.z + rayDistance));
    }

    private void Update()
    {
        lineRenderer.enabled = !IsPinching();

        if (!intercepting)
        {
            lineRenderer.startColor = lineRenderer.endColor = Color.clear;
            lineRenderer.SetPosition(1, new Vector3(0, 0, transform.position.z + rayDistance));
            
        }

        SelectionStarted();
    }
    private void SelectionStarted()
    {
        if (IsPinching())
        {
            if (lastEyeInteractable != null)
            {
                // Move the selected object to the hand's position
                Rigidbody interactableRigidbody = lastEyeInteractable.GetComponent<Rigidbody>();

                if (interactableRigidbody != null)
                {
                    Vector3 targetPosition = (handUsedForPinchSelection?.IsTracked ?? false)
                        ? (handUsedForPinchSelection.grabPoint != null ? handUsedForPinchSelection.grabPoint.position : handUsedForPinchSelection.transform.position)
                        : transform.position;

                    // Calculate the direction towards the hand
                    Vector3 directionToHand = (targetPosition - interactableRigidbody.position);

                    // Translate the object towards the hand
                    interactableRigidbody.transform.Translate(directionToHand, Space.World);

                    

                    if (!isObjectMoved && interactableRigidbody.position != previousPosition)
                    {
                        grabTime = Time.time;
                        RecordGrabbedObject(lastEyeInteractable.gameObject.name, grabTime);
                        isObjectMoved = true; // Set the flag to indicate the object has moved
                    }
                    previousPosition = interactableRigidbody.position;
                }

                Transform anchorTransform = (handUsedForPinchSelection != null && handUsedForPinchSelection.IsTracked)
                    ? handUsedForPinchSelection.grabPoint
                    : transform;

                lastEyeInteractable.Select(true, anchorTransform);
                
            }
        }
        else
        {
            isObjectMoved = false;
            lastEyeInteractable?.Select(false);
            
        }
    }


    void FixedUpdate()
    {
        if (IsPinching()) return;

        Vector3 rayDirection = transform.TransformDirection(Vector3.forward) * rayDistance;
        intercepting = Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, Mathf.Infinity, layersToInclude);

        if (intercepting)
        {
            EyeInteractable eyeInteractable = hit.transform.GetComponent<EyeInteractable>();

            if (eyeInteractable != null && lastEyeInteractable != eyeInteractable)
            {
                lastEyeInteractable?.Hover(false); // Reset the last hovered object
                lastEyeInteractable = eyeInteractable;
                lastEyeInteractable.Hover(true);  // Set the new object as hovered
            }

            var toLocalSpace = transform.InverseTransformPoint(hit.point);
            lineRenderer.SetPosition(1, new Vector3(0, 0, toLocalSpace.z));
        }
    }

    private void RecordGrabbedObject(string objectName, float grabTime)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // Format the data as a string
        string data = sceneName + "," + sceneStartTime.ToString("F2") + "," + objectName + "," + grabTime.ToString("F2");

        

        // Define the file path outside of the project directory
        string filePath = Path.Combine(Application.persistentDataPath, sceneName + "_grabbed_objects.txt");

        // Check if the file exists, if not, create it and add a header
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = File.CreateText(filePath))
            {
                writer.WriteLine("Scene Name,Scene Start Time (ms), Object Name,Grab Time (ms)");
            }
        }

        // Append the data to the file
        using (StreamWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine(data);
        }

        Debug.Log("Object grabbed: " + objectName + " at time: " + grabTime.ToString("F2") + " milliseconds. Data saved to file.");
    }
    private void OnHoverEnded()
    {
        if (lastEyeInteractable != null)
        {
            lastEyeInteractable.Hover(false);
            lastEyeInteractable = null;
        }
    }
    private void OnDestroy() => interactables.Clear();

    private bool IsPinching() => (allowPinchSelection && handUsedForPinchSelection.GetFingerIsPinching(OVRHand.HandFinger.Index) && handUsedForPinchSelection.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.5f);
}