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
        string playerName;
        string playerScore;
        List<Score> scores;

        Pose windowPose = new Pose(0, 0, 0, Quat.Identity);
        string text = "Edit me";
        string number = "1";

        Pose windowPoseButton = new Pose(0, 0, 0, Quat.Identity);
        public event Action<Score> PlayerScoreAdd;

        public DemoShow()
        {
            PlayerScoreAdd += PlayerScoreAdd;
        }

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

            UI.HSeparator();
            UI.Label(playerName, labelSize);

            foreach (var score in scores)
            {
                UI.Label(score.name, labelSize);
                UI.SameLine();
                UI.Label(score.score.ToString(), labelSize);
                UI.NextLine();
            }

            UI.WindowEnd();
        }

        void SetPlayerScore(Score score)
        {
            // change the text here
            playerName = score.name;
            playerScore = score.score.ToString();
        }

        public void SetHighScores(List<Score> scores)
        {
            this.scores = scores;
        }
    }

}
