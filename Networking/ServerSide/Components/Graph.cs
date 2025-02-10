using Coop_Vr.Networking.ClientSide;
using StereoKit;
using System;
using System.Collections.Generic;

namespace Coop_Vr.Networking.ServerSide.Components
{
    public class Graph : Component
    {
        List<GraphPoint> _graphPoints;

        public void SetGraphPoints(List<GraphPoint> p) => _graphPoints = p;

        public void GenerateGraphPoints()
        {
            foreach (var graphPoint in _graphPoints)
            {
                //is added automatically to the scene
                var moveComponent = new Move();

                _ = new SkObject
                (
                    parentID: gameObject.ID,
                    components: new List<Component>
                    {
                        new PosComponent(),
                        graphPoint,
                        new ModelComponent
                        {
                            MeshName = "sphere",
                        },
                        moveComponent
                    }
                );

                moveComponent.OnMove += onMove;
            }
        }

        // When moving the object this function will be called
        void onMove(Move move)
        {
            var movingPoint = move.gameObject.GetComponent<GraphPoint>();
            float threshold = 5.0f; // Example threshold

            foreach (var point in _graphPoints)
            {
                if (point != movingPoint)
                {
                    // Calculate distance between movingPoint and point
                    float distance = MathF.Sqrt(
                        MathF.Pow(movingPoint.X - point.X, 2) +
                        MathF.Pow(movingPoint.Y - point.Y, 2) +
                        MathF.Pow(movingPoint.Z - point.Z, 2)
                    );

                    // Trigger an event if the distance is below the threshold
                    if (distance < threshold)
                    {
                        Log.Do($"Points are within {threshold} units: {distance}");
                        // Add your event handling code here
                    }
                }
            }
        }

        public override void Update()
        {
            DrawAxes(10);
        }

        void DrawAxes(float axisLimit)
        {
            Lines.Add(Vec3.Zero, new Vec3(axisLimit, 0, 0), Color.White, 0.01f); // X-axis
            Text.Add("X", Matrix.TRS(new Vec3(axisLimit, 0, 0), Quat.Identity, 5f), TextAlign.BottomCenter);

            Lines.Add(Vec3.Zero, new Vec3(0, axisLimit, 0), Color.White, 0.01f); // Y-axis
            Text.Add("Y", Matrix.TRS(new Vec3(0, axisLimit, 0), Quat.Identity, 5f), TextAlign.BottomCenter);

            Lines.Add(Vec3.Zero, new Vec3(0, 0, axisLimit), Color.White, 0.01f); // Z-axis
            Text.Add("Z", Matrix.TRS(new Vec3(0, 0, axisLimit), Quat.Identity, 5f), TextAlign.BottomCenter);
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(_graphPoints.Count);
            foreach (var item in _graphPoints)
            {
                item.Serialize(pPacket);
            }
        }

        public override void Deserialize(Packet pPacket)
        {
            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                var p = new GraphPoint();
                p.Deserialize(pPacket);
                _graphPoints.Add(p);
            }
        }
    }
}
