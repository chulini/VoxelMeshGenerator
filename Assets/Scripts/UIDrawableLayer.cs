using System.Security.Cryptography.X509Certificates;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <inheritdoc />
/// <summary>
/// Enables the player to click and paint a Texture2D
/// </summary>
[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(UIImagePointerClickToPixel))]
public class UIDrawableLayer : MonoBehaviour
{
    
    [HideInInspector] public RectTransform myRectTransform;
    UIImagePointerClickToPixel _pointerPixelEvents;
    
    bool _isHidden;
    public bool IsHidden
    {
        get { return _isHidden; }
        set
        {
            _isHidden = value;
            _rawImage.enabled = !_isHidden;
        }
    }

    public bool IsEditable
    {
        get { return _isEditable; }
        set
        {
            _isEditable = value;
            _rawImage.color = _isEditable ? Color.white : Color.gray;
        }
    }

    bool _isEditable;
    
    RawImage _rawImage;
    int _layerId;


    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        myRectTransform = GetComponent<RectTransform>();
        _pointerPixelEvents = GetComponent<UIImagePointerClickToPixel>();
    }

    void Start()
    {
        _pointerPixelEvents.OnPointerDown.AddListener(((x, y, color) =>
        {
            if(IsEditable)
                Paint(x,y,color);    
        }));
        _pointerPixelEvents.OnPointerDrag.AddListener((x, y, color) =>
        {
            if(IsEditable)
                Paint(x,y,color);
        });
    }
    /// <summary>
    /// Initializes the class
    /// </summary>
    /// <param name="layerId">ID of the layer</param>
    public void Init(int layerId)
    {
        _layerId = layerId;
        RedrawRawImage();
    }

    void RedrawRawImage()
    {
        _rawImage.texture = Map.layers[_layerId].GetTexture(true);
    }

    /// <summary>
    /// Store the integer in the selected layer
    /// </summary>
    void Paint(int xCenter, int yCenter, Color color)
    {
        yCenter = Map.height-yCenter;
        bool mustRedraw = false;
        
        //Paint r pixels around the pressed pixel
        const int r = Map.width/16;
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                BlockCoordinate current = new BlockCoordinate(xCenter + x, yCenter + y, _layerId);
                //If the integer stored in that coordinate is a different value, set the value in the IntArrayFromTexture and redraw the texture
                if (current.inBounds() &&
                    Map.layers[_layerId].GetInt(current.x, current.y) != UIColorPicker.paintingBlockID)
                {
                    Map.layers[_layerId].SetInt(current.x, current.y, UIColorPicker.paintingBlockID);
                    mustRedraw = true;
                }
            }
        }

        if(mustRedraw)
            RedrawRawImage();
        
    }



}
