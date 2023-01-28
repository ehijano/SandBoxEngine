using System;
using System.Collections.Generic;
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
using System.Drawing;
using System.Threading;
using Rectangle = System.Windows.Shapes.Rectangle;
using System.Runtime.ConstrainedExecution;

namespace SandBoxEngine
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
		private Thread gameThread;
		private Boolean gameRunning = false, gamePaused = false;

		private long elapsed, startTime, timePerFrameMillis = 10, frameTime = 0, pauseTime = 0, lastPauseTime = 0;

		public List<Bbox> vBbox;
		public List<Circle> vCircle;

		public double gravity = 0.8;
		private CollisionDetector collisionDetector;
		//private int j = 0;
		public MainWindow()
		{
			InitializeComponent();
			this.MinHeight = 600;
			this.MinWidth = 800;

			auxCanvas.Height = this.Height;
			auxCanvas.Width = this.Width;
			auxCanvas.Focusable = true;

			/*
			// Game time ticker
			gameTickTimer.Tick += GameTickTimer_Tick;
			gameTickTimer.Interval = TimeSpan.FromMilliseconds(10);  // 100 FPS = 1/10th a frame per milisecond = 1 frame per 10 milliseconds
			*/

			gameThread = new Thread(run_game);
			gameThread.IsBackground = true;

			startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

			//this.SizeChanged += MainWindow_SizeChanged;
		}

		private void loadObjects()
		{
			vBbox = new List<Bbox>();
			//vBbox.Add(new Bbox(300, 400, 250, 100, -10.0));
			//vBbox.Add(new Bbox(200, 280, 200, 200, 50.0));

			//vBbox.Add(new Bbox(200, 430, 250, 100, 0.0));
			//vBbox.Add(new Bbox(220, 280, 200, 200, 60.0));

			vBbox.Add(new Bbox(200, 400, 200, 100, 0.0));
			vBbox.Add(new Bbox(250, 450, 200, 100, 0.0));

			//vBbox.Add(new Bbox(200, 170, 200, 100, 10.0));
			//vBbox.Add(new Bbox(100, 260, 200, 100, 50.0));



			vCircle = new List<Circle>();
			vCircle.Add(new Circle(200, 200, 50));
			vCircle.Add(new Circle(150, 50, 50));
			vCircle.Add(new Circle(250, 50, 50));
			vCircle.Add(new Circle(400, 250, 50));
			vCircle.Add(new Circle(500, 400, 100));
		}

		private void run_game()
		{
			while (gameRunning)
			{
				if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - frameTime > timePerFrameMillis) && (!gamePaused))
				{
					// record time at which frame started
					frameTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

					// Save total game running time
					elapsed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime - pauseTime;

					// Physics
					this.Dispatcher.Invoke(() => {
						checkBboxCollisions();
					});

					// Update canvas
					this.Dispatcher.Invoke(() => { drawScene(); });
					//j += 1;
				}
			}
		}

		private void checkBboxCollisions()
		{
			int numberBboxes = vBbox.Count();

			for (int i = 0; i < numberBboxes; i++)
			{
				Bbox box1 = vBbox.ElementAt(i);
				int j = i + 1;
				while (j < numberBboxes)
				{
					Bbox box2 = vBbox.ElementAt(j);
					collisionDetector = new CollisionDetector(box1.ToPolygon(), box2.ToPolygon());
					if (collisionDetector.intersect())
					{
						MyVector contactPoint = collisionDetector.contactPoint();
						// TODO
					}
					j += 1;
				}
			}
		}

		private void drawPoint(MyVector p)
		{
			Ellipse el = new Ellipse();
			el.Fill = new SolidColorBrush(Colors.Blue);
			el.Height = 6;
			el.Width = 6;
			Canvas.SetTop(el, p.vy - 3);
			Canvas.SetLeft(el, p.vx - 3);
			auxCanvas.Children.Add(el);
		}

		private void checkCircleCollisions(Circle circle, int indexCircle)
		{
			// Other circles
			int indexOtherCircle = indexCircle + 1;
			while (indexOtherCircle < vCircle.Count())
			{
				Circle otherCircle = vCircle.ElementAt(indexOtherCircle);
				if (circle.collidesCircle(otherCircle))
				{
					List<double> relativeVelocity = new List<double>();
					relativeVelocity.Add(circle.SpeedX - otherCircle.SpeedX);
					relativeVelocity.Add(circle.SpeedY - otherCircle.SpeedY);
					List<double> normal = computeNormal(circle.Xc, circle.Yc, otherCircle.Xc, otherCircle.Yc);


					if (dotProduct(relativeVelocity, normal) < 0)// Only act if balls are not already sepparating
					{
						double restitution = Math.Min(circle.restitutionCoefficient, otherCircle.restitutionCoefficient);
						double Dpn = -(1 + restitution) * (dotProduct(relativeVelocity, normal)) / ((1 / circle.mass) + (1 / otherCircle.mass));
						circle.SpeedX += Dpn * (normal.ElementAt(0)) / circle.mass;
						circle.SpeedY += Dpn * (normal.ElementAt(1)) / circle.mass;
						otherCircle.SpeedX -= Dpn * (normal.ElementAt(0)) / otherCircle.mass;
						otherCircle.SpeedY -= Dpn * (normal.ElementAt(1)) / otherCircle.mass;
					}
				}
				indexOtherCircle += 1;
			}

			// Floor and gravity
			int floorY = (int)(768 - (768 / 10));
			if ((circle.collidesFloor(floorY)) && (circle.SpeedY > 0.0))
			{
				circle.SpeedY = -circle.SpeedY * circle.restitutionCoefficient; // invert speed
				circle.Yc = floorY - circle.R;

			}
			else if ((circle.Yc + circle.R > floorY - 10) && (Math.Abs(circle.SpeedY) < 1))
			{
				circle.Yc = floorY - circle.R;
				circle.SpeedY = 0;
			}
			else
			{
				circle.Yc += (int)circle.SpeedY;
				circle.SpeedY += gravity;
			}

			// Horizontal movement
			circle.Xc += (int)circle.SpeedX;

		}


		private void gameReset()
		{
			startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			vBbox = new List<Bbox>();
			vCircle = new List<Circle>();
		}

		/*
        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
			elapsed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime;

			drawEllipse(j);
			j += 1;
		}
		*/

		private void exitButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void auxCanvas_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.P)
			{
				if (gamePaused)
				{
					pauseTime += (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastPauseTime;
				}
				else
				{
					lastPauseTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

				}
				gamePaused = !gamePaused;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			gameRunning = false;

			this.Close();
		}

		private void playButton_Click(object sender, RoutedEventArgs e)
		{
			this.auxCanvas.Visibility = Visibility.Visible;
			this.buttonGrid.Visibility = Visibility.Hidden;
			auxCanvas.Focus();

			gameReset();

			startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			gameRunning = true;
			gameThread.Start();
			loadObjects();

			//gameTickTimer.IsEnabled = true;
		}

		public void drawScene()
		{
			// This scales the canvas to the size of the window
			auxCanvas.RenderTransform = new ScaleTransform(this.ActualWidth / 1024, this.ActualHeight / 768);
			// Clear canvas
			auxCanvas.Children.Clear();


			// Draw Floor
			drawFloor();
			// Draw Bboxes
			drawBboxes();
			// Draw Circles
			drawCircles();

			// Time in MM:SS
			TextBlock auxText = new TextBlock();
			auxText.Text = millisecond_to_MMSS(elapsed);
			Canvas.SetLeft(auxText, 0);
			Canvas.SetTop(auxText, 0);
			auxText.FontSize = 40;
			auxText.Foreground = new SolidColorBrush(Colors.Blue);
			auxCanvas.Children.Add(auxText);

			// Time ms
			TextBlock auxText2 = new TextBlock();
			auxText2.Text = elapsed.ToString();
			Canvas.SetLeft(auxText2, 0);
			Canvas.SetTop(auxText2, 100);
			auxText2.FontSize = 40;
			auxText2.Foreground = new SolidColorBrush(Colors.Green);
			auxCanvas.Children.Add(auxText2);

			/*
			// j*delay
			TextBlock auxText3 = new TextBlock();
			auxText3.Text = (10*j).ToString();
			Canvas.SetLeft(auxText3, 100);
			Canvas.SetTop(auxText3, 0);
			auxText3.FontSize = 40;
			auxText3.Foreground = new SolidColorBrush(Colors.Orange);
			auxCanvas.Children.Add(auxText3);
			*/
			/*

			List<double[]> points = collisionDetector.getPoints1();
            for (int k=0; k< points.Count(); k++)
            {
				double[] position = points.ElementAt(k);
				Ellipse el = new Ellipse();
				el.Width = 10;
				el.Height = 10;
				Canvas.SetLeft(el, position[0]-5);
				Canvas.SetTop(el, position[1]-5);
				el.Fill = new SolidColorBrush(Colors.Black);
				auxCanvas.Children.Add(el);
			}
			*/

		}

		private void drawFloor()
		{
			Rectangle rec = new Rectangle();
			rec.Fill = new SolidColorBrush(Colors.Black);
			rec.Width = 1024;
			rec.Height = 768 / 10;
			Canvas.SetTop(rec, 9 * 768 / 10);
			Canvas.SetLeft(rec, 0);
			auxCanvas.Children.Add(rec);
		}

		private void drawBbox(Bbox bbox)
		{
			Rectangle rec = new Rectangle();
			rec.Fill = new SolidColorBrush(Colors.Red);
			rec.Height = bbox.H;
			rec.Width = bbox.W;
			TransformGroup totalTransform = new TransformGroup();
			totalTransform.Children.Add(new TranslateTransform(-bbox.W / 2, -bbox.H / 2));
			totalTransform.Children.Add(new RotateTransform(bbox.Phi));
			totalTransform.Children.Add(new TranslateTransform(bbox.W / 2, bbox.H / 2));
			rec.RenderTransform = totalTransform;
			Canvas.SetTop(rec, bbox.Yc - (bbox.H) / 2);
			Canvas.SetLeft(rec, bbox.Xc - (bbox.W) / 2);
			auxCanvas.Children.Add(rec);

		}

		private void drawBboxes()
		{
			for (int i = 0; i < vBbox.Count(); i++)
			{
				drawBbox(vBbox.ElementAt(i));
			}
		}

		private void drawCircle(Circle circle)
		{
			Ellipse el = new Ellipse();
			el.Fill = new SolidColorBrush(Colors.Blue);
			el.Height = 2 * circle.R;
			el.Width = 2 * circle.R;
			Canvas.SetTop(el, circle.Yc - circle.R);
			Canvas.SetLeft(el, circle.Xc - circle.R);
			auxCanvas.Children.Add(el);
		}

		private void drawCircles()
		{
			for (int indexCircle = 0; indexCircle < vCircle.Count(); indexCircle++)
			{
				checkCircleCollisions(vCircle.ElementAt(indexCircle), indexCircle);
				drawCircle(vCircle.ElementAt(indexCircle));

				/*
				TextBlock auxText3 = new TextBlock();
				auxText3.Text = (Math.Abs((int)circle.SpeedY)).ToString();
				Canvas.SetLeft(auxText3, circle.Xc);
				Canvas.SetTop(auxText3, circle.Yc);
				auxText3.FontSize = 40;
				auxText3.Foreground = new SolidColorBrush(Colors.Orange);
				auxCanvas.Children.Add(auxText3);
				*/

			}
		}

		private double dotProduct(List<double> v1, List<double> v2)
		{
			return v1.ElementAt(0) * v2.ElementAt(0) + v1.ElementAt(1) * v2.ElementAt(1);
		}
		private List<double> computeNormal(int x1, int y1, int x2, int y2)
		{
			List<double> result = new List<double>();
			double d = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
			result.Add((x1 - x2) / d);
			result.Add((y1 - y2) / d);
			return result;
		}
		private String millisecond_to_MMSS(long t)
		{
			int total_seconds = (int)(t / 1000);
			int seconds = total_seconds % 60;
			int minutes = (int)((total_seconds - seconds) / 60);

			String solution = "";
			if (minutes < 10)
			{
				solution = "0" + minutes.ToString();
			}
			else
			{
				solution = minutes.ToString();
			}
			solution = solution + ":";
			if (seconds < 10)
			{
				solution = solution + "0" + seconds.ToString();
			}
			else
			{
				solution = solution + seconds.ToString();
			}

			return solution;
		}

	}
}

