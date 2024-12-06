using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coop_Vr.Networking.ServerSide.Components
{
    public class Graph
    {
        private List<GraphPoint> graphPoints;

        public Graph(List<GraphPoint> points)
        {
            graphPoints = points;
        }

        public void Render()
        {
            var sphereTransforms = new Dictionary<GraphPoint, Pose>();
            foreach (var point in graphPoints)
            {
                sphereTransforms[point] = new Pose(new Vec3(point.X, point.Y, point.Z), Quat.Identity);
            }
            Mesh mesh = Mesh.GenerateSphere(1f);
            Model sphere = Model.FromMesh(mesh, Default.MaterialUnlit);

            while (SK.Step(() =>
            {
                DrawAxes(20f);

                foreach (var point in graphPoints)
                {
                    Pose spherePose = sphereTransforms[point];
                    float scale = CalculateScale(point);
                    Color color = Color.HSV((point.Z + 10) / 20.0f, 1.0f, 1.0f);

                    UI.Handle($"Sphere-{point.ExtraInfo.GetValueOrDefault("Country", "Unknown")}", ref spherePose, new Bounds(Vec3.One * scale));

                    sphere.Draw(Matrix.TRS(spherePose.position, Quat.Identity, scale), color);

                    Vec3 labelPosition = spherePose.position + new Vec3(0, scale + 0.01f, 0);
                    string label = point.ExtraInfo.GetValueOrDefault("Country", "Point");
                    Text.Add(label, Matrix.TRS(labelPosition, Quat.FromAngles(0, 180, 0), 10f), TextAlign.TopCenter);

                    Vec3 coordPosition = spherePose.position + new Vec3(0, -scale - 0.01f, 0);
                    string coordinates = $"({spherePose.position.x:F1}, {spherePose.position.y:F1}, {spherePose.position.z:F1})";
                    Text.Add(coordinates, Matrix.TRS(coordPosition, Quat.FromAngles(0, 180, 0), 5f), TextAlign.BottomCenter);
                }
            })) { }
        }

        private float CalculateScale(GraphPoint point)
        {
            return Math.Clamp(point.Y / 10.0f, 0.5f, 2.0f);
        }

        public void DrawAxes(float axisLimit)
        {
            Lines.Add(Vec3.Zero, new Vec3(axisLimit, 0, 0), Color.White, 0.01f); // X-axis
            Text.Add("X", Matrix.TRS(new Vec3(axisLimit, 0, 0), Quat.Identity, 5f), TextAlign.BottomCenter);

            Lines.Add(Vec3.Zero, new Vec3(0, axisLimit, 0), Color.White, 0.01f); // Y-axis
            Text.Add("Y", Matrix.TRS(new Vec3(0, axisLimit, 0), Quat.Identity, 5f), TextAlign.BottomCenter);

            Lines.Add(Vec3.Zero, new Vec3(0, 0, axisLimit), Color.White, 0.01f); // Z-axis
            Text.Add("Z", Matrix.TRS(new Vec3(0, 0, axisLimit), Quat.Identity, 5f), TextAlign.BottomCenter);
        }
    }
}
