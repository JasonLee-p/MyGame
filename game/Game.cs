using MyGame.MusicSynth;
using MyGame.physics;
using MyGame.physics.GameObjects;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.game
{
    internal class Game
    {
        static Game game;
        // 状态
        public bool isRunning = false;
        private int FPS = 60;  // 刷新率
        // 场景
        private List<Scene> scenes = new List<Scene>();
        public Scene currentScene = new Scene();
        // 光源
        public Light mainLight = new PointLight(new Vector3(0, 0, 0), new Vector3(20, 20, 20), 50.0f);
        // 线程
        private Thread gameThread;
        private Thread audioThread;
        // 合成器
        private Synth synth;

        public Game()
        {
            Game.game = this;
            // 初始化场景
            currentScene = new Scene(true, FPS);
            scenes.Add(currentScene);
            InitializeScenes();
            // 统一添加光源
            addLight2GameObjects(mainLight);
            isRunning = true;
            // 启动游戏线程
            gameThread = new Thread(new ThreadStart(GameLoop));
            // 启动音频线程
            InitializeAudioStream();
        }

        public void InitializeScenes()
        {
            SphereParticle sphereParticle = new SphereParticle(true, true, new Vector3(0, 0, 0), 1);

            currentScene.AddGameObject(sphereParticle);

            sphereParticle.setColor(Color.DarkGray);
        }

        public void InitializeAudioStream()
        {
            // 初始化合成器
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            synth = new Synth(waveFormat, 0.7f);
            // 启动音频线程
            audioThread = new Thread(new ThreadStart(AudioLoop));
            audioThread.Start();
        }

        public void GameLoop()
        {
            while (isRunning)
            {
                //
            }
        }

        public void AudioLoop()
        {
            // 播放合成器音频
        }

        public void PlayNote(int midiNoteNumber, int time)
        {
            synth.NoteOn(midiNoteNumber, time);
        }

            public void addLight2GameObjects(Light light)
        {
            foreach (Scene scene in scenes)
            {
                foreach (GameObject gameObject in scene.GetGameObjects())
                {
                    gameObject.addLight(light);
                }
            }
        }

        public void Update(float time)
        {
            if (!isRunning) { return; }
            foreach (Scene scene in scenes)
            {
                scene.Update(time);
            }
        }

        internal void Render(BufferedGraphics buffer, float[,] depthBuffer, int[,] indexBuffer)
        {
            currentScene.Render(buffer, depthBuffer, indexBuffer);
        }

        public void Resize(int width, int height)
        {
            foreach (Scene scene in scenes)
            {
                scene.Resize(width, height);
            }
        }

        public void keyDown(KeyEventArgs e)
        {
            foreach (GameObject gameObject in currentScene.GetGameObjects())
            {
                gameObject.keyDown(e);
            }
        }

        public void keyUp(KeyEventArgs e)
        {
            foreach (GameObject gameObject in currentScene.GetGameObjects())
            {
                gameObject.keyUp(e);
            }
        }

        public void mouseDown(MouseEventArgs e)
        {
            foreach (GameObject gameObject in currentScene.GetGameObjects())
            {
                gameObject.mouseDown(e);
            }
        }

        public void mouseUp(MouseEventArgs e)
        {
            foreach (GameObject gameObject in currentScene.GetGameObjects())
            {
                gameObject.mouseUp(e);
            }
        }

        public void mouseMove(MouseEventArgs e, Point lastMousePos)
        {
            if (e.Button == MouseButtons.Middle)
            {
                translate(new Vector3(e.X - lastMousePos.X, lastMousePos.Y - e.Y, 0));
            }
            foreach (GameObject gameObject in currentScene.GetGameObjects())
            {
                gameObject.mouseMove(e);
            }
        }

        public void mouseWheel(MouseEventArgs e)
        {
            currentScene.zoom(e.Delta);
            foreach (GameObject gameObject in currentScene.GetGameObjects())
            {
                gameObject.mouseWheel(e);
            }
        }

        public void rotate(int dx, int dy)
        {
            currentScene.rotate(dx, dy);
        }

        public void translate(Vector3 offset)
        {
            currentScene.translate(-offset / 1000);
        }

        // 关闭游戏
        public void close()
        {
            isRunning = false;
        }
    }
}
