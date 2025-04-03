using System;
using System.Collections.Generic;
using System.Linq;

namespace Match3
{
    public enum GemType
    {
        // Simple
        None = 0,
        Blue = 1,
        Green = 2,
        Red = 3,
        Yellow = 4,
        Purple = 5,
        
        // Specials
        Bomb = 10
    }
    
    public static class GemTypeExtensions
    {
        public static bool IsSimple(this GemType gemType)
        {
            return gemType == GemType.Blue ||
                gemType == GemType.Green ||
                gemType == GemType.Red ||
                gemType == GemType.Yellow ||
                gemType == GemType.Purple;
        }

        public static bool IsSpecial(this GemType gemType)
        {
            return gemType == GemType.Bomb;
        }

        public static List<GemType> GetSimpleGemsTypeList()
        {
            return Enum.GetValues(typeof(GemType))
                .Cast<GemType>()
                .Where(gem => gem.IsSimple())
                .ToList();
        }
        
        public static List<GemType> GetSpecialGemsTypeList()
        {
            return Enum.GetValues(typeof(GemType))
                .Cast<GemType>()
                .Where(gem => gem.IsSpecial())
                .ToList();
        }
    }
}