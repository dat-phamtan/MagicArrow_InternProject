using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Interfaces
{
    public interface IBoardState
    {
        public BoardData ConfigData { get; }
        public List<int> ArrowMatrix { get; }

        public bool IsOccupied(int boardIndex);
        public bool IsArrowActive(int boardIndex);
        public int GetConfigIndex(int boardIndex);
        public PartType GetPartType(Position pos);
        public Direction GetDirection(int boardIndex);
        public Direction GetDirection(Position pos);
        public void DisableArrow(int configIndex);
        public void SetAnimated(int configIndex, bool animated);
        public bool IsAnimated(int configIndex);
        public void ResetBoard();
        public void Load(BoardData data);
        public int IntPositionToIndex(Position pos);
        public Position IndexToPosition(int boardIndex);
        public Direction GetDirection(Position prePos, Position currentPos);
    }
}
