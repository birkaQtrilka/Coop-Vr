using Coop_Vr.Networking.ClientSide;
using Coop_Vr.Networking.Messages;
using Coop_Vr.Networking.ServerSide.StateMachine;
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

                moveComponent.OnMove += OnMove;
            }
        }

        // When moving the object this function will be called
        void OnMove(Move move, MoveRequestResponse msg)
        {
            // Get the GraphPoint component of the moving object
            var movingPoint = move.gameObject.GetComponent<GraphPoint>();

            // File path for data (if needed for further processing)
            string filePath = "Assets\\Documents\\europe_data.csv";

            // Create a FileHandler instance
            var fileHandler = new FileHandler(filePath);

            // Get the new position of the moving object
            var moverPos = msg.Position.pose.position;

           


            // TODO: Get the exact position of Moving Point after moving
            // TODO: Get X and Z values from the new position
            // Example of how to use the new position and reverse scale
            var scaledCapacity = moverPos.x; // X - Capacity1
            var scaledInvestment = moverPos.z; // Z - Investment1

            // Reverse scale the position (if needed)
            (float unscaledX, float unscaledZ) = fileHandler.ReverseScale(movingPoint, scaledCapacity, scaledInvestment, 10f);

            // Log the new position
            Log.Do(unscaledX);
            Log.Do(unscaledZ);

            // TODO: Update new X and Z values to the dataset
            // TODO: Recalculate the Y value based on the new X and Z values
            // TODO: Update graph points based on the new values

            // Update other points based on the moving point
            List<IMessage> msgList = new();
            foreach (GraphPoint point in _graphPoints)
            {
                if (point == movingPoint) continue;

                // Update the position of the other point based on the moving point
                point.gameObject.Transform.pose = new Pose(
                    moverPos.x + .1f,
                    moverPos.y - .1f,
                    moverPos.z + .1f,
                    Quat.Identity
                );

                msgList.Add(SendMessage(move, msg, point));
            }

            // Send messages to other members in the current room
            ServerStateMachine.Instance.CurrentRoom.SafeForEachMember(m =>
            {
                foreach (var msg in msgList) m.SendMessage(msg);
            });
        }

        IMessage SendMessage(Move move, MoveRequestResponse msg, GraphPoint point)
        {
            if (msg.stopped)//terminating ownership
                move.MoverClientID = -1;
            else//claiming / continuing ownership 
                move.MoverClientID = msg.SenderID; // server is owner

            var response = new MoveRequestResponse()
            {
                ObjectID = point.gameObject.ID,
                SenderID = msg.SenderID,
                Position = point.gameObject.Transform,
                stopped = msg.stopped,
                alsoMoveSender = true,
            };
            return response;
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
