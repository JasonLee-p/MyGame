using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.physics.GameObjects
{
    internal class Surface : GameObject
    {
        public Surface(PrimitiveType primitiveType, bool solid, Vector3 position, Vector3 scale) : base(solid, new Mesh(), 1, position)
        {
            mesh.setVertices(new List<Vector3>
            {
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(-0.5f, 0, 0.5f),
            });
            setPrimitiveType(primitiveType);
            this.scale = scale;
        }

        public void setPrimitiveType(PrimitiveType primitiveType)
        {
            mesh.setPrimitiveType(primitiveType);
            switch
                (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    mesh.setIndices(new List<int> { 0, 1, 2, 0, 2, 3 });
                    break;
                case PrimitiveType.QuadList:
                    mesh.setIndices(new List<int> { 0, 1, 2, 3 });
                    break;
                case PrimitiveType.LineList:
                    mesh.setIndices(new List<int> { 0, 1, 1, 2, 2, 3, 3, 0 });
                    break;
                case PrimitiveType.LineStrip:
                    mesh.setIndices(new List<int> { 0, 1, 2, 3, 0 });
                    break;
                case PrimitiveType.PointList:
                    mesh.setIndices(new List<int> { 0, 1, 2, 3 });
                    break;
            }
        }
    }

    internal class Cube : GameObject
    {
        public Cube(bool solid, Vector3 position, Vector3 scale) : base(solid, new Mesh(), 1, position)
        {
            mesh.setVertices(new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
            });
            this.scale = scale;
            setPrimitiveType(PrimitiveType.QuadList);
        }
        public Cube(PrimitiveType primitiveType, bool solid, Vector3 position, Vector3 scale) : base(solid, new Mesh(), 1, position)
        {
            mesh.setVertices(new List<Vector3>
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
            });
            setPrimitiveType(primitiveType);
            this.scale = scale;
        }

        public void setPrimitiveType(PrimitiveType primitiveType)
        {
            mesh.setPrimitiveType(primitiveType);
            switch
                (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    mesh.setIndices(new List<int>
                    {
                        0, 1, 2, 0, 2, 3,
                        1, 5, 6, 1, 6, 2,
                        5, 4, 7, 5, 7, 6,
                        4, 0, 3, 4, 3, 7,
                        3, 2, 6, 3, 6, 7,
                        4, 5, 1, 4, 1, 0
                    });
                    break;
                case PrimitiveType.QuadList:
                    mesh.setIndices(new List<int>
                    {
                        0, 1, 2, 3,
                        4, 5, 6, 7,
                        0, 4, 5, 1,
                        1, 5, 6, 2,
                        2, 6, 7, 3,
                        3, 7, 4, 0
                    });
                    break;
                case PrimitiveType.LineList:
                    mesh.setIndices(new List<int> { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 });
                    break;
                case PrimitiveType.LineStrip:
                    mesh.setIndices(new List<int> { 0, 1, 2, 3, 0, 4, 5, 6, 7, 4, 0, 1, 5, 2, 6, 3, 7 });
                    break;
                case PrimitiveType.PointList:
                    mesh.setIndices(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 });
                    break;
            }
        }
    }

    internal class TriangleSurface : GameObject
    {
        public TriangleSurface(bool solid, Vector3 position, List<Point> dots, Vector3 scale) : base(solid, new Mesh(), 1, position)
        {
            mesh.setVertices(new List<Vector3>
            {
                new Vector3(dots[0].X, 0, dots[0].Y),
                new Vector3(dots[1].X, 0, dots[1].Y),
                new Vector3(dots[2].X, 0, dots[2].Y),
            });
            setPrimitiveType(PrimitiveType.TriangleList);
            this.scale = scale;
        }

        public void setPrimitiveType(PrimitiveType primitiveType)
        {
            mesh.setPrimitiveType(primitiveType);
            switch
                (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    mesh.setIndices(new List<int> { 0, 1, 2 });
                    break;
                case PrimitiveType.QuadList:
                    mesh.setIndices(new List<int> { 0, 1, 2 });
                    break;
                case PrimitiveType.LineList:
                    mesh.setIndices(new List<int> { 0, 1, 1, 2, 2, 0 });
                    break;
                case PrimitiveType.LineStrip:
                    mesh.setIndices(new List<int> { 0, 1, 2, 0 });
                    break;
                case PrimitiveType.PointList:
                    mesh.setIndices(new List<int> { 0, 1, 2 });
                    break;
            }
        }
    }

    internal class SphereParticle : GameObject
    {
        private int count = 500;
        private bool dynamic;
        Random random = new Random();

        public SphereParticle(bool solid, bool dynamic, Vector3 position, float radius) : base(solid, new Mesh(), 1, position)
        {
            generateRandomPoints(count, random);
            this.dynamic = dynamic;
            mesh.setPrimitiveType(PrimitiveType.PointList);
            mesh.setIndices(new List<int> { });
            this.scale = new Vector3(radius, radius, radius);
        }

        public SphereParticle(bool solid, bool dynamic, Vector3 position, float radius, int count) : base(solid, new Mesh(), 1, position)
        {
            this.count = count;
            generateRandomPoints(count, random);
            this.dynamic = dynamic;
            mesh.setPrimitiveType(PrimitiveType.PointList);
            mesh.setIndices(new List<int> { });
            this.scale = new Vector3(radius, radius, radius);
        }

        public void setColor(Color color) { mesh.setLineColor(color); }
        public void setLineWidth(float width) { mesh.setLineWidth(width); }

        public void generateRandomPoints(int count, Random random)
        {
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                // 生成中心为 (0,0,0)，接近正态分布的球云，3σ = radius
                vertices.Add(GeneratePoint(random));
            }
            mesh.setVertices(vertices);
        }

        private Vector3 GeneratePoint(Random random)
        {
            // Generate random values for spherical coordinates
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();
            float theta = 2.0f * MathF.PI * u; // Azimuthal angle
            float phi = MathF.Acos(2.0f * v - 1.0f); // Polar angle

            // Box-Muller transform to generate normally distributed points
            float u1 = 1.0f - (float)random.NextDouble();
            float u2 = 1.0f - (float)random.NextDouble();
            float randStdNormal = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2); // Random normal(0,1)

            // Scale the normally distributed random number to fit the desired radius
            float r = (randStdNormal / 3.0f); // 3σ = radius

            // Convert spherical coordinates to Cartesian coordinates
            float x = r * MathF.Sin(phi) * MathF.Cos(theta);
            float y = r * MathF.Sin(phi) * MathF.Sin(theta);
            float z = r * MathF.Cos(phi);

            return new Vector3(x, y, z);
        }

        // 覆盖update函数
        public override void update(float time)
        {
            if (dynamic)
            {
                generateRandomPoints(count, random);
            }
            base.update(time);
        }
    }
}
