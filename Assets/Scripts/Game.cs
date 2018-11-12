using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public enum GameState {Drawing, GeneratingMesh, LookingMesh}

    GameState _gameState = GameState.Drawing;

    public GameState gameState
    {
        get { return _gameState; }
        set
        {
            _gameState = value;
            if (_gameState == GameState.Drawing)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (_gameState == GameState.GeneratingMesh)
            {
                _layerEditorCanvas.SetActive(false);
                _meshCanvas.SetActive(true);
                _map.SaveToHDD();
                StartCoroutine("GenerateMesh");
            }
            
        }
    }

    [SerializeField] GameObject _layerEditorCanvas;
    [SerializeField] GameObject _meshCanvas;
    [SerializeField] Button _okButton;
    [SerializeField] Material _materialForMesh;
    [SerializeField] Text meshInfoText;
    GreedyMeshGenerator[] _greedyMeshGenerators;
    Map _map;
    

    void Awake()
    {
        _greedyMeshGenerators = new GreedyMeshGenerator[3];

        _map = new Map();
        _okButton.onClick.AddListener(() =>
        {
            gameState = GameState.GeneratingMesh;   
            
        });

        for (int i = 0; i < 3; i++)
            _greedyMeshGenerators[i] = new GreedyMeshGenerator();

    }

    IEnumerator GenerateMesh()
    {
        for (int i = 0; i < _greedyMeshGenerators.Length; i++)
        {
            _greedyMeshGenerators[i].layers = Map.layers;
            _greedyMeshGenerators[i].material = _materialForMesh;
            _greedyMeshGenerators[i].Init();
        }

        _greedyMeshGenerators[0].renderAxis = "xy";
        _greedyMeshGenerators[1].renderAxis = "xz";
        _greedyMeshGenerators[2].renderAxis = "yz";

        yield return new WaitForSecondsRealtime(.3f);


        for (int i = 0; i < _greedyMeshGenerators.Length; i++)
        {
            _greedyMeshGenerators[i].IsDone = false;
            _greedyMeshGenerators[i].Start();
        }

        float t0 = Time.time;

        //Wait until all threads are done
        do
        {
            meshInfoText.text = "Generating mesh... "+ThreadsDone().ToString() + " threads done."; 
            yield return null;
        } while (ThreadsDone() != _greedyMeshGenerators.Length);

        int totalQuads = 0;
        for (int i = 0; i < _greedyMeshGenerators.Length; i++)
            totalQuads += _greedyMeshGenerators[i].QuadCounter;
            
        meshInfoText.text = "Greedy mesh using only "+totalQuads+" quads was generated in " + (Time.time - t0).ToString("0.0000") +
                            " seconds using 3 threads.";
        
        Debug.Log("All threads done in "+(Time.time - t0).ToString("0.0000")+"[s]");
        
        for (int i = 0; i < _greedyMeshGenerators.Length; i++)
        {
            _greedyMeshGenerators[i].UpdateMesh();
            GameObject meshContainer = new GameObject(_greedyMeshGenerators[i].renderAxis);
            meshContainer.AddComponent<MeshFilter>().mesh = _greedyMeshGenerators[i].mesh;
            meshContainer.AddComponent<MeshRenderer>().sharedMaterial = _materialForMesh;
            meshContainer.transform.SetParent(gameObject.transform);
        }
        gameState = GameState.LookingMesh;

        meshInfoText.text += "\nPress [ESC] to edit.";
    }
    
    int ThreadsDone()
    {
        int counter = 0;
        for (int i = 0; i < _greedyMeshGenerators.Length; i++)
            if (_greedyMeshGenerators[i].IsDone)
                counter++;

        return counter;
    }

    void Update()
    {
        if (gameState == GameState.LookingMesh && Input.GetButtonDown("Cancel"))
        {
            SceneManager.LoadScene(0);
        }

        if (gameState == GameState.Drawing && Input.GetButtonDown("Submit"))
        {
            _okButton.onClick.Invoke();
        }
    }
}
