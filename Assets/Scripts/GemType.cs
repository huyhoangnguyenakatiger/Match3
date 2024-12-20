using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "GemType", menuName = "Match3/GameType")]
    public class GemType : ScriptableObject
    {
        public Sprite sprite;
    }
}