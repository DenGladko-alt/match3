
namespace Match3
{
    public interface IPoolable
    {
        public PoolType GetPoolType { get; set; }
        public void OnSpawn();
        public void OnDespawn();
    }

    public enum PoolType
    {
        GemBase,
        
        GemVFXBlue,
        GemVFXRed,
        GemVFXGreen,
        GemVFXYellow,
        GemVFXPurple,
        
        GemVFXBomb
    }
}
