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
            return _storage.Load<ConfigData>("Config.json") ?? CreateComplexMockConfig();
        }

        // Board 10x10 = 100 ô. index = y * BoardWidth + x
        // Mỗi hàng (row) là 1 mũi tên ngang dài 10 ô, đầu mũi tên nằm ở cột cuối (x = 9),
        // hướng từ trái (x=0) sang phải (x=9). Toàn bộ 10 hàng => phủ đúng 100 ô, không chồng lấn.
        private ConfigData CreateComplexMockConfig()
        {
            const int width = 10;
            const int height = 10;

            List<Arrow> arrows = new List<Arrow>();

            // ==========================================
            // TẠO CÁC MŨI TÊN VỚI HÌNH THÙ KHÁC NHAU
            // (x, y) - Tọa độ đầu tiên trong mảng sẽ là ĐẦU mũi tên
            // ==========================================

            // 1. Mũi tên thẳng (Độ dài 2 - Tối thiểu)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (1, 0), (0, 0) // Đầu ở (1,0), đuôi ở (0,0)
        }));

            // 2. Mũi tên thẳng ngang (Độ dài 4)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (5, 0), (4, 0), (3, 0), (2, 0)
        }));

            // 3. Mũi tên hình chữ L (Độ dài 3 - Tối thiểu để có 1 góc)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (2, 2), // Đầu mũi tên
            (1, 2), // Góc bẻ
            (1, 1)  // Đuôi mũi tên
        }));

            // 4. Mũi tên hình chữ L dài hơn (Độ dài 4)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (4, 4), (4, 3), (4, 2), (5, 2)
        }));

            // 5. Mũi tên hình chữ U (Độ dài 5 - Có 2 góc bẻ)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (7, 2), // Đầu
            (7, 3), (8, 3), (9, 3), (9, 2), (9, 1), // Thân
            (9, 0)  // Đuôi
        }));

            // 6. Mũi tên dích dắc (Z-shape) (Độ dài 5)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (0, 5), (1, 5), (1, 6), (2, 6), (2, 7)
        }));

            // 7. Mũi tên bọc góc lớn (Giống viền map)
            arrows.Add(BuildArrow(width, new (int, int)[] {
            (0, 9), (1, 9), (2, 9), (3, 9), (3, 8), (3, 7)
        }));


            // ==========================================
            // XÁO TRỘN ĐỂ CÓ THỨ TỰ KHÁC NHAU (Shuffle)
            // ==========================================
            ShuffleList(arrows);

            return new ConfigData(width, height, arrows.ToArray());
        }

        /// <summary>
        /// Hàm hỗ trợ: Chuyển đổi mảng tọa độ (x,y) thành object Arrow với index 1D.
        /// </summary>
        private Arrow BuildArrow(int gridWidth, (int x, int y)[] points)
        {
            int[] indices = new int[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                // Công thức chuyển 2D (x,y) sang 1D (index)
                indices[i] = points[i].x + (points[i].y * gridWidth);
            }

            // Lấy tọa độ x, y của điểm đầu tiên làm Head
            int headX = points[0].x;
            int headY = points[0].y;

            return new Arrow(headX, headY, indices);
        }

        /// <summary>
        /// Hàm hỗ trợ: Trộn ngẫu nhiên danh sách (Thuật toán Fisher-Yates)
        /// </summary>
        private void ShuffleList(List<Arrow> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1); // Dùng của Unity

                // Hoán đổi vị trí
                Arrow temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
