namespace Match3
{
    public static class GemExtensions
    {
        public static bool IsSimple(this Gem gem)
        {
            return gem.GemType
                is GemType.Blue
                or GemType.Green
                or GemType.Red
                or GemType.Yellow
                or GemType.Purple;
        }

        public static bool IsSpecial(this Gem gem)
        {
            return gem.GemType == GemType.Bomb;
        }
    }
}