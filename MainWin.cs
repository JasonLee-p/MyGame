using MyGame.game;
using MyGame.physics;
using MyGame.physics.GameObjects;
using System;
using System.Drawing.Printing;
using System.Numerics;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace MyGame
{
    public partial class MainWin : Form
    {
        public static MainWin Instance;
        public Color clearColor = Color.Black;
        private int FPS;
        private System.Windows.Forms.Timer mainTimer;
        private BufferedGraphicsContext context = null;
        private List<BufferedGraphics> graphicsBuffers = new List<BufferedGraphics>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Point lastMousePos = new Point(0, 0);
        private DateTime lastPaintTime = DateTime.Now;
        private object drawLock = new object();
        private int waitTime;
        public Mutex statuLabelMutex = new Mutex();
        private Game game;
        private float[,] depthBuffer;
        private int[,] indexBuffer;

        public MainWin()
        {
            FPS = userSetting.Default.FPS;
            Instance = this;
            InitializeComponent();
            InitializeBindings();
            InitializeBuffer();
            Text = Properties.Resources.GameName;
            game = new Game();
            waitTime = (int)(250 / FPS);
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Interval = 500 / FPS;
            mainTimer.Tick += MainTimer_Tick;
            mainTimer.Start();
            StartRenderingLoop();
        }

        private void InitializeBindings()
        {
            this.Resize += MainWin_Resize;
            this.FormClosing += MainWin_FormClosing;
            this.KeyDown += MainWin_KeyDown;
            this.KeyUp += MainWin_KeyUp;
            this.MouseDown += MainWin_MouseDown;
            this.MouseUp += MainWin_MouseUp;
            this.MouseMove += MainWin_MouseMove;
            this.MouseWheel += MainWin_MouseWheel;

            this.toolStripContainer1.ContentPanel.MouseMove += MainWin_MouseMove;
            this.toolStripContainer1.ContentPanel.MouseDown += MainWin_MouseDown;
            this.toolStripContainer1.ContentPanel.MouseUp += MainWin_MouseUp;
            this.toolStripContainer1.ContentPanel.MouseWheel += MainWin_MouseWheel;
        }

        private void InitializeBuffer()
        {
            context = BufferedGraphicsManager.Current;
            AllocateBuffers();
        }

        private void AllocateBuffers()
        {
            lock(drawLock)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    var buffer = context.Allocate(this.toolStripContainer1.ContentPanel.CreateGraphics(), this.toolStripContainer1.ContentPanel.DisplayRectangle);
                    buffer.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphicsBuffers.Add(buffer);
                }
                depthBuffer = new float[this.toolStripContainer1.ContentPanel.Width, this.toolStripContainer1.ContentPanel.Height];
                indexBuffer = new int[this.toolStripContainer1.ContentPanel.Width, this.toolStripContainer1.ContentPanel.Height];
            }
        }

        private bool ClearBuffer(BufferedGraphics buffer)
        {
            if (buffer.Graphics == null) return false;
            buffer.Graphics.Clear(clearColor);
            return true;
        }

        private void ResizeBuffer(int width, int height)
        {
            lock (drawLock)
            {
                foreach (var buffer in graphicsBuffers)
                {
                    buffer.Dispose();
                }
                graphicsBuffers.Clear();
                AllocateBuffers();
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            UpdateGame();
        }

        private void UpdateGame()
        {
            float paintTime = (float)(DateTime.Now - lastPaintTime).TotalSeconds;
            lastPaintTime = DateTime.Now;
            game.Update(paintTime);
        }

        private void StartRenderingLoop()
        {
            Task.Run(() =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Parallel.For(0, graphicsBuffers.Count, i =>
                    {
                        lock (drawLock)
                        {
                            var buffer = graphicsBuffers[i];
                            if (!ClearBuffer(buffer)) return;
                            game.Render(buffer, depthBuffer, indexBuffer);
                            buffer.Render();
                        }
                    });
                    Thread.Sleep(1000 / FPS);
                }
            }, cancellationTokenSource.Token);
        }

        public void updateStatus(string status)
        {
            if (statuLabelMutex.WaitOne(10))
            {
                if (statusStrip.InvokeRequired)
                {
                    Action<string> actionDelegate = (x) => { statusStrip.Items[0].Text = x; };
                    try
                    {
                        this.Invoke(actionDelegate, status);
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                }
                else
                {
                    statusStrip.Items[0].Text = status;
                }
                statuLabelMutex.ReleaseMutex();
            }
        }

        private void MainWin_Resize(object sender, EventArgs e)
        {
            game.Resize(this.toolStripContainer1.ContentPanel.Width, this.toolStripContainer1.ContentPanel.Height);
            ResizeBuffer(this.toolStripContainer1.ContentPanel.Width, this.toolStripContainer1.ContentPanel.Height);
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e) { game.keyDown(e); }
        private void MainWin_KeyUp(object sender, KeyEventArgs e) { game.keyUp(e); }
        private void MainWin_MouseDown(object sender, MouseEventArgs e) { game.mouseDown(e); }
        private void MainWin_MouseUp(object sender, MouseEventArgs e) { game.mouseUp(e); }
        private void MainWin_MouseMove(object sender, MouseEventArgs e)
        {
            game.mouseMove(e, lastMousePos);
            lastMousePos = e.Location;
        }
        private void MainWin_MouseWheel(object sender, MouseEventArgs e) { game.mouseWheel(e); }

        private void MainWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            game.isRunning = false;
            mainTimer.Stop();
            cancellationTokenSource.Cancel();
            // 暂停一会，让渲染线程退出
            Thread.Sleep(100);
            foreach (var buffer in graphicsBuffers)
            {
                buffer.Dispose();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 50; i++)
            {
                game.PlayNote(69, 2000);
                Random random = new Random();
                Cube cube = new Cube(PrimitiveType.LineList, false, new Vector3(0, 0, 0), new Vector3(0.4f, 0.4f, 0.4f));
                cube.mesh.setLineColor(Color.FromArgb(random.Next(128, 255), random.Next(128, 255), random.Next(128, 255)));
                cube.mesh.setLineWidth(2f);
                float vRange = 5.0f;
                cube.setVelocity(new Vector3((float)random.NextDouble() * vRange * 2 - vRange, (float)random.NextDouble() * vRange * 2 - vRange, (float)random.NextDouble() * vRange * 2 - vRange));
                float avRange = 10.0f;
                cube.setAngularVelocity(new Vector3((float)random.NextDouble() * avRange * 2 - avRange, (float)random.NextDouble() * avRange * 2 - avRange, (float)random.NextDouble() * avRange * 2 - avRange));
                game.currentScene.AddGameObject(cube);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Cube cube = new Cube(PrimitiveType.LineList, false, new Vector3(0, 0, 0), new Vector3(0.4f, 0.4f, 0.4f));
            cube.mesh.setLineColor(Color.White);
            cube.mesh.setLineWidth(2f);
            cube.setVelocity(new Vector3(0, 7.5f, 0));
            cube.setAngularVelocity(new Vector3(0.0f, 4.0f, 0.0f));
            game.currentScene.AddGameObject(cube);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            Surface surface = new Surface(PrimitiveType.LineStrip, false, new Vector3(0, 0, 0), new Vector3(2, 1, 2));
            surface.mesh.setLineColor(Color.White);
            surface.mesh.setLineWidth(2f);
            float avRange = 10.0f;
            surface.setAngularVelocity(new Vector3((float)random.NextDouble() * avRange * 2 - avRange, (float)random.NextDouble()
                * avRange * 2 - avRange, (float)random.NextDouble() * avRange * 2 - avRange));
            game.currentScene.AddGameObject(surface);
        }
    }
}
