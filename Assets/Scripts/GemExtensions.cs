namespace Match3
{
    public static class GemExtensions
    {
        public static bool IsSpecial(this GemType gemType)
        {
            return gemType == GemType.Bomb;
        }
    }
}