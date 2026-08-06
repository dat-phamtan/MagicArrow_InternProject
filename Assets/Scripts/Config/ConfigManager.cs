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
            // LỚP 1: CÁC MŨI TÊN NẰM CHỜ SẴN Ở BÌA (THOÁT ĐƯỢC NGAY)
            // Đầu mũi tên quay thẳng ra ngoài viền bản đồ
            // ==========================================

            // Thoát qua cạnh Phải (X = 9)
            arrows.Add(BuildArrow(width, new (int, int)[] { (9, 9), (8, 9), (7, 9), (6, 9) })); // A1: Thẳng dài 4
            arrows.Add(BuildArrow(width, new (int, int)[] { (9, 4), (8, 4), (8, 5), (8, 6), (8, 7), (8, 8) })); // A3: Chữ L dài 6

            // Thoát qua cạnh Trên (Y = 9)
            arrows.Add(BuildArrow(width, new (int, int)[] { (9, 8), (9, 7), (9, 6), (9, 5) })); // A2: Thẳng dài 4

            // Thoát qua cạnh Dưới (Y = 0)
            arrows.Add(BuildArrow(width, new (int, int)[] { (9, 0), (9, 1), (9, 2), (9, 3) })); // A4: Thẳng dài 4
            arrows.Add(BuildArrow(width, new (int, int)[] { (1, 0), (1, 1) })); // A26: Thẳng ngắn 2

            // Thoát qua cạnh Trái (X = 0)
            arrows.Add(BuildArrow(width, new (int, int)[] { (0, 8), (1, 8), (2, 8) })); // A9: Thẳng 3
            arrows.Add(BuildArrow(width, new (int, int)[] { (0, 7), (1, 7), (2, 7) })); // A10: Thẳng 3
            arrows.Add(BuildArrow(width, new (int, int)[] { (0, 6), (1, 6), (2, 6) })); // A13: Thẳng 3
            arrows.Add(BuildArrow(width, new (int, int)[] { (0, 5), (1, 5), (2, 5), (2, 4) })); // A15: Chữ L 4
            arrows.Add(BuildArrow(width, new (int, int)[] { (0, 4), (1, 4), (1, 3), (1, 2) })); // A18: Chữ L 4

            // ==========================================
            // LỚP 2: THOÁT RA SAU KHI LỚP 1 ĐÃ BAY ĐI
            // ==========================================
            arrows.Add(BuildArrow(width, new (int, int)[] { (5, 9), (4, 9), (3, 9) })); // A5: Bay theo A1
            arrows.Add(BuildArrow(width, new (int, int)[] { (7, 8), (6, 8), (5, 8), (4, 8) })); // A7: Bay theo A3
            arrows.Add(BuildArrow(width, new (int, int)[] { (7, 7), (6, 7), (5, 7), (4, 7) })); // A11: Bay theo A3
            arrows.Add(BuildArrow(width, new (int, int)[] { (0, 3), (0, 2), (0, 1), (0, 0) })); // A19: Bay theo A18 (Lên)
            arrows.Add(BuildArrow(width, new (int, int)[] { (2, 3), (2, 2), (2, 1), (2, 0) })); // A20: Bay theo A15 (Lên)
            arrows.Add(BuildArrow(width, new (int, int)[] { (8, 3), (7, 3), (6, 3), (5, 3) })); // A21: Bay theo A4
            arrows.Add(BuildArrow(width, new (int, int)[] { (8, 2), (7, 2), (6, 2), (6, 1) })); // A22: Chữ L, bay theo A4
            arrows.Add(BuildArrow(width, new (int, int)[] { (8, 1), (7, 1), (7, 0), (8, 0) })); // A24: Chữ U, bay theo A4

            // ==========================================
            // LỚP 3 & BÊN TRONG CÙNG: CÁC KHỐI PHỨC TẠP
            // Chờ các lớp ngoài dọn dẹp xong mới có đường thoát
            // ==========================================
            arrows.Add(BuildArrow(width, new (int, int)[] { (2, 9), (1, 9), (0, 9) })); // A6: Bay sau A5
            arrows.Add(BuildArrow(width, new (int, int)[] { (3, 8), (3, 7), (3, 6) })); // A8: Bay sau A5 (Lên)
            arrows.Add(BuildArrow(width, new (int, int)[] { (7, 6), (7, 5), (6, 5), (5, 5), (5, 6), (6, 6) })); // A12: Chữ U dài 6, bay sau A11
            arrows.Add(BuildArrow(width, new (int, int)[] { (4, 6), (4, 5), (4, 4), (4, 3) })); // A14: Thẳng 4, bay sau A11
            arrows.Add(BuildArrow(width, new (int, int)[] { (3, 5), (3, 4), (3, 3) })); // A16: Thẳng 3, bay sau A8
            arrows.Add(BuildArrow(width, new (int, int)[] { (7, 4), (6, 4), (5, 4) })); // A17: Thẳng 3, bay sau A3
            arrows.Add(BuildArrow(width, new (int, int)[] { (5, 2), (4, 2), (3, 2) })); // A23: Thẳng 3, bay sau A22

            // Khối dích dắc (Z-Shape) phức tạp ở góc dưới cùng
            arrows.Add(BuildArrow(width, new (int, int)[] { (6, 0), (5, 0), (5, 1), (4, 1), (3, 1) })); // A27: Z-Shape 5 ô, bay sau A24
            arrows.Add(BuildArrow(width, new (int, int)[] { (4, 0), (3, 0) })); // A28: Khối nhỏ chèn chỗ trống, bay sau A27

            // ==========================================
            // XÁO TRỘN ĐỂ THÊM VÀO HÀNG ĐỢI NGẪU NHIÊN
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
