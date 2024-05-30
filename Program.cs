using MyGame.game;

namespace MyGame
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            Application.EnableVisualStyles();  // ʹ���Զ�����ʽ
            Application.SetCompatibleTextRenderingDefault(false);  // ʹ���Զ�������
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            MainWin mainWin = new MainWin();
            MyLogger.mainWin = mainWin;
            Application.Run(mainWin);
        }
    }
}