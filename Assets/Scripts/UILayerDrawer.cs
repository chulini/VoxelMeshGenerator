
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Switches the drawable layers using the next and prev buttons 
/// </summary>
public class UILayerDrawer : MonoBehaviour
{
    [SerializeField] RectTransform _drawableLayersContainer;
    [SerializeField] GameObject _drawableLayer;
    [SerializeField] Button _nextButton;
    [SerializeField] Button _prevButton;
    [SerializeField] Text _titleText;

    /// <summary>
    /// The int value of the color that will be painting
    /// </summary>
    bool _usingAxises = false;

    int _selectedLayer = 0;
    UIDrawableLayer[] _uiDrawableLayers;

    void Awake()
    {

        _uiDrawableLayers = new UIDrawableLayer[Map.depth];
        _nextButton.onClick.AddListener(() =>
        {
            //Select next layer
            _selectedLayer = (_selectedLayer + 1) % Map.depth;
            RefreshSelectedLayer();
        });
        _prevButton.onClick.AddListener(() =>
        {
            //Select previous layer
            _selectedLayer = (_selectedLayer - 1 < 0) ? Map.depth - 1 : _selectedLayer - 1;
            RefreshSelectedLayer();
        });
    }


    void RefreshSelectedLayer()
    {
        _uiDrawableLayers[_selectedLayer].myRectTransform.localScale = Vector3.one * .6f;
        _titleText.text = "Editing: Layer " + _selectedLayer;
    }

    void Start()
    {

        for (int i = 0; i < Map.depth; i++)
        {
            GameObject newLayer = Instantiate(_drawableLayer) as GameObject;
            newLayer.transform.SetParent(_drawableLayersContainer);
            newLayer.name = "Layer " + i;
            _uiDrawableLayers[i] = newLayer.GetComponent<UIDrawableLayer>();
            _uiDrawableLayers[i].Init(i);
        }

        RefreshSelectedLayer();
    }

    /// <summary>
    /// Invoke clicks un prev/next buttons when axises are moved
    /// </summary>
    void UpdateButtonsUsingAxises()
    {
        if (!_usingAxises)
        {
            if (Input.GetAxis("Horizontal") > .5f)
            {
                _nextButton.onClick.Invoke();
            }
            else if (Input.GetAxis("Horizontal") < -.5f)
            {
                _prevButton.onClick.Invoke();
            }

            if (Input.GetAxis("Vertical") > .5f)
            {
                _nextButton.onClick.Invoke();
            }
            else if (Input.GetAxis("Vertical") < -.5f)
            {
                _prevButton.onClick.Invoke();
            }

        }

        _usingAxises = (Mathf.Abs(Input.GetAxis("Horizontal")) > .5f) || (Mathf.Abs(Input.GetAxis("Vertical")) > .5f);
    }

    /// <summary>
    /// Before _selectedLayer layer ==> enable visibility
    /// _selectedLayer ==> set as editable
    /// After _selectedLayer ==> set as invisible 
    /// </summary>
    void UpdateLayersVisibility()
    {
        for (int i = 0; i < _uiDrawableLayers.Length; i++)
        {
            if (i < _selectedLayer)
            {
                _uiDrawableLayers[i].IsHidden = false;
                _uiDrawableLayers[i].IsEditable = false;
            } else if (i == _selectedLayer)
            {
                //_uiDrawableLayers[i].enabled = true;
                //uiDrawableLayers[i].myRectTransform.anchoredPosition = Vector2.zero;
                //_uiDrawableLayers[i].myRectTransform.SetAsLastSibling();
                
                _uiDrawableLayers[i].myRectTransform.localScale = Vector3.Lerp(
                    _uiDrawableLayers[i].myRectTransform.localScale,
                    Vector3.one, Time.deltaTime * 15f);
                
                _uiDrawableLayers[i].IsHidden = false;
                _uiDrawableLayers[i].IsEditable = true;
            }
            else
            {
                _uiDrawableLayers[i].IsHidden = true;
                _uiDrawableLayers[i].IsEditable = false;
            }
        }
    }

    void Update()
    {
        UpdateButtonsUsingAxises();
        UpdateLayersVisibility();
    }
}
