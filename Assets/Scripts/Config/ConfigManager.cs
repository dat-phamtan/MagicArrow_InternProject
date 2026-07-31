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

            // 2. Tạo 1 mũi tên chữ L ngược (cột ngoài cùng bên phải và hàng dưới cùng)
            // Tổng số index = chiều cao + chiều rộng - 1 (trừ đi ô góc dưới cùng bên phải bị trùng)
            var longIndices = new int[height + width - 1];
            int currentIndex = 0;

            // Phân đoạn 1: Đi dọc từ trên xuống dưới ở cột cuối cùng (cột 9)
            for (int i = 0; i < height; i++)
            {
                longIndices[currentIndex++] = (width - 1) + (i * width);
            }

            // Phân đoạn 2: Đi ngang từ phải sang trái ở hàng cuối cùng (hàng 9)
            // Bắt đầu từ width - 2 để không lấy lại ô góc (đã lấy ở vòng lặp trên)
            for (int j = width - 2; j >= 0; j--)
            {
                longIndices[currentIndex++] = j + (height - 1) * width;
            }

            // Khởi tạo mũi tên chữ L ngược (vị trí xuất phát ở góc trên cùng bên phải)
            arrows[height - 1] = new Arrow(width - 1, 0, longIndices);

            return new ConfigData(width, height, arrows);
        }
    }
}
