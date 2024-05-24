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



    public string ObjectName
    {
        get { return gameObject.name; }
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

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
        }
        else if (IsHovered)
        {
            OnObjectHover?.Invoke(gameObject);
            meshRenderer.material = OnHoverActiveMaterial;
        }
        else
        {
            meshRenderer.material = OnIdleMaterial;
        }

    }
}