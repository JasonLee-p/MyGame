using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MyGame.physics.GameObjects;

namespace MyGame.physics
{
    public struct BoundingBox
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
    }


    public class Octree
    {
        private const int MaxObjects = 8;  // 每个节点最大对象数
        private const int MaxLevels = 5;  // 最大层数

        private int level;
        private List<GameObject> objects;
        private BoundingBox bounds;
        private Octree[] nodes;

        public Octree(int level, BoundingBox bounds)
        {
            this.level = level;
            this.objects = new List<GameObject>();
            this.bounds = bounds;
            this.nodes = new Octree[8];
        }

        public void Clear()
        {
            // 清理所有对象
            objects.Clear();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    nodes[i].Clear();
                    nodes[i] = null;
                }
            }
        }

        private void Split()
        {
            // 计算子节点的大小
            float subWidth = (bounds.Max.X - bounds.Min.X) / 2;
            float subHeight = (bounds.Max.Y - bounds.Min.Y) / 2;
            float subDepth = (bounds.Max.Z - bounds.Min.Z) / 2;

            float x = bounds.Min.X;
            float y = bounds.Min.Y;
            float z = bounds.Min.Z;

            nodes[0] = new Octree(level + 1, new BoundingBox(new Vector3(x, y, z), new Vector3(x + subWidth, y + subHeight, z + subDepth)));
            nodes[1] = new Octree(level + 1, new BoundingBox(new Vector3(x + subWidth, y, z), new Vector3(x + subWidth * 2, y + subHeight, z + subDepth)));
            nodes[2] = new Octree(level + 1, new BoundingBox(new Vector3(x, y + subHeight, z), new Vector3(x + subWidth, y + subHeight * 2, z + subDepth)));
            nodes[3] = new Octree(level + 1, new BoundingBox(new Vector3(x + subWidth, y + subHeight, z), new Vector3(x + subWidth * 2, y + subHeight * 2, z + subDepth)));
            nodes[4] = new Octree(level + 1, new BoundingBox(new Vector3(x, y, z + subDepth), new Vector3(x + subWidth, y + subHeight, z + subDepth * 2)));
            nodes[5] = new Octree(level + 1, new BoundingBox(new Vector3(x + subWidth, y, z + subDepth), new Vector3(x + subWidth * 2, y + subHeight, z + subDepth * 2)));
            nodes[6] = new Octree(level + 1, new BoundingBox(new Vector3(x, y + subHeight, z + subDepth), new Vector3(x + subWidth, y + subHeight * 2, z + subDepth * 2)));
            nodes[7] = new Octree(level + 1, new BoundingBox(new Vector3(x + subWidth, y + subHeight, z + subDepth), new Vector3(x + subWidth * 2, y + subHeight * 2, z + subDepth * 2)));
        }

        private int GetIndex(GameObject gameObject)
        {
            // 获取对象所在的子节点
            int index = -1;
            float verticalMidpoint = bounds.Min.X + (bounds.Max.X - bounds.Min.X) / 2;
            float horizontalMidpoint = bounds.Min.Y + (bounds.Max.Y - bounds.Min.Y) / 2;
            float depthMidpoint = bounds.Min.Z + (bounds.Max.Z - bounds.Min.Z) / 2;

            bool topQuadrant = (gameObject.PosY() < horizontalMidpoint && gameObject.PosY() + gameObject.SizeY() < horizontalMidpoint);
            bool bottomQuadrant = (gameObject.PosY() > horizontalMidpoint);
            bool frontQuadrant = (gameObject.PosZ() < depthMidpoint && gameObject.PosZ() + gameObject.SizeZ() < depthMidpoint);
            bool backQuadrant = (gameObject.PosZ() > depthMidpoint);

            if (gameObject.PosX() < verticalMidpoint && gameObject.PosX() + gameObject.SizeX() < verticalMidpoint)
            {
                if (frontQuadrant)
                {
                    if (topQuadrant) index = 0;
                    else if (bottomQuadrant) index = 2;
                }
                else if (backQuadrant)
                {
                    if (topQuadrant) index = 4;
                    else if (bottomQuadrant) index = 6;
                }
            }
            else if (gameObject.PosX() > verticalMidpoint)
            {
                if (frontQuadrant)
                {
                    if (topQuadrant) index = 1;
                    else if (bottomQuadrant) index = 3;
                }
                else if (backQuadrant)
                {
                    if (topQuadrant) index = 5;
                    else if (bottomQuadrant) index = 7;
                }
            }

            return index;
        }

        public void Insert(GameObject gameObject)
        {
            if (nodes[0] != null)
            {
                int index = GetIndex(gameObject);

                if (index != -1)
                {
                    nodes[index].Insert(gameObject);
                    return;
                }
            }

            objects.Add(gameObject);

            if (objects.Count > MaxObjects && level < MaxLevels)
            {
                if (nodes[0] == null)
                {
                    Split();
                }

                int i = 0;
                while (i < objects.Count)
                {
                    int index = GetIndex(objects[i]);
                    if (index != -1)
                    {
                        nodes[index].Insert(objects[i]);
                        objects.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        public List<GameObject> Retrieve(List<GameObject> returnObjects, GameObject gameObject)
        {
            // 
            int index = GetIndex(gameObject);
            if (index != -1 && nodes[0] != null)
            {
                nodes[index].Retrieve(returnObjects, gameObject);
            }

            returnObjects.AddRange(objects);

            return returnObjects;
        }
    }
}
