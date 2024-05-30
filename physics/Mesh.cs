using MyGame.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;

namespace MyGame.physics
{
    // 材质结构体
    public struct Material
    {
        public Vector3 ambient;  // 环境光
        public Vector3 diffuse;  // 漫反射
        public Vector3 specular;  // 镜面反射
        public float shininess;  // 光泽度
    }

    public enum PrimitiveType
    {
        PointList,
        LineList,
        TriangleList,
        QuadList,
        LineStrip
    }

    internal class CollisionBox
    {
        public bool isIntersect(CollisionBox other)
        {
            return false;
        }
    }

    internal class AABB: CollisionBox
    {
        public Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public AABB()
        {
        }

        public void update(Vector3 vertex)
        {
            min.X = Math.Min(min.X, vertex.X);
            min.Y = Math.Min(min.Y, vertex.Y);
            min.Z = Math.Min(min.Z, vertex.Z);
            max.X = Math.Max(max.X, vertex.X);
            max.Y = Math.Max(max.Y, vertex.Y);
            max.Z = Math.Max(max.Z, vertex.Z);
        }

        public bool isIntersect(AABB other)
        {
            return min.X <= other.max.X && max.X >= other.min.X &&
                min.Y <= other.max.Y && max.Y >= other.min.Y &&
                min.Z <= other.max.Z && max.Z >= other.min.Z;
        }
    }


    internal class SphereBB: CollisionBox
    {
        public Vector3 center = new Vector3(0, 0, 0);
        public float radius = 0;

        public SphereBB(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public SphereBB()
        {
        }

        public void update(Vector3 vertex)
        {
            center = (center + vertex) / 2;
            radius = Math.Max(radius, Vector3.Distance(center, vertex));
        }

        public bool isIntersect(SphereBB other)
        {
            return Vector3.Distance(center, other.center) <= radius + other.radius;
        }
    }

    public class Mesh
    {
        private Material material = new Material();
        private List<Light> lights = new List<Light>();  // 光源
        private Color lineColor = Color.White;  // 线条颜色
        private float lineWidth = 1.5f;  // 线条宽度

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> indices = new List<int>();
        // 锁
        // 采用的图元
        private PrimitiveType primitiveType = PrimitiveType.TriangleList;

        public Mesh()
        {
            this.initMaterial(new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0.0f, 0.0f, 0.0f), 32.0f);
        }

        public Mesh(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
            this.initMaterial(new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0.0f, 0.0f, 0.0f), 32.0f);
        }

        public Mesh(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, PrimitiveType primitiveType=PrimitiveType.TriangleList)
        {
            this.primitiveType = primitiveType;
            this.vertices = vertices;  // 顶点
            this.normals = normals;  // 法向量
            this.uvs = uvs;  // 纹理坐标
            this.indices = indices;  // 顶点索引
            this.initMaterial(new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0.0f, 0.0f, 0.0f), 32.0f);
        }

        public void initMaterial(Vector3 ambient, Vector3 diffuse, Vector3 specular, float shininess)
        {
            material.ambient = ambient;
            material.diffuse = diffuse;
            material.specular = specular;
            material.shininess = shininess;
        }

        public void paint(BufferedGraphics graphics, Vector3 scale, Matrix4x4 transform, Matrix4x4 cameraTransform, float[,] depthBuffer, int[,] indexBuffer, float near = 0.1f, float far = 1000)
        {
            // 创建缩放矩阵
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scale);

            // 将顶点变换到世界坐标系，并应用缩放和投影变换
            List<Vector4> vertices4 = getVertices()
                .Select(v => new Vector4(v, 1))
                .Select(v => Vector4.Transform(v, scaleMatrix))
                .Select(v => Vector4.Transform(v, transform))
                .Select(v => Vector4.Transform(v, cameraTransform))
                .ToList();

            // 获取当前窗口的宽度和高度
            int width = MainWin.Instance.toolStripContainer1.ContentPanel.DisplayRectangle.Width;
            int height = MainWin.Instance.toolStripContainer1.ContentPanel.DisplayRectangle.Height;

            switch (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    paintTriangleList(graphics, vertices4, indices, near, far, width, height);
                    break;
                case PrimitiveType.LineList:
                    paintLineList(graphics, vertices4, indices, near, far, width, height);
                    break;
                case PrimitiveType.LineStrip:
                    paintLineStrip(graphics, vertices4, indices, near, far, width, height);
                    break;
                case PrimitiveType.PointList:
                    paintPointList(graphics, vertices4, near, far, width, height);
                    break;
                case PrimitiveType.QuadList:
                    paintQuadList(graphics, vertices4, indices, near, far, width, height);
                    break;
            }
        }

        private void paintTriangleList(BufferedGraphics graphics, List<Vector4> vertices4, List<int> indices, float near, float far, int width, int height)
        {
            for (int i = 0; i < indices.Count; i += 3)
            {
                Vector4 v1 = vertices4[indices[i]];
                Vector4 v2 = vertices4[indices[i + 1]];
                Vector4 v3 = vertices4[indices[i + 2]];

                // 裁剪
                if (v1.Z < near || v1.Z > far || v2.Z < near || v2.Z > far || v3.Z < near || v3.Z > far)
                {
                    continue;
                }

                // 透视除法，转换到屏幕空间
                Vector3 p1 = new Vector3(v1.X / v1.W, v1.Y / v1.W, v1.Z / v1.W);
                Vector3 p2 = new Vector3(v2.X / v2.W, v2.Y / v2.W, v2.Z / v2.W);
                Vector3 p3 = new Vector3(v3.X / v3.W, v3.Y / v3.W, v3.Z / v3.W);

                // 屏幕坐标
                Vector3 s1 = new Vector3((p1.X + 1) * width / 2, (1 - p1.Y) * height / 2, p1.Z);
                Vector3 s2 = new Vector3((p2.X + 1) * width / 2, (1 - p2.Y) * height / 2, p2.Z);
                Vector3 s3 = new Vector3((p3.X + 1) * width / 2, (1 - p3.Y) * height / 2, p3.Z);

                // 裁剪
                if ((s1.X < 0 && s2.X < 0 && s3.X < 0) || (s1.X > width && s2.X > width && s3.X > width) || (s1.Y < 0 && s2.Y < 0 && s3.Y < 0) || (s1.Y > height && s2.Y > height && s3.Y > height)) { continue; }

                // 计算法线
                Vector3 v1v2 = new Vector3(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
                Vector3 v1v3 = new Vector3(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
                Vector3 normal = Vector3.Normalize(Vector3.Cross(v1v2, v1v3));
                // 遍历mesh.lights并计算光照强度
                float intensity = 0;
                foreach (Light light in getLights())
                {
                    Vector3 lightDirection = Vector3.Normalize(light.position - (new Vector3(v1.X, v1.Y, v1.Z) + new Vector3(v2.X, v2.Y, v2.Z) + new Vector3(v3.X, v3.Y, v3.Z)) / 3);
                    intensity += Math.Max(0, Vector3.Dot(normal, lightDirection));
                }
                intensity = Math.Min(intensity, 1); // 确保强度在0到1之间
                Color color = Color.FromArgb((int)(255 * intensity), (int)(255 * intensity), (int)(255 * intensity));
                // 在Graphics上绘制实心三角形
                graphics.Graphics.FillPolygon(new SolidBrush(color), new PointF[] { new PointF(s1.X, s1.Y), new PointF(s2.X, s2.Y), new PointF(s3.X, s3.Y) });

                // 绘制线框，圆形线头
                Pen _pen = new Pen(lineColor, lineWidth);
                graphics.Graphics.DrawPolygon(
                    _pen,
                    new PointF[] { new PointF(s1.X, s1.Y), new PointF(s2.X, s2.Y), new PointF(s3.X, s3.Y) }
                );
            }
        }

        private void paintLineList(BufferedGraphics graphics, List<Vector4> vertices4, List<int> indices, float near, float far, int width, int height)
        {
            for (int i = 0; i < indices.Count; i += 2)
            {
                Vector4 v1 = vertices4[indices[i]];
                Vector4 v2 = vertices4[indices[i + 1]];

                // 裁剪
                if (v1.Z < near || v1.Z > far || v2.Z < near || v2.Z > far)
                {
                    continue;
                }

                // 透视除法，转换到屏幕空间
                Vector3 p1 = new Vector3(v1.X / v1.W, v1.Y / v1.W, v1.Z / v1.W);
                Vector3 p2 = new Vector3(v2.X / v2.W, v2.Y / v2.W, v2.Z / v2.W);

                // 屏幕坐标
                Vector3 s1 = new Vector3((p1.X + 1) * width / 2, (1 - p1.Y) * height / 2, p1.Z);
                Vector3 s2 = new Vector3((p2.X + 1) * width / 2, (1 - p2.Y) * height / 2, p2.Z);

                // 裁剪
                if ((s1.X < 0 && s2.X < 0) || (s1.X > width && s2.X > width) || (s1.Y < 0 && s2.Y < 0) || (s1.Y > height && s2.Y > height)) { continue; }

                // 在Graphics上绘制线段
                Pen _pen = new Pen(lineColor, lineWidth);
                graphics.Graphics.DrawLine(_pen, s1.X, s1.Y, s2.X, s2.Y);
            }
        }

        private void paintLineStrip(BufferedGraphics graphics, List<Vector4> vertices4, List<int> indices, float near, float far, int width, int height)
        {
            for (int i = 0; i < indices.Count - 1; i++)
            {
                Vector4 v1 = vertices4[indices[i]];
                Vector4 v2 = vertices4[indices[i + 1]];

                // 裁剪
                if (v1.Z < near || v1.Z > far || v2.Z < near || v2.Z > far)
                {
                    continue;
                }

                // 透视除法，转换到屏幕空间
                Vector3 p1 = new Vector3(v1.X / v1.W, v1.Y / v1.W, v1.Z / v1.W);
                Vector3 p2 = new Vector3(v2.X / v2.W, v2.Y / v2.W, v2.Z / v2.W);

                // 屏幕坐标
                Vector3 s1 = new Vector3((p1.X + 1) * width / 2, (1 - p1.Y) * height / 2, p1.Z);
                Vector3 s2 = new Vector3((p2.X + 1) * width / 2, (1 - p2.Y) * height / 2, p2.Z);

                // 裁剪
                if ((s1.X < 0 && s2.X < 0) || (s1.X > width && s2.X > width) || (s1.Y < 0 && s2.Y < 0) || (s1.Y > height && s2.Y > height)) { continue; }

                // 在Graphics上绘制线段
                Pen _pen = new Pen(lineColor, lineWidth);
                graphics.Graphics.DrawLine(_pen, s1.X, s1.Y, s2.X, s2.Y);
            }
        }

        private void paintPointList(BufferedGraphics graphics, List<Vector4> vertices4, float near, float far, int width, int height)
        {
            foreach (Vector4 v in vertices4)
            {
                // 裁剪
                if (v.Z < near || v.Z > far)
                {
                    continue;
                }

                // 透视除法，转换到屏幕空间
                Vector3 p = new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);

                // 屏幕坐标
                Vector3 s = new Vector3((p.X + 1) * width / 2, (1 - p.Y) * height / 2, p.Z);

                // 裁剪
                if (s.X < 0 || s.X > width || s.Y < 0 || s.Y > height) { continue; }

                // 在Graphics上绘制小圆形
                Brush _brush = new SolidBrush(lineColor);
                graphics.Graphics.FillEllipse(_brush, s.X - lineWidth / 2, s.Y - lineWidth / 2, lineWidth, lineWidth);
            }
        }

        private void paintQuadList(BufferedGraphics graphics, List<Vector4> vertices4, List<int> indices, float near, float far, int width, int height)
        {
            for (int i = 0; i < indices.Count; i += 4)
            {
                Vector4 v1 = vertices4[indices[i]];
                Vector4 v2 = vertices4[indices[i + 1]];
                Vector4 v3 = vertices4[indices[i + 2]];
                Vector4 v4 = vertices4[indices[i + 3]];

                // 裁剪
                if (v1.Z < near || v1.Z > far || v2.Z < near || v2.Z > far || v3.Z < near || v3.Z > far || v4.Z < near || v4.Z > far)
                {
                    continue;
                }

                // 透视除法，转换到屏幕空间
                Vector3 p1 = new Vector3(v1.X / v1.W, v1.Y / v1.W, v1.Z / v1.W);
                Vector3 p2 = new Vector3(v2.X / v2.W, v2.Y / v2.W, v2.Z / v2.W);
                Vector3 p3 = new Vector3(v3.X / v3.W, v3.Y / v3.W, v3.Z / v3.W);
                Vector3 p4 = new Vector3(v4.X / v4.W, v4.Y / v4.W, v4.Z / v4.W);

                // 屏幕坐标
                Vector3 s1 = new Vector3((p1.X + 1) * width / 2, (1 - p1.Y) * height / 2, p1.Z);
                Vector3 s2 = new Vector3((p2.X + 1) * width / 2, (1 - p2.Y) * height / 2, p2.Z);
                Vector3 s3 = new Vector3((p3.X + 1) * width / 2, (1 - p3.Y) * height / 2, p3.Z);
                Vector3 s4 = new Vector3((p4.X + 1) * width / 2, (1 - p4.Y) * height / 2, p4.Z);
                
                // 裁剪
                if ((s1.X < 0 && s2.X < 0 && s3.X < 0 && s4.X < 0) || (s1.X > width && s2.X > width && s3.X > width && s4.X > width) || (s1.Y < 0 && s2.Y < 0 && s3.Y < 0 && s4.Y < 0) || (s1.Y > height && s2.Y > height && s3.Y > height && s4.Y > height)) { continue; }

                // 在Graphics上绘制
                Pen _pen = new Pen(lineColor, lineWidth);
                graphics.Graphics.DrawPolygon(_pen, new PointF[] { new PointF(s1.X, s1.Y), new PointF(s2.X, s2.Y), new PointF(s3.X, s3.Y), new PointF(s4.X, s4.Y) });
            }
        }

        public void setLineColor(Color color) { lineColor = color; }
        public void setLineWidth(float width) { lineWidth = width; }
        public Material getMaterial() { return material; }
        public void setMaterial(Material material) { this.material = material; }
        public List<Light> getLights() { return lights; }
        public void setLights(List<Light> lights) { this.lights = lights; }
        public void addLight(Light light) { lights.Add(light); }
        public void removeLight(Light light) { if (lights.Contains(light)) lights.Remove(light); }
        public List<Vector3> getVertices() { return vertices; }
        public void setVertices(List<Vector3> v) { vertices = v; }
        public List<Vector3> getNormals() { return normals; }
        public void setNormals(List<Vector3> normals) { this.normals = normals; }
        public List<Vector2> getUVs() { return uvs; }
        public void setUVs(List<Vector2> uvs) { this.uvs = uvs; }
        public List<int> getIndices() { return indices; }
        public void setIndices(List<int> indices) { this.indices = indices; }
        public void setPrimitiveType(PrimitiveType primitiveType) { this.primitiveType = primitiveType; }
        public PrimitiveType getPrimitiveType() { return primitiveType; }
    }
}
