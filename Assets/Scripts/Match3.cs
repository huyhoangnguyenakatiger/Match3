using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace Match3
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 origin = Vector3.zero;
        [SerializeField] bool debug = true;
        GridSystem2D<GridObject<Gem>> grid;
        [SerializeField] Gem gemPrefab;
        // [SerializeField] GameObject gameObject;
        [SerializeField] GemType[] gemTypes;
        [SerializeField] Ease ease = Ease.InQuad;
        Vector2Int selectedGem = Vector2Int.one * -1;
        InputReader inputReader;
        void Awake()
        {
            inputReader = GetComponent<InputReader>();
        }
        void Start()
        {

            InitializeGrid();
            inputReader.Fire += OnSelectedGem;
        }

        void OnDestroy()
        {
            inputReader.Fire -= OnSelectedGem;
        }

        void OnSelectedGem()
        {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            if (selectedGem == gridPos)
            {
                DeselectedGem();
            }
            else if (selectedGem == Vector2Int.one * -1)
            {
                SelectGem(gridPos);
            }
            else
            {
                StartCoroutine(RunGameLoop(selectedGem, gridPos));
            }

        }

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);
            gridObjectA.GetValue().transform.DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f).SetEase(ease);
            gridObjectB.GetValue().transform.DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f).SetEase(ease);
            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);
            yield return new WaitForSeconds(0.5f);
        }

        void DeselectedGem() { selectedGem = new Vector2Int(-1, -1); }
        void SelectGem(Vector2Int gridPos) { }




        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Gem>>.VerticalConverter(width, height, cellSize, origin, debug);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
                    gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
                    var gridObject = new GridObject<Gem>(grid, x, y);
                    gridObject.SetValue(gem);
                    grid.SetValue(x, y, gridObject);
                }
            }
        }
    }

}