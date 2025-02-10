using Coop_Vr.Networking.ServerSide.Components;
using Coop_Vr.Networking.ServerSide.StateMachine.States;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.Scenes
{
    public class GraphScene : Scene
    {
        public GraphScene(GameRoom room) : base(room)
        {
        }

        public override void OnStart()
        {
            Log.Do("Entered game room\nMember count: " + room.MemberCount());

            string filePath = "Assets\\Documents\\sample_data_3.csv";

            // Create a FileHandler instance
            var fileHandler = new FileHandler(filePath);

            // Read graph points from the CSV file
            var graphPoints = fileHandler.ReadGraphPointsFromCsv();

            // Scale the graph points
            fileHandler.ScaleGraphPoints(graphPoints, 50f);
            var graph = new Graph();
            graph.SetGraphPoints(graphPoints);

            SkObject graphHolder = new(
                components: new List<Component>() { new PosComponent(), graph }
            );

            graph.GenerateGraphPoints();

            var response = new CreateObjectMsg() { NewObj = graphHolder, ParentID = -1 };
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                room.Context.SendMessage(response);

            });
        }

        public override void OnStop()
        {
        }
    }
}
