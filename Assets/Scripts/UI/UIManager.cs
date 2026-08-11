using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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
        private Dictionary<int, GameObject> _arrowRoots;
        private Dictionary<int, ArrowMeshBuilder> _arrowBuilders;
        private Dictionary<int, Vector3[]> _arrowPaths;
        private Dictionary<int, Vector3[]> _curvedPath;
        private Dictionary<int, float[]> _cumulativeLength;

        public UIManager(IController controller, IInput input, float spacing)
        {
            _input = input;
            _controller = controller;
            //_spacing = spacing;
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



        //private IEnumerator AnimateMoveFail(GameObject interactedArrowRoot, GameObject collidedArrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, float[] cumulativeLength, int deltaIndex, int interactedConfigIndex)
        //{
        //    float exitDistance = (deltaIndex == 1) ? 0.5f * spacing : (deltaIndex - 1) * spacing;
        //    int n = originalPath.Length;
        //    //float totalLength = (n - 1) * spacing;
        //    float totalLength = cumulativeLength[^1];
        //    float travelled = 0f;

        //    while (travelled < exitDistance)
        //    {
        //        travelled = Mathf.Min(travelled + speed * Time.deltaTime, exitDistance);
        //        float headDist = -travelled;
        //        float tailDist = totalLength - travelled;

        //        var newPathList = new List<Vector3>();
        //        newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

        //        for (int i = 0; i < n; i++)
        //        {
        //            float nodeDist = cumulativeLength[i];
        //            if (nodeDist > headDist && nodeDist < tailDist)
        //                newPathList.Add(originalPath[i]);
        //        }
        //        newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
        //        builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
        //        yield return null;
        //    }

        //    HandleFirstFailAnimaion(interactedConfigIndex, interactedArrowRoot, collidedArrowRoot);

        //    while (travelled > 0)
        //    {
        //        travelled = Mathf.Max(travelled - speed * Time.deltaTime, 0f);
        //        float headDist = -travelled;
        //        float tailDist = totalLength - travelled;

        //        var newPathList = new List<Vector3>();
        //        newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

        //        for (int i = 0; i < n; i++)
        //        {
        //            float nodeDist = cumulativeLength[i];
        //            if (nodeDist > headDist && nodeDist < tailDist)
        //            {
        //                newPathList.Add(originalPath[i]);
        //            }
        //        }
        //        newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
        //        builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
        //        yield return null;
        //    }

        //    builder.BuildArrow(originalPath, cumulativeLength, spacing);
        //    OnUnblockInteractWidthArrow?.Invoke(interactedConfigIndex);
        //}

        //private IEnumerator AnimateMoveSuccess(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, float[] cumulativeLength, int configIndex)
        //{
        //    float exitDistance = camera.orthographicSize * 2f * camera.aspect + exitPadding;
        //    int n = originalPath.Length;
        //    //float totalLength = (n - 1) * spacing;
        //    float totalLength = cumulativeLength[^1];
        //    float targetTravel = totalLength + exitDistance;
        //    float travelled = 0f;

        //    while (travelled < targetTravel)
        //    {
        //        travelled += speed * Time.deltaTime;
        //        float headDist = -travelled;
        //        float tailDist = totalLength - travelled;

        //        var newPathList = new List<Vector3>();
        //        //newPathList.Add(PositionBehindHead(originalPath, exitDir, headDist));
        //        newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

        //        for (int i = 0; i < n; i++)
        //        {
        //            //float nodeDist = i * spacing;
        //            float nodeDist = cumulativeLength[i];
        //            if (nodeDist > headDist && nodeDist < tailDist)
        //            {
        //                newPathList.Add(originalPath[i]);
        //            }
        //        }
        //        //newPathList.Add(PositionBehindHead(originalPath, exitDir, tailDist));
        //        newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
        //        builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
        //        yield return null;
        //    }
        //    Destroy(arrowRoot);
        //    OnArrowDestroyed?.Invoke();
        //}

        


        ////HELPER FUNC
        //private Vector3 PositionAtDistance(Vector3[] curvedPath, float[] cumLen, float distance)
        //{
        //    if (distance <= 0f)
        //    {
        //        var dir = (curvedPath[1] - curvedPath[0]).normalized;
        //        return curvedPath[0] + dir * distance;
        //    }

        //    int lastPos = cumLen.Length - 1;
        //    if (distance >= cumLen[lastPos])
        //    {
        //        var dir = (curvedPath[lastPos] - curvedPath[lastPos - 1]).normalized;
        //        return curvedPath[lastPos] + dir * (distance - cumLen[lastPos]);
        //    }

        //    int lo = 0;
        //    while (cumLen[lo + 1] < distance)
        //        lo++;
        //    float t = (distance - cumLen[lo]) / (cumLen[lo + 1] - cumLen[lo]);
        //    return Vector3.Lerp(curvedPath[lo], curvedPath[lo + 1], t);
        //}

        //private void HandleFirstFailAnimaion(int configIndex, GameObject interactedArrowRoot, GameObject collidedArrowRoot)
        //{
        //    if (_controller.IsFirstMoveFail(configIndex))
        //    {
        //        arrowAssembler.ChangeArrowColor(1, interactedArrowRoot.GetComponent<ArrowMeshBuilder>());
        //    }
        //    OnCollidedAnimation?.Invoke(collidedArrowRoot);
        //}





    }
}















    //    //NO NEED FOR NOW
    //    private IEnumerator PlayCollidedAnimation(GameObject collidedArrow)
    //    {
    //        float time = 0f;
    //        Vector3 initScale = collidedArrow.transform.localScale;
    //        while (time < _arrowAnimationTime)
    //        {
    //            time += Time.deltaTime;
    //            collidedArrow.transform.localScale = Vector3.one * 1.05f;
    //            yield return null;
    //        }

    //        while (time > 0)
    //        {
    //            time += Time.deltaTime;
    //            collidedArrow.transform.localScale = Vector3.one * 1.05f;
    //            yield return null;
    //        }
    //        collidedArrow.transform.localScale = initScale;
    //    }
    //    //public List<Verticle> InitBoard(float spacing)
    //    //{
    //    //    int width = _controller.GetConfigData().BoardWidth;
    //    //    int height = _controller.GetConfigData().BoardHeight;

    //    //    float xPos = - (width - 1) * spacing / 2f;
    //    //    float yPos = - (height - 1) * spacing / 2f;

    //    //    for (int i = 0;  i < height; i++)
    //    //    {
    //    //        for (int j = 0; j < width; j++)
    //    //        {
    //    //            var type = _controller.GetArrowTypeAtPosition(new Position(j, i));

    //    //            if (type == PartType.HEAD)
    //    //            {
    //    //                _verticles.Add(new Verticle(xPos, yPos, VerticleType.HEAD));
    //    //            }

    //    //            else if (type == PartType.TAIL)
    //    //            {
    //    //                _verticles.Add(new Verticle(xPos, yPos, VerticleType.TAIL));
    //    //            }

    //    //            else if (type == PartType.BODY)
    //    //            {
    //    //                _verticles.Add(new Verticle(xPos, yPos, VerticleType.BODY));
    //    //            }
    //    //            xPos += spacing;
    //    //        }
    //    //        yPos += spacing;
    //    //        xPos = - (width - 1) * spacing / 2f;
    //    //    }
    //    //    return _verticles;
    //    //}     
    //}

