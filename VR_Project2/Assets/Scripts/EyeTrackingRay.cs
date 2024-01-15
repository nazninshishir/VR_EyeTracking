using System.Collections.Generic;
using UnityEngine;

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
    private Color rayColorDefaultState = Color.yellow;

    [SerializeField]
    private Color rayColorHoverState = Color.red;

    [SerializeField]
    private OVRHand handUsedForPinchSelection;

    [SerializeField]
    private bool mockhandUsedForPinchSelection;

    private bool intercepting;

    private bool allowPinchSelection;

    private LineRenderer lineRenderer;

    private Dictionary<int, EyeInteractable> interactables = new Dictionary<int, EyeInteractable>();

    private EyeInteractable lastEyeInteractable;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        allowPinchSelection = handUsedForPinchSelection != null;
        SetupRay();


    }

    void SetupRay()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = rayColorDefaultState;
        lineRenderer.endColor = rayColorDefaultState;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, transform.position.z + rayDistance));
    }

    private void Update()
    {
        lineRenderer.enabled = !IsPinching();

        SelectionStarted();

        if (!intercepting)
        {
            lineRenderer.startColor = lineRenderer.endColor = rayColorDefaultState;
            lineRenderer.SetPosition(1, new Vector3(0, 0, transform.position.z + rayDistance));
            OnHoverEnded();
        }
    }
    private void SelectionStarted()
    {
        if (IsPinching())
        {
            lastEyeInteractable?.Select(true, (handUsedForPinchSelection?.IsTracked ?? false) ? handUsedForPinchSelection.transform : transform);
        }
        else
        {
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
            OnHoverEnded();
            lineRenderer.startColor = lineRenderer.endColor = rayColorHoverState;

            if (!interactables.TryGetValue(hit.transform.gameObject.GetHashCode(), out EyeInteractable eyeInteractable))
            {
                eyeInteractable = hit.transform.GetComponent<EyeInteractable>();
                interactables.Add(hit.transform.gameObject.GetHashCode(), eyeInteractable);
            }

            var toLocalSpace = transform.InverseTransformPoint(eyeInteractable.transform.position);
            lineRenderer.SetPosition(1, new Vector3(0, 0, toLocalSpace.z));

            eyeInteractable.Hover(true);
            lastEyeInteractable = eyeInteractable;
        }
    }
    private void OnHoverEnded()
    {
        foreach (var interactable in interactables) interactable.Value.Hover(false);
    }
    private void OnDestroy() => interactables.Clear();

    private bool IsPinching() => (allowPinchSelection && handUsedForPinchSelection.GetFingerIsPinching(OVRHand.HandFinger.Index)) || mockhandUsedForPinchSelection;
}
