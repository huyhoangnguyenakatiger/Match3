using UnityEngine;

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
        void Start()
        {

            InitializeGrid();
        }

        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Gem>>.VerticalConverter(width, height, cellSize, origin, debug);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
                    // gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
                    var gridObject = new GridObject<Gem>(grid, x, y);
                    gridObject.SetValue(gem);
                    grid.SetValue(x, y, gridObject);
                }
            }
        }
    }

}