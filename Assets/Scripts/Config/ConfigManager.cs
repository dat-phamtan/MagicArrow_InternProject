using Assets.Scripts.Data;
using Assets.Scripts.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Config
{
    public class ConfigManager : IConfig
    {
        private readonly IStorage _storage;

        public ConfigManager(IStorage storage)
        {
            _storage = storage;
        }

        public ConfigData Load()
        {
            return _storage.Load<ConfigData>("Config.json") ?? CreateMockConfig();
        }

        // Board 10x10 = 100 ô. index = y * BoardWidth + x
        // Mỗi hàng (row) là 1 mũi tên ngang dài 10 ô, đầu mũi tên nằm ở cột cuối (x = 9),
        // hướng từ trái (x=0) sang phải (x=9). Toàn bộ 10 hàng => phủ đúng 100 ô, không chồng lấn.
        private ConfigData CreateMockConfig()
        {
            const int width = 10;
            const int height = 10;

            var arrows = new Arrow[height];

            for (int y = 0; y < height; y++)
            {
                var indices = new int[width];
                for (int x = 0; x < width; x++)
                {
                    indices[x] = y * width + x; // tail (x=0) -> head (x=width-1)
                }

                arrows[y] = new Arrow(
                    xArrowHead: width - 1,
                    yArrowHead: y,
                    arrowIndices: indices
                );
            }

            return new ConfigData(width, height, arrows);
        }
    }
}
