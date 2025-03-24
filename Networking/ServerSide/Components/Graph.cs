﻿using Coop_Vr.Networking.ClientSide;
using Coop_Vr.Networking.Messages;
using Coop_Vr.Networking.ServerSide.StateMachine;
using StereoKit;
using System;
using System.Collections.Generic;
using Coop_Vr.config;

namespace Coop_Vr.Networking.ServerSide.Components
{
    public class Graph : Component
    {
        List<GraphPoint> _graphPoints;
        private List<IDictionary<string, object>> _originalRecords;
        public void SetGraphPoints(List<GraphPoint> p) => _graphPoints = p;
        public const float POSITION_SCALE = 10f;

        public void SetOrigialRecords(List<IDictionary<string, object>> original)
        {
            _originalRecords = original;
        }

        public void GenerateGraphPoints()
        {
            foreach (var graphPoint in _graphPoints)
            {
                //is added automatically to the scene
                MakeGraphPointObject(graphPoint);
            }
        }

        SkObject MakeGraphPointObject(GraphPoint graphPoint)
        {
            var moveComponent = new Move();

            var obj = new SkObject
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

            return obj;
        }

        public (float X, float Z) ReverseScale(GraphPoint point, float X, float Z, float scale)
        {
            float xMin = float.Parse(point.ExtraInfo["xMin"]);
            float xMax = float.Parse(point.ExtraInfo["xMax"]);
            float zMin = float.Parse(point.ExtraInfo["zMin"]);
            float zMax = float.Parse(point.ExtraInfo["zMax"]);

            X = X * (xMax - xMin) / scale + xMin;
            Z = Z * (zMax - zMin) / scale + zMin;
            return (X, Z);
        }

        // Handle moving objects through the event
        void OnMove(Move move, MoveRequestResponse msg)
        {
            move.gameObject.Transform.LocalPosition = msg.Position.LocalPosition;
            var movingPoint = move.gameObject.GetComponent<GraphPoint>();

            // Process the final position when the object stops moving
            if (msg.stopped)
            {
                //Console.WriteLine("In this if already");
                ProcessGraphPointUpdate(movingPoint, msg.Position.LocalPosition);
            }
            else
            {
                // Optionally process intermediate positions while object is still moving
                // You might want to skip this to avoid excessive updates
                // ProcessGraphPointUpdate(movingPoint, msg.Position.pose.position);
            }
        }

        // Process updates for a graph point
        private void ProcessGraphPointUpdate(GraphPoint point, Vec3 position)
        {
            // Get X and Z values from the position
            var scaledCapacity = position.x * POSITION_SCALE;
            var scaledInvestment = position.z * POSITION_SCALE;

            // Reverse scale to get original values
            (float unscaledX, float unscaledZ) = ReverseScale(point, scaledCapacity, scaledInvestment, 1f);

            // Update the point's data
            point.ExtraInfo["Capacity1"] = unscaledX.ToString();
            point.ExtraInfo["Investment1"] = unscaledZ.ToString();

            // Update the stored original data
            UpdateOriginalData(point);

            // Recalculate influence scores and other derived values
        }

        // Update original data for a specific point
        private void UpdateOriginalData(GraphPoint graphPoint)
        {
            var FileHandler = new FileHandler();
            var records = new List<IDictionary<string, object>>();
            try
            {
                foreach (var record in _originalRecords)
                {
                    if (record != null)
                    {
                        // Check if the record matches the graphPoint's ExtraInfo
                        if (record["Project Name"].ToString() == graphPoint.ExtraInfo["Project1"] &&
                            record["Technology"].ToString() == graphPoint.ExtraInfo["Technology1"] &&
                            record["Country"].ToString() == graphPoint.ExtraInfo["Country1"] &&
                            record["Date Online"].ToString() == graphPoint.ExtraInfo["Date1"])
                        {
                            // Update the record with the new values from graphPoint
                            record["Capacity (kt H2/y)"] = graphPoint.ExtraInfo["Capacity1"];
                            record["Investment Cost (MUSD)"] = graphPoint.ExtraInfo["Investment1"];
                            records.Add(record);
                        }
                        else
                        {
                            records.Add(record);
                        }
                    }
                }

                // Calculate influence scores using XRFomula
                var influenceScores = FileHandler.CalculateInfluenceScore(records, config.Config.NUMBEROFPROJECT);
                // Removing old objects
                foreach (var p in _graphPoints)
                {
                    p.gameObject.Destroy();
                    ServerStateMachine.MessageSender.SendMessage(new RemoveMsg() { ID = p.gameObject.ID, ParentID = gameObject.ID });
                }

                _graphPoints.Clear();
                foreach (var score in influenceScores)
                {
                    //remember that it will not remove the old objects
                    var newPoint = GraphPoint.FromCsvRecord(score);
                    _graphPoints.Add(newPoint);
                }

                // Update the graph points
                FileHandler.ScaleGraphPoints(_graphPoints, config.Config.SCALE);

                foreach (GraphPoint point in _graphPoints)
                {
                    MakeGraphPointObject(point);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating dataset: {ex.Message}");
            }
        }

        public override void Update()
        {
            DrawAxes(1f);
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
