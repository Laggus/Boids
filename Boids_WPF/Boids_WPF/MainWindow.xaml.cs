using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Boids_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Canvas mainCanvas;
        public static Slider alignSlider;
        public static Slider cohesionSlider;
        public static Slider separationSlider;
        public static TextBox alignmentRange;
        public static TextBox cohesionRange;
        public static TextBox separationRange;

        public static List<Boid> flock = new List<Boid>();
        DispatcherTimer dispatcherTimer;
        Stopwatch stopwatch = new Stopwatch();
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            mainCanvas = MainCanvas;
            alignSlider = AlignSlider;
            cohesionSlider = CohesionSlider;
            separationSlider = SeparationSlider;
            alignmentRange = AlignmentRange;
            cohesionRange = CohesionRange;
            separationRange = SeparationRange;


            for (int i = 0; i < 200; i++) flock.Add(new Boid(new Vector(random.NextDouble() * mainCanvas.Width, random.NextDouble() * mainCanvas.Height), new Vector(random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1), new Vector()));

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Render);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Tick += Tick;
            dispatcherTimer.Start();
            stopwatch.Start();
        }

        int fps = 60;
        long nextFrameTime;
        
        void Tick(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > nextFrameTime)
            {

                foreach (Boid boid in flock) boid.Flock(flock.ToArray());
                foreach (Boid boid in flock) boid.Update();
                nextFrameTime = stopwatch.ElapsedMilliseconds + (1000 / fps);
            }

            
            
        }
    }
}
