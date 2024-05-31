using TMPro;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class EyeInteractable : MonoBehaviour
{
    [field: SerializeField]
    public bool IsHovered { get; private set; }

    [field: SerializeField]
    public bool IsSelected { get; private set; }


    [SerializeField]
    private UnityEvent<GameObject> OnObjectHover;

    [SerializeField]
    private UnityEvent<GameObject> OnObjectSelect;



    [SerializeField]
    private Material OnHoverActiveMaterial;

    [SerializeField]
    private Material OnSelectActiveMaterial;

    [SerializeField]
    private Material OnIdleMaterial;



    private MeshRenderer meshRenderer;

    private Transform originalAnchor;

    private TextMeshPro statusText;



    public string ObjectName
    {
        get { return gameObject.name; }
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        statusText = GetComponentInChildren<TextMeshPro>();
        originalAnchor = transform.parent;

    }

    public void Hover(bool state)
    {
        IsHovered = state;
    }

    public void Select(bool state, Transform anchor = null)
    {
        IsSelected = state;
        if (anchor) transform.SetParent(anchor);
        if (!IsSelected) transform.SetParent(originalAnchor);
    }


    private void Update()
    {
        if (IsSelected)
        {
            OnObjectSelect?.Invoke(gameObject);
            meshRenderer.material = OnSelectActiveMaterial;
            statusText.text = "";
        }
        else if (IsHovered)
        {
            OnObjectHover?.Invoke(gameObject);
            meshRenderer.material = OnHoverActiveMaterial;
            statusText.text = $"<color=\"yellow\">SELECTED</color>";
        }
        else
        {
            meshRenderer.material = OnIdleMaterial;
            statusText.text = "";
        }
        
    }
}