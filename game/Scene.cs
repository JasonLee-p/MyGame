using MyGame.physics;
using MyGame.physics.GameObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace MyGame.game
{
    internal class Scene
    {
        private List<GameObject> gameObjects = new List<GameObject>();  // 游戏对象
        private Camera2D camera2d;
        private Camera3D camera3d;
        private int frameRate = 30;  // 帧率
        private bool is3D = false;  // 是否为3D场景
        // 锁
        private Mutex gameObjectMutex = new Mutex();

        public Scene(bool is3D = false, int frameRate = 30)
        {
            this.is3D = is3D;
            this.frameRate = frameRate;
            this.camera2d = new Camera2D(
                new Vector3(0, 0, 5),  // position
                0.0f,  // Zrotation
                1.0f,  // scale
                800,  // width
                600,  // height
                0.1f,  // near
                1000.0f  // far
            );
            this.camera3d = new Camera3D(
                new Vector3(0, 0, 7.5f),  // position
                new Vector3(0, 0, 0),  // target
                45.0f * (float)Math.PI / 180,  // fov
                1.0f,  // aspect
                0.1f,  // near
                1000.0f  // far
            );
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
        }

        public List<GameObject> GetGameObjects()
        {
            return gameObjects;
        }

        public void ClearGameObjects()
        {
            gameObjects.Clear();
        }

        public void Update(float time)
        {
            // 复制一份gameObjects，防止在遍历过程中被修改
            List<GameObject> _gameObjects = new List<GameObject>(this.gameObjects);
            foreach (GameObject gameObject in _gameObjects)
            {
                if (is3D) gameObject.update(time);
                else gameObject.update(time);
            }
        }

        internal void Render(BufferedGraphics buffer, float[,] depthBuffer, int[,] indexBuffer)
        {
            // 创建本地的gameObjects副本，避免在遍历过程中被修改
            List<GameObject> _gameObjects;
            lock (gameObjects)
            {
                _gameObjects = new List<GameObject>(gameObjects);
            }

            // 使用并行处理来加速渲染
            try
            {
                Parallel.ForEach(_gameObjects, gameObject =>
                {
                    lock (buffer)
                    {
                        if (is3D) { gameObject.paint(buffer, camera3d.getAllTransform(), depthBuffer, indexBuffer, camera3d.getNear(), camera3d.getFar()); }
                        else { gameObject.paint(buffer, camera2d.getAllTransform(), depthBuffer, indexBuffer, camera2d.getNear(), camera2d.getFar()); }
                    }
                });
            }
            catch (Exception e)
            {
                MyLogger.Log("渲染错误: " + e.Message);
            }
        }


        public void rotate(int dx, int dy)
        {
            // if (is3D) camera3d.rotate(dx, dy);
            // else camera2d.rotate(dx, dy);
        }

        public void Resize(int width, int height)
        {
            if (is3D) camera3d.resize(width, height);
            else camera2d.resize(width, height);
        }

        internal void zoom(int delta)
        {
            if (is3D) camera3d.zoom(delta);
            else camera2d.zoom(delta);
        }

        public void translate(Vector3 delta)
        {
            if (is3D) camera3d.move(delta);
            else camera2d.move(delta);
        }

        internal Camera getCamera()
        {
            if (is3D) return camera3d;
            else return camera2d;
        }
    }
}
