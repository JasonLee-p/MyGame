using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.physics
{
    internal class Camera
    {
        // 虚函数
        public virtual void move(Vector3 offset) { }
        public virtual void move(float x, float y) { }
        public virtual void moveTo(Vector3 position) { }
        public virtual void rotate(Vector3 offset) { }
        public virtual void rotateTo(Vector3 rotation) { }
        public virtual void zoom(float offset) { }
        public virtual void resize(int width, int height) { }
    }

    internal class Camera3D : Camera
    {
        private Mutex fovMutex = new Mutex();
        private Vector3 position = new Vector3(0, 0, 0);
        private Vector3 rotation = new Vector3(0, 0, 0);
        private float fov = 45 * MathF.PI / 180;
        private float aspect = 1;
        private float near = 0.1f;
        private float far = 1000;
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projectionMatrix;

        public Camera3D() { }

        public Camera3D(Vector3 position, Vector3 rotation, float fov, float aspect, float near, float far)
        {
            this.position = position;
            this.rotation = rotation;
            this.fov = fov;
            this.aspect = aspect;
            this.near = near;
            this.far = far;
            this.viewMatrix = updateViewMatrix();
            this.projectionMatrix = updateProjectionMatrix();
        }

        public Matrix4x4 updateViewMatrix()
        {
            // 在xy平面平移
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(-position);
            // 旋转
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            // 乘积
            viewMatrix = rotationMatrix * translationMatrix;
            return viewMatrix;
        }

        public Matrix4x4 updateProjectionMatrix()
        {
            // 上锁
            if (fovMutex.WaitOne(0))
            {
                projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspect, near, far);
                fovMutex.ReleaseMutex();
            }
            return projectionMatrix;
        }

        public Matrix4x4 getAllTransform()
        {
            return viewMatrix * projectionMatrix;
        }

        public float getNear()
        {
            return near;
        }
        public float getFar()
        {
            return far;
        }

        public void move(Vector3 offset)
        {
            position += offset;
            updateViewMatrix();
        }

        public void moveTo(Vector3 position)
        {
            this.position = position;
            updateViewMatrix();
        }

        public void rotate(Vector3 offset)
        {
            rotation += offset;
            updateViewMatrix();
        }

        public void rotateTo(Vector3 rotation)
        {
            this.rotation = rotation;
            updateViewMatrix();
        }

        public void zoom(float offset)
        {
            if (fovMutex.WaitOne(0))
            {
                fov -= offset / 5000;
                if (fov > 3.1415926f) fov = 3.1415926f;
                if (fov < 0.1f) fov = 0.1f;
                updateProjectionMatrix();
            }
            fovMutex.ReleaseMutex();
        }

        public void resize(int width, int height)
        {
            aspect = (float)width / height;
            if (aspect >= 0)
            {
                this.projectionMatrix = updateProjectionMatrix();
            }
        }
    }

    class Camera2D : Camera
    {
        private Vector3 position;
        private float rotation = 0; // 顺时针旋转角度
        private float scale = 1;
        private int width = 800;
        private int height = 600;
        private float aspect = 1;  // 宽高比
        private float near = 0;
        private float far = 1000;
        public Matrix4x4 viewMatrix;  // 视图矩阵
        public Matrix4x4 orthographicMatrix;  // 正交投影矩阵
        public Matrix4x4 transform;  // 总变换矩阵
        public Camera2D() { position = new Vector3(0, 0, 30); }
        public Camera2D(Vector3 position, float rotation, float scale, int width, int height, float near, float far)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.near = near;
            this.far = far;
            this.viewMatrix = getViewMatrix();
            this.width = width;
            this.height = height;
            this.orthographicMatrix = getOthergraphicMatrix();
        }
        public Matrix4x4 getViewMatrix()
        {
            return Matrix4x4.CreateTranslation(-position) * Matrix4x4.CreateRotationZ(rotation);
        }


        public Matrix4x4 getOthergraphicMatrix()
        {
            return Matrix4x4.CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, near, far);
        }

        public Matrix4x4 getAllTransform()
        {
            return transform;
        }

        public float getNear()
        {
            return near;
        }
        public float getFar()
        {
            return far;
        }

        public void move(Vector3 offset)
        {
            this.position += offset;
            this.viewMatrix.M41 = -position.X;
            this.viewMatrix.M42 = -position.Y;
        }

        public void moveTo(Vector3 position)
        {
            this.position = position;
            this.viewMatrix.M41 = -position.X;
            this.viewMatrix.M42 = -position.Y;
        }

        public void rotate(float offset)
        {
            rotation += offset;
            this.viewMatrix = getViewMatrix();
        }

        public void rotateTo(float rotation)
        {
            this.rotation = rotation;
            this.viewMatrix = getViewMatrix();
        }

        public void zoom(float offset)
        {
            scale += offset;
            this.orthographicMatrix = getOthergraphicMatrix();
        }

        public void resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.orthographicMatrix = getOthergraphicMatrix();
        }
    }
}
