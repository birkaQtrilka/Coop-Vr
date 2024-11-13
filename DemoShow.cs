using System;
using System.Collections.Generic;
using Coop_Vr.Networking;
using StereoKit;

namespace Coop_Vr
{
    public class DemoShow
    {
        string title = "Text Input";
        string description = "";


        Pose windowPose = new Pose(0, 0, 0, Quat.Identity);
        string text = "Edit me";
        string number = "1";

        Pose windowPoseButton = new Pose(0, 0, 0, Quat.Identity);
        public event Action<Score> PlayerScoreAdd;
        public void Step()
        {
            UI.WindowBegin("Text Input", ref windowPose);

            UI.HSeparator();

            Vec2 inputSize = V.XY(20 * U.cm, 0);
            Vec2 labelSize = V.XY(8 * U.cm, 0);
            UI.Label("Username", labelSize); UI.SameLine(); UI.Input("Username", ref text, inputSize, TextContext.Text);
            UI.Label("Score", labelSize); UI.SameLine(); UI.Input("Score", ref number, inputSize, TextContext.Number);

            UI.HSeparator();

            if (UI.Button("Add"))
                PlayerScoreAdd?.Invoke(new Score(text, int.Parse(number)));


            UI.WindowEnd();
        }

        public void SetPlayerScore(int score)
        {

        }

        public void SetHighScores(List<Score> scores)
        { 
        
        }
    }

}
