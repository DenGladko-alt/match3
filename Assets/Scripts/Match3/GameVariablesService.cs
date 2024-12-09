using UnityEngine.Serialization;
using Utility;

namespace Match3
{
    public class GameVariablesService : MonoBehaviourService<GameVariablesService>
    {
        public GameConfig GameSettings;
        [FormerlySerializedAs("LevelSettings")] public LevelConfig LevelConfig;
    }
}
