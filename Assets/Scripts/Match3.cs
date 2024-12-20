using UnityEngine;

namespace Match3
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 origin = Vector3.zero;
        GridSystem2D<GridObject<Gem>> grid;
        [SerializeField] bool debug = true;
        void Start()
        {
            grid = GridSystem2D<GridObject<Gem>>.VerticalConverter(width, height, cellSize, origin, debug);
        }
    }

}