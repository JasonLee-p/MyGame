using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace MyGame.physics
{
    public class Force
    {
        public static Dictionary<int, Force> idMap = new Dictionary<int, Force>();

        public string name = "未命名力";
        private int id = 0;
        private Vector3 f = new Vector3(0, 0, 0);
        private Vector3 pos = new Vector3(0, 0, 0);  // 力的局部坐标作用点

        public Force(string name, Vector3 f)
        {
            this.name = name;
            this.f = f;
            arrangeID();
        }

        // 构造函数
        public Force(string name) { this.name = name; arrangeID(); }
        public Force(Vector3 f) { this.f = f; arrangeID(); }
        public Force(Vector3 f, Vector3 pos) { this.f = f; this.pos = pos; arrangeID(); }
        public Force() { arrangeID(); }

        // 方法
        public void arrangeID()
        {
            Random random = new Random();
            int id = random.Next(1, int.MaxValue);
            while (idMap.ContainsKey(id))
            {
                id = random.Next(1, int.MaxValue);
            }
            this.id = id;
            idMap.Add(id, this);
        }
        public int getId() { return id; }
        public Vector3 get() { return f; }
        public void set(Vector3 f) { this.f = f; }
        public void add(Vector3 f) { this.f += f; }
        public void setPos(Vector3 pos) { this.pos = pos; }
        public Vector3 getPos() { return pos; }
        public Vector3 getTorque() { return Vector3.Cross(pos, f); }
        public void clear() { f = new Vector3(0, 0, 0); }
    }

    internal class Gravity : Force
    {
        public readonly static Vector3 G = new Vector3(0, -9.8f, 0);
        public float mass = 1.0f;
        public Gravity(float mass) : base("重力", G * mass) { this.mass = mass; }
        public void setMass(float mass) { this.mass = mass; this.set(G * mass); }
        new public Vector3 get() { return G * mass; }
    }
}
