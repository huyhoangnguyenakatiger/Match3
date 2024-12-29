using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Match3
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;

        [SerializeField] Gem gemPrefab;
        [SerializeField] GemType[] gemTypes;
        [SerializeField] Ease ease = Ease.InQuad;

        GridSystem2D<GridObject<Gem>> grid;
        AudioManager audioManager;
        [SerializeField] GameObject explosion;
        InputReader inputReader;
        Vector2Int selectedGem = Vector2Int.one * -1;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] Button quitButton;
        int score = 0;

        void Awake()
        {
            inputReader = GetComponent<InputReader>();
            audioManager = GetComponent<AudioManager>();
        }

        void OnDestroy()
        {
            inputReader.Fire -= OnSelectGem;
        }

        void Start()
        {
            InitializeGrid();
            inputReader.Fire += OnSelectGem;
            quitButton.onClick.AddListener(() => Application.Quit());
        }

        // void Update()
        // {
        //     scoreText.text = score.ToString();
        // }

        void OnSelectGem()
        {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(new Vector3(inputReader.Selected.x, inputReader.Selected.y, 7f)));
            // var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));
            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;

            if (selectedGem == gridPos)
            {
                DeselectGem();
                audioManager.PlayDeselect();
            }
            else if (selectedGem == Vector2Int.one * -1)
            {
                SelectGem(gridPos);
                audioManager.PlayClick();
            }
            else
            {
                StartCoroutine(RunGameLoop(selectedGem, gridPos));
            }
        }

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));

            List<Vector2Int> matches = FindMatches();

            CalculateScore(matches);

            yield return StartCoroutine(ExplodeGem(matches));

            yield return StartCoroutine(MakeGemsFall());

            yield return StartCoroutine(FillEmptySpots());

            DeselectGem();
        }

        void CalculateScore(List<Vector2Int> matches)
        {
            if (matches.Count == 0)
            {
                return;
            }
            score += matches.Count * 10;
            scoreText.text = score.ToString();
        }

        IEnumerator FillEmptySpots()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        CreateGem(x, y);
                        audioManager.PlayPop();
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }

        IEnumerator MakeGemsFall()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        for (var i = y + 1; i < height; i++)
                        {
                            if (grid.GetValue(x, i) != null)
                            {
                                var gem = grid.GetValue(x, i);
                                grid.SetValue(x, i, null);
                                grid.SetValue(x, y, gem);

                                gem.GetValue().transform.DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.2f).SetEase(ease);

                                audioManager.PlayWoosh();
                                yield return new WaitForSeconds(0.2f);
                                break;
                            }
                        }
                    }
                }
            }
        }

        IEnumerator ExplodeGem(List<Vector2Int> matches)
        {
            audioManager.PlayPop();
            foreach (Vector2Int match in matches)
            {

                var gem = grid.GetValue(match.x, match.y).GetValue();
                grid.SetValue(match.x, match.y, null);

                ExplodeVFX(match);

                gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f);

                yield return new WaitForSeconds(0.1f);

                gem.DestroyGem();

            }

        }

        void ExplodeVFX(Vector2Int match)
        {
            var fx = Instantiate(explosion, transform);
            fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
            Destroy(fx, 5f);
        }

        List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new HashSet<Vector2Int>();

            //horizontal
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width - 2; x++)
                {
                    var gemA = grid.GetValue(x, y);
                    var gemB = grid.GetValue(x + 1, y);
                    var gemC = grid.GetValue(x + 2, y);
                    if (gemA == null || gemB == null || gemC == null) continue;
                    if (gemA.GetValue().GetType() == gemB.GetValue().GetType() && gemB.GetValue().GetType() == gemC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x + 1, y));
                        matches.Add(new Vector2Int(x + 2, y));
                    }
                }
            }
            //vertical
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    var gemA = grid.GetValue(x, y);
                    var gemB = grid.GetValue(x, y + 1);
                    var gemC = grid.GetValue(x, y + 2);
                    if (gemA == null || gemB == null || gemC == null) continue;
                    if (gemA.GetValue().GetType() == gemB.GetValue().GetType() && gemB.GetValue().GetType() == gemC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x, y + 1));
                        matches.Add(new Vector2Int(x, y + 2));
                    }
                }
            }

            if (matches.Count == 0)
            {
                audioManager.PlayNoMatch();

            }
            else
            {
                audioManager.PlayMatch();
            }

            return new List<Vector2Int>(matches);

        }

        IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.2f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.2f)
                .SetEase(ease);

            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.2f);
        }

        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Gem>>.VerticalConverter(width, height, cellSize, originPosition, debug);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    CreateGem(x, y);
                }
            }
        }


        void CreateGem(int x, int y)
        {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            grid.SetValue(x, y, gridObject);
        }

        void DeselectGem() => selectedGem = new Vector2Int(-1, -1);
        void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;

        bool IsValidPosition(Vector2 gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}