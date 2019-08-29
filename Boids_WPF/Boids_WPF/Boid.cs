using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Boids_WPF
{
    public class Boid
    {
        Ellipse ellipse { get; set; }
        Vector position { get; set; }
        Vector velocity { get; set; }
        Vector acceleration { get; set; }
        double maxForce = 0.1;
        double maxSpeed = 2;

        double boidWidth = 8;
        double boidHeight = 8;

        bool outOfBound = false;

        public Boid(Vector _position, Vector _velocity, Vector _acceleration)
        {
            position = _position;
            velocity = _velocity;
            velocity.Normalize();
            velocity *= maxSpeed;
            acceleration = _acceleration;
            Create();
        }

        public void Create()
        {
            ellipse = new Ellipse();
            ellipse.Width = boidWidth;
            ellipse.Height = boidHeight;
            MainWindow.mainCanvas.Children.Add(ellipse);
            ellipse.Margin = new Thickness(position.X - boidWidth, position.Y - boidHeight, 0, 0);
            ellipse.Fill = new SolidColorBrush(Colors.Blue);
        }

        Boid[] GetClosestBoids(Boid[] _boids)
        {
            double[] perceptionRanges = new double[3];
            perceptionRanges[0] = Int32.Parse(MainWindow.alignmentRange.Text);
            perceptionRanges[1] = Int32.Parse(MainWindow.cohesionRange.Text);
            perceptionRanges[2] = Int32.Parse(MainWindow.separationRange.Text);
            double longestRange = Math.Max(perceptionRanges[0], perceptionRanges[1]);
            longestRange = Math.Max(longestRange, perceptionRanges[2]);
            List<Boid> closestBirds = new List<Boid>();
            for (int i = 0; i < _boids.Length; i++)
            {
                if (Math.Sqrt(Math.Pow(_boids[i].position.X - position.X, 2) + Math.Pow(_boids[i].position.Y - position.Y, 2)) < longestRange && _boids[i] != this) closestBirds.Add(_boids[i]);
            }
            return closestBirds.ToArray();
        }

        Vector Align(Boid[] _boids)
        {
            Vector steeringForce = new Vector(0, 0);
            double range = Int32.Parse(MainWindow.alignmentRange.Text);
            int count = 0;
            foreach(Boid boid in _boids)
            {
                if (Math.Sqrt(Math.Pow(boid.position.X - position.X, 2) + Math.Pow(boid.position.Y - position.Y, 2)) < range)
                {
                    steeringForce += boid.velocity;
                    count++;
                }
            }
            if (count > 0)
            {
                steeringForce /= _boids.Length;
                steeringForce.Normalize();
                steeringForce *= maxSpeed;
                steeringForce -= velocity;
               
                if (steeringForce.Length > maxForce)
                {
                    double diff = steeringForce.Length / maxForce;
                    double xLength = steeringForce.X / diff;
                    double yLength = steeringForce.Y / diff;
                    steeringForce = new Vector(xLength, yLength);
                } 



                //steeringForce = new Vector(maxSpeed / steeringForce.X * Math.Abs(steeringForce.X), maxSpeed / steeringForce.Y * Math.Abs(steeringForce.Y));
            }
            return steeringForce;
        }

        Vector Cohesion(Boid[] _boids)
        {
            Vector steeringForce = new Vector(0, 0);
            double range = Int32.Parse(MainWindow.cohesionRange.Text);
            int count = 0;
            foreach (Boid boid in _boids)
            {
                if (Math.Sqrt(Math.Pow(boid.position.X - position.X, 2) + Math.Pow(boid.position.Y - position.Y, 2)) < range)
                {
                    steeringForce += boid.position;
                    count++;
                }
            }
            if (count > 0)
            {
                steeringForce /= _boids.Length;
                steeringForce -= position;
                steeringForce.Normalize();
                steeringForce *= maxSpeed;
                steeringForce -= velocity;
                if (steeringForce.Length > maxForce)
                {
                    double diff = steeringForce.Length / maxForce;
                    double xLength = steeringForce.X / diff;
                    double yLength = steeringForce.Y / diff;
                    steeringForce = new Vector(xLength, yLength);
                }


                //steeringForce = new Vector(maxSpeed / steeringForce.X * Math.Abs(steeringForce.X), maxSpeed / steeringForce.Y * Math.Abs(steeringForce.Y));
            }
            return steeringForce;
        }

        Vector Separation(Boid[] _boids)
        {
            Vector steeringForce = new Vector(0, 0);
            double range = Int32.Parse(MainWindow.separationRange.Text);
            int count = 0;
            foreach (Boid boid in _boids)
            {
                if (Math.Sqrt(Math.Pow(boid.position.X - position.X, 2) + Math.Pow(boid.position.Y - position.Y, 2)) < range)
                {
                    Vector diff = position - boid.position;
                    diff /= diff * diff;
                    steeringForce += diff;
                    count++;
                }
            }
            if (count > 0)
            {
                steeringForce /= count;
                steeringForce.Normalize();
                steeringForce *= maxSpeed;
                steeringForce -= velocity;
                if (steeringForce.Length > maxForce)
                {
                    double diff = steeringForce.Length / maxForce;
                    double xLength = steeringForce.X / diff;
                    double yLength = steeringForce.Y / diff;
                    steeringForce = new Vector(xLength, yLength);
                }


                //steeringForce = new Vector(maxSpeed / steeringForce.X * Math.Abs(steeringForce.X), maxSpeed / steeringForce.Y * Math.Abs(steeringForce.Y));
            }
            return steeringForce;
        }

        bool CheckIfOutside(Vector _point)
        {
            if (_point.Y > MainWindow.mainCanvas.Height - boidHeight) return true;
            else if (_point.Y < boidHeight) return true;
            if (_point.X > MainWindow.mainCanvas.Width - boidWidth) return true;
            else if (_point.X < boidWidth) return true;
            return false;
        }

        Vector WallAvoidance()
        {
            Vector steeringForce = new Vector(0, 0);
            int count = 0;

            int numPoints = 50;
            double turnFraction = 1.6180339;
            for (int i = 0; i < numPoints; i++)
            {
                double dst = Math.Pow(i / ((numPoints - 1) * 0.005), 0.5);
                double angle = 2 * Math.PI * turnFraction * i;

                double x = dst * Math.Cos(angle) + position.X;
                double y = dst * Math.Sin(angle) + position.Y;

                Vector point = new Vector(x, y);
                if (CheckIfOutside(point))
                {
                    Vector diff = position - point;
                    steeringForce += diff;
                    count++;
                }
            }

            if (count > 0)
            {
                steeringForce /= count;
                steeringForce -= velocity;
                if (steeringForce.Length > maxForce)
                {
                    double diff = steeringForce.Length / maxForce;
                    double xLength = steeringForce.X / diff;
                    double yLength = steeringForce.Y / diff;
                    steeringForce = new Vector(xLength, yLength);
                }


                //steeringForce = new Vector(maxSpeed / steeringForce.X * Math.Abs(steeringForce.X), maxSpeed / steeringForce.Y * Math.Abs(steeringForce.Y));
            }

            return steeringForce;
        }

        Vector TendTowards()
        {
            return (new Vector(MainWindow.mainCanvas.Width / 2, MainWindow.mainCanvas.Height / 2) - position) / 10000;
        }

        public void Flock(Boid[] _boids)
        {
            Boid[] closestBoids = GetClosestBoids(_boids.ToArray());
            
            if (!outOfBound) acceleration += Align(closestBoids) * MainWindow.alignSlider.Value;
            if (!outOfBound) acceleration += Cohesion(closestBoids) * MainWindow.cohesionSlider.Value;
            if (!outOfBound) acceleration += Separation(closestBoids) * MainWindow.separationSlider.Value;
            if (!outOfBound) acceleration += TendTowards();
            acceleration += WallAvoidance() * 10;
        }

        void Show()
        {
            bool outOfBoundsLeft = (position.X < boidWidth * 2);
            bool outOfBoundsUp = (position.Y < boidHeight * 2);
            bool outOfBoundsRight = (position.X > MainWindow.mainCanvas.Width - boidWidth * 2);
            bool outOfBoundsDown = (position.Y > MainWindow.mainCanvas.Height - boidHeight * 2);

            if (outOfBoundsDown || outOfBoundsLeft || outOfBoundsRight || outOfBoundsUp)
            {
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                outOfBound = true;
            } else
            {
                ellipse.Margin = new Thickness(position.X - boidWidth / 2, position.Y - boidHeight / 2, 0, 0);
                ellipse.Fill = new SolidColorBrush(Colors.Blue);
                outOfBound = false;
            }
        }

        Random ran = new Random();
        public void Update()
        {
            position += velocity;
            velocity += acceleration;
            acceleration *= 0;
            velocity.Normalize();
            //if (position.Y > MainWindow.mainCanvas.Height) position = new Vector(450, 450);
            //else if (position.Y < boidHeight) position = new Vector(450, 450);
            //if (position.X > MainWindow.mainCanvas.Width - boidHeight) position = new Vector(450, 450);
            //else if (position.X < boidWidth) position = new Vector(450, 450);
            Show();
        }
    }
}
