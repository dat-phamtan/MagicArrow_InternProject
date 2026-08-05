using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.UI
{
    public class UIManager : IUIManager
    {
        private IInput _input;
        private IController _controller;
        private IEventHandler _eventHandler;
        private ConfigData _configData;
        private float _spacing;
        private Dictionary<int, GameObject> _arrowRoots;
        private Dictionary<int, ArrowMeshBuilder> _arrowBuilders;
        private Dictionary<int, Vector3[]> _arrowPaths;

        public UIManager(IController controller, IInput input, float spacing)
        {
            _input = input;
            _controller = controller;
            _spacing = spacing;
        }

        public void Init(IEventHandler eventHandler)
        {
            _eventHandler = eventHandler;

            _eventHandler.OnInteractAt += HandleInteractAt;
        }

        private void HandleInteractAt(Vector3 pos)
        {
            _input.HandleInput(pos);
        }



        

        //public List<Verticle> InitBoard(float spacing)
        //{
        //    int width = _controller.GetConfigData().BoardWidth;
        //    int height = _controller.GetConfigData().BoardHeight;

        //    float xPos = - (width - 1) * spacing / 2f;
        //    float yPos = - (height - 1) * spacing / 2f;

        //    for (int i = 0;  i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            var type = _controller.GetArrowTypeAtPosition(new Position(j, i));

        //            if (type == PartType.HEAD)
        //            {
        //                _verticles.Add(new Verticle(xPos, yPos, VerticleType.HEAD));
        //            }

        //            else if (type == PartType.TAIL)
        //            {
        //                _verticles.Add(new Verticle(xPos, yPos, VerticleType.TAIL));
        //            }

        //            else if (type == PartType.BODY)
        //            {
        //                _verticles.Add(new Verticle(xPos, yPos, VerticleType.BODY));
        //            }
        //            xPos += spacing;
        //        }
        //        yPos += spacing;
        //        xPos = - (width - 1) * spacing / 2f;
        //    }
        //    return _verticles;
        //}     
    }
}
