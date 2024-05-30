using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.game
{
    public class Light
    {
        public Vector3 position = new Vector3(0, 0, 0);  // 位置
        public Vector3 color = new Vector3(1, 1, 1);  // 颜色
        public float intensity = 1.0f;  // 强度
    }

    internal class PointLight : Light
    {
        public PointLight(Vector3 position, Vector3 color, float intensity)
        {
            this.position = position;
            this.color = color;
            this.intensity = intensity;
        }
    }

    internal class DirectionalLight : Light
    {
        public Vector3 direction = new Vector3(0, 0, 0);  // 方向

        public DirectionalLight(Vector3 direction, Vector3 color, float intensity)
        {
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
        }
    }

    internal class SpotLight : Light
    {
        public Vector3 direction = new Vector3(0, 0, 0);  // 方向
        public float cutoff = 0.0f;  // 截止角
        public float outerCutoff = 0.0f;  // 外截止角

        public SpotLight(Vector3 position, Vector3 direction, Vector3 color, float intensity, float cutoff, float outerCutoff)
        {
            this.position = position;
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
            this.cutoff = cutoff;
            this.outerCutoff = outerCutoff;
        }
    }
}
