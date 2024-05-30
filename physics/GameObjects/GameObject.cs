using MyGame.game;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.ES20;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;

namespace MyGame.physics.GameObjects
{
    public class GameObject
    {
        // 锁
        private Mutex updateMutex = new Mutex();
        // 类属性（非实例属性）
        public static Dictionary<int, GameObject> idMap = new Dictionary<int, GameObject>();  // id映射
        // id
        private int id = 0;
        // 图形
        public Mesh mesh = new Mesh();  // 网格
        public Vector3 scale = new Vector3(1, 1, 1);  // 缩放
        // 物理属性
        public Matrix4x4 inertiaTensor;
        public Matrix4x4 inertiaTensorInverse;

        private bool solid;
        private List<Force> forces = new List<Force>();  // 力集
        private Gravity gravity = new Gravity(25);  // 重力
        private float friction = 0.01f;  // 摩擦力
        private float angularf = 0.01f;  // 角阻尼
        private Vector3 velocity = new Vector3(0, 0, 0);  // 速度
        private Vector3 acceleration = new Vector3(0, 0, 0);  // 加速度
        private Matrix4x4 transform = Matrix4x4.Identity;  // 位置以及旋转
        private Vector3 angularVelocity = new Vector3(0, 0, 0);  // 角速度
        private Vector3 angularAcceleration = new Vector3(0, 0, 0);  // 角加速度


        public GameObject(bool solid, Mesh mesh, float mass, Vector3 position, Vector3 rotation)
        {
            arrangeID();
            this.solid = solid;
            gravity.setMass(mass);
            this.mesh = mesh;
            transform = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            transform.M41 = position.X;
            transform.M42 = position.Y;
            transform.M43 = position.Z;
        }

        public GameObject(bool solid, Mesh mesh, float mass, Vector3 position)
        {
            arrangeID();
            this.solid = solid;
            gravity.setMass(mass);
            this.mesh = mesh;
            transform.M41 = position.X;
            transform.M42 = position.Y;
            transform.M43 = position.Z;
        }

        private void arrangeID()
        {
            // 随机生成整数，作为ID
            Random random = new Random();
            int id = random.Next(1, int.MaxValue);
            // 如果ID已经存在，则重新生成
            while (idMap.ContainsKey(id))
            {
                id = random.Next(1, int.MaxValue);
            }
            this.id = id;
            idMap.Add(id, this);
        }
        public int getId()
        {
            return id;
        }

        public void InitializeInertiaTensor()
        {
            // 假设物体质心在原点且均匀分布
            inertiaTensor = new Matrix4x4(
                gravity.mass * (scale.Y * scale.Y + scale.Z * scale.Z) / 12, 0, 0, 0,
                0, gravity.mass * (scale.X * scale.X + scale.Z * scale.Z) / 12, 0, 0,
                0, 0, gravity.mass * (scale.X * scale.X + scale.Y * scale.Y) / 12, 0,
                0, 0, 0, 1
            );

            // 计算惯性张量的逆矩阵
            Matrix4x4.Invert(inertiaTensor, out inertiaTensorInverse);
        }

        public virtual void update(float time)
        {
            if (solid) return;

            // 计算总外力
            Vector3 totalForce = Vector3.Zero;
            Vector3 torque = Vector3.Zero;
            foreach (var force in forces)
            {
                totalForce += force.get();
                torque += Vector3.Cross(force.getPos() - getPos(), force.get());
            }
            totalForce += gravity.get();

            // 计算线性加速度 F = ma
            acceleration = totalForce / gravity.mass;

            // 更新速度 v = u + at
            velocity += acceleration * time;

            // 计算摩擦力并应用
            velocity *= 1 - friction;

            // 更新位置 s = ut + 0.5 * a * t^2
            Vector3 displacement = velocity * time + 0.5f * acceleration * time * time;

            // 计算角加速度 τ = I * α (τ是合力矩，I是惯性张量，α是角加速度)
            angularAcceleration = Vector3.Transform(torque, inertiaTensorInverse);

            // 更新角速度 ω = ω + α * t
            angularVelocity += angularAcceleration * time;

            // 计算角阻尼并应用
            angularVelocity *= 1 - angularf;

            // 更新旋转 θ = θ + ω * t
            Vector3 rotation = angularVelocity * time;

            if (updateMutex.WaitOne(0))
            {
                move(displacement);
                Matrix4x4 r = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                transform = r * transform;
                updateMutex.ReleaseMutex();
            }
        }



        public virtual void paint(BufferedGraphics graphics, Matrix4x4 cameraTransform, float[,] depthBuffer, int[,] indexBuffer, float near = 0.1f, float far = 1000)
        {
            mesh.paint(graphics, scale, transform, cameraTransform, depthBuffer, indexBuffer, near, far);
        }

        public void move(Vector3 vector3)
        {
            transform.M41 += vector3.X;
            transform.M42 += vector3.Y;
            transform.M43 += vector3.Z;
        }

        public void moveTo(Vector3 position)
        {
            transform.M41 = position.X;
            transform.M42 = position.Y;
            transform.M43 = position.Z;
        }

        public void rotateTo(Vector3 rotation)
        {
            transform = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        public void addForce(Force f)
        {
            forces.Add(f);
        }

        public void setVelocity(Vector3 velocity) { this.velocity = velocity; }
        public void setAngularVelocity(Vector3 angularVelocity) { this.angularVelocity = angularVelocity; }

        public void addLight(Light light)
        {
            mesh.addLight(light);
        }

        // 监测两个GameObject的mesh是否相交（碰撞）
        public bool isCollide(GameObject other)
        {
            // 获取当前物体的顶点
            List<Vector3> thisVertices = mesh.getVertices().Select(v => Vector3.Transform(v, transform)).ToList();

            // 获取另一个物体的顶点
            List<Vector3> otherVertices = other.mesh.getVertices().Select(v => Vector3.Transform(v, other.transform)).ToList();

            // 检查每对顶点是否相交
            foreach (Vector3 vertex1 in thisVertices)
            {
                foreach (Vector3 vertex2 in otherVertices)
                {
                    // 如果两个顶点之间的距离小于某个阈值，则认为它们相交
                    float distance = Vector3.Distance(vertex1, vertex2);
                    if (distance < 0.1f) // 这里的0.1f是一个示例阈值，你可以根据需要调整
                    {
                        return true; // 发现碰撞
                    }
                }
            }

            return false; // 没有碰撞
        }

        public Matrix4x4 getTransform() { return transform; }
        public Vector3 getPos() { return new Vector3(transform.M41, transform.M42, transform.M43); }
        public float PosX() { return transform.M41; }
        public float PosY() { return transform.M42; }
        public float PosZ() { return transform.M43; }
        public float SizeX() { return scale.X; }
        public float SizeY() { return scale.Y; }
        public float SizeZ() { return scale.Z; }
        public float CollisionBoxRadius()
        { 
            // 碰撞盒半径
            return (float)Math.Sqrt(scale.X * scale.X + scale.Y * scale.Y + scale.Z * scale.Z) / 2;
        }
        public Vector3 getRot() { return new Vector3(transform.M31, transform.M32, transform.M33); }
        public Vector3 getScale() { return scale; }
        public Vector3 getVelocity() { return velocity; }
        public Vector3 getAngularVelocity() { return angularVelocity; }
        public Vector3 getAcceleration() { return acceleration; }
        public Vector3 getAngularAcceleration() { return angularAcceleration; }

        // 键鼠事件
        public void keyDown(KeyEventArgs e) { }
        public void keyUp(KeyEventArgs e) { }
        public void mouseDown(MouseEventArgs e) { }
        public void mouseUp(MouseEventArgs e) { }
        public void mouseMove(MouseEventArgs e) { }
        public void mouseWheel(MouseEventArgs e) { }
        public void mouseDoubleClick(MouseEventArgs e) { }

    }
}
