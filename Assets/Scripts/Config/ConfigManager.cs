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

            for (int i = 0; i < height - 1; i++)
            {
                var indices = new int[width - 1];
                for (int j = 0; j < width - 1; j++)
                {
                    indices[j] = j + width * i;
                }

                arrows[i] = new Arrow(0, i, indices);
            }

            var longIndices = new int[height + width - 1];
            int currentIndex = 0;

            for (int i = 0; i < height; i++)
            {
                longIndices[currentIndex++] = (width - 1) + (i * width);
            }

            for (int j = width - 2; j >= 0; j--)
            {
                longIndices[currentIndex++] = j + (height - 1) * width;
            }

            arrows[height - 1] = new Arrow(width - 1, 0, longIndices);

            return new ConfigData(width, height, arrows);
        }
    }
}
