using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Drives a switchable AR face filter. Each <see cref="FilterLook"/> bundles an
/// optional face-mask texture, a set of prop prefabs and an optional particle
/// effect. A small on-screen UI lets the user cycle through the looks at runtime.
/// </summary>
public class FaceFilterController : MonoBehaviour
{
    [System.Serializable]
    public class FilterLook
    {
        public string name = "Look";
        [Tooltip("Texture applied to the AR face mesh. Leave empty to hide the mask for this look.")]
        public Texture2D maskTexture;
        [Tooltip("Prop prefabs parented to the tracked face (anchored by their own local transform).")]
        public GameObject[] props;
        [Tooltip("Optional particle-effect prefab parented to the tracked face.")]
        public GameObject particleEffect;
    }

    [SerializeField] private ARFaceManager faceManager;
    [SerializeField] private FilterLook[] looks;

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private int currentIndex;
    private Text label;

    private void Awake()
    {
        if (faceManager == null)
            faceManager = FindFirstObjectByType<ARFaceManager>();

        EnsureEventSystem();
        BuildUI();
    }

    private void OnEnable()
    {
        if (faceManager != null)
            faceManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        if (faceManager != null)
            faceManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void Start()
    {
        ApplyLook();
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARFace> changes)
    {
        foreach (ARFace face in changes.added)
            SetupFace(face);
    }

    public void NextLook()
    {
        if (!HasLooks()) return;
        currentIndex = (currentIndex + 1) % looks.Length;
        ApplyLook();
    }

    public void PreviousLook()
    {
        if (!HasLooks()) return;
        currentIndex = (currentIndex - 1 + looks.Length) % looks.Length;
        ApplyLook();
    }

    private void ApplyLook()
    {
        if (!HasLooks() || faceManager == null) return;
        foreach (ARFace face in faceManager.trackables)
            SetupFace(face);
        UpdateLabel();
    }

    private void SetupFace(ARFace face)
    {
        if (!HasLooks() || face == null) return;
        FilterLook look = looks[currentIndex];

        // Clear anything previously attached (props from the template prefab or a prior look).
        // The face mesh lives on the face root, so only child objects are removed.
        for (int i = face.transform.childCount - 1; i >= 0; i--)
            Destroy(face.transform.GetChild(i).gameObject);

        // Mask: drive the texture on the face-mesh renderer, or hide it when the look has no mask.
        MeshRenderer faceRenderer = face.GetComponent<MeshRenderer>();
        if (faceRenderer == null)
            faceRenderer = face.GetComponentInChildren<MeshRenderer>(true);

        if (faceRenderer != null)
        {
            if (look.maskTexture != null)
            {
                faceRenderer.enabled = true;
                Material mat = faceRenderer.material;
                mat.mainTexture = look.maskTexture;
                if (mat.HasProperty(BaseMapId))
                    mat.SetTexture(BaseMapId, look.maskTexture);
            }
            else
            {
                faceRenderer.enabled = false;
            }
        }

        // Props + particle effect parented to the face (keeping each prefab's authored local transform).
        if (look.props != null)
        {
            foreach (GameObject prop in look.props)
                if (prop != null)
                    Instantiate(prop, face.transform, false);
        }

        if (look.particleEffect != null)
            Instantiate(look.particleEffect, face.transform, false);
    }

    private bool HasLooks() => looks != null && looks.Length > 0;

    private void UpdateLabel()
    {
        if (label != null && HasLooks())
            label.text = looks[currentIndex].name;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private void BuildUI()
    {
        var canvasGo = new GameObject("FaceFilterCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        label = CreateLabel(canvasGo.transform);
        CreateButton(canvasGo.transform, "<", new Vector2(0f, 0f), new Vector2(40f, 130f), PreviousLook);
        CreateButton(canvasGo.transform, "Next  >", new Vector2(1f, 0f), new Vector2(-40f, 130f), NextLook);

        UpdateLabel();
    }

    private Text CreateLabel(Transform parent)
    {
        var go = new GameObject("FilterNameLabel");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(800f, 110f);
        rect.anchoredPosition = new Vector2(0f, -60f);

        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 56;
        text.color = Color.white;
        text.text = "Filter";

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        outline.effectDistance = new Vector2(2f, -2f);
        return text;
    }

    private void CreateButton(Transform parent, string caption, Vector2 anchor, Vector2 anchoredPos,
        UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(caption + "Button");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.sizeDelta = new Vector2(320f, 140f);
        rect.anchoredPosition = anchoredPos;

        var image = go.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 48;
        text.color = Color.white;
        text.text = caption;
    }
}
