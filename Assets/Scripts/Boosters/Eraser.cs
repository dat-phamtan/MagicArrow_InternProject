using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Boosters
{
    public class Eraser : IBooster
    {
        //public event Action OnArrowClicked;
        public Eraser() { }

        public void OnClick(IController controller, Action onComplete)
        {
            //hide topbar and boosters bar
            controller.ChangeEraserUsedMode();
            
            //show notification bar
            //disable arrow in boardmatrix
            //play delete animation <-- just simple for now ;))) --> disapear
        }
    }
}
