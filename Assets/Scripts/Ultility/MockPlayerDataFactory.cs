using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Temporary mock data generator for PlayerData, used until a real save file / backend exists.
    /// Constraints respected:
    ///  - PlayerData.Heart is always less than 5
    ///  - Every LevelData.Star is always less than 3 (values 0, 1 or 2)
    ///  - UNPLAYED levels always have a higher LevelId than COMPLETED / NOTCOMLETED levels
    /// </summary>
    public static class MockPlayerDataFactory
    {
        public static PlayerData CreateMockPlayerData()
        {
            var levels = new List<LevelData>
            {
                CreateLevel(1, star: 2, state: LevelState.COMPLETED,   hardness: Hardness.NORMAL),
                CreateLevel(2, star: 1, state: LevelState.COMPLETED,   hardness: Hardness.NORMAL),
                CreateLevel(3, star: 0, state: LevelState.NOTCOMLETED, hardness: Hardness.HARD),
                CreateLevel(4, star: 0, state: LevelState.UNPLAYED,    hardness: Hardness.HARD),
                CreateLevel(5, star: 0, state: LevelState.UNPLAYED,    hardness: Hardness.SUPERHARD),
            };

            return new PlayerData
            {
                Gold = 500,
                Heart = 3,               // < 5
                Star = SumStars(levels), // total stars earned across all levels
                CurrentLevelsData = levels.ToArray()
            };
        }

        private static int SumStars(List<LevelData> levels)
        {
            int total = 0;
            foreach (var level in levels)
                total += level.Star;
            return total;
        }

        private static LevelData CreateLevel(int levelId, int star, LevelState state, Hardness hardness)
        {
            return new LevelData
            {
                LevelId = levelId,
                Star = star,
                LevelState = state,
                Hardness = hardness,
                BoardData = CreateSimpleBoard()
            };
        }

        // Small placeholder board so LevelData.BoardData isn't null.
        // Swap this out for real level layouts (e.g. loaded via ConfigManager) when available.
        private static BoardData CreateSimpleBoard()
        {
            var arrows = new Arrow[]
            {
                new Arrow(4, 0, new int[] { 4, 3, 2 }),
                new Arrow(0, 3, new int[] { 15, 10 })
            };
            return new BoardData(5, 4, arrows);
        }
    }
}
