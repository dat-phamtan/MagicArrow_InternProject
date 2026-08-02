using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.UI
{
    public class UIManager
    {
        //public float spacing = 0.5f;
        private IController _controller;
        private List<int> _arrowMatrix;
        private List<Verticle> _verticles;
        private ConfigData _config;



        public UIManager(IController controller)
        {
            _controller = controller;
            _arrowMatrix = _controller.GetArrowMatrix();
            _verticles = new List<Verticle>();
        }

        public List<Verticle> InitBoard(float spacing)
        {
            int width = _controller.GetConfigData().BoardWidth;
            int height = _controller.GetConfigData().BoardHeight;

            float xPos = - (width - 1) * spacing / 2f;
            float yPos = - (height - 1) * spacing / 2f;

            for (int i = 0;  i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var type = _controller.GetArrowTypeAtPosition(new Position(j, i));

                    if (type == PartType.HEAD)
                    {
                        _verticles.Add(new Verticle(xPos, yPos, VerticleType.HEAD));
                    }
                        
                    else if (type == PartType.TAIL)
                    {
                        _verticles.Add(new Verticle(xPos, yPos, VerticleType.TAIL));
                    }
                        
                    else if (type == PartType.BODY)
                    {
                        _verticles.Add(new Verticle(xPos, yPos, VerticleType.BODY));
                    }
                        

                    xPos += spacing;
                }
                yPos += spacing;
                xPos = - (width - 1) * spacing / 2f;
            }

            return _verticles;
        }

        
    }
}
