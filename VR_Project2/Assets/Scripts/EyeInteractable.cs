using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using UnityEngine.SceneManagement;


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

    private float hoverStartTime = 0f;
    private float sceneStartTime = 0f;



    public string ObjectName
    {
        get { return gameObject.name; }
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        statusText = GetComponentInChildren<TextMeshPro>();
        originalAnchor = transform.parent;
      
        sceneStartTime = Time.time;

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hoverStartTime = 0f;
        sceneStartTime = Time.time;// Reset hover start time when a new scene is loaded
    }

    public void Hover(bool state)
    {
        IsHovered = state;
        if (IsHovered)
        {
            hoverStartTime = Time.time-sceneStartTime;
            SaveHoverStartTimeToFile();
        }
        
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
    private void SaveHoverStartTimeToFile()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string data = $"{sceneName},{ObjectName},{hoverStartTime:F2}";

        string filePath = Path.Combine(Application.persistentDataPath, $"{sceneName}_hover_start_times.txt");

        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = File.CreateText(filePath))
            {
                writer.WriteLine("Scene Name,Object Name,Hover Start Time (s)");
            }
        }

        using (StreamWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine(data);
        }

        Debug.Log($"Hover start time for {ObjectName} recorded at {hoverStartTime:F2} seconds. Data saved to file.");
    }
    public float GetHoverStartTime()
    {
        return hoverStartTime;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
