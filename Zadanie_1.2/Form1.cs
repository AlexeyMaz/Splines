using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace Zadanie_1
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        int n;
        public Form1()
        {
            InitializeComponent();
            Draw();
        }

        public double f(double x)
        {
            return Math.Sin(4 * x);//1 / (1 + 25 * x * x);
        }

        double S_k(double x, double x_i, double a, double b, double c, double d)
        {
            return a + b * (x - x_i) + c * Math.Pow((x - x_i), 2) + d * Math.Pow((x - x_i), 3);
        }

        public double[] Tridiagonal(double[] h, double[] l)
        {
            double[] c = new double[n + 1];
            double[] beta = new double[n + 1];
            double[] lbd = new double[n + 1];
            beta[1] = -h[2] / (2 * (h[1] + h[2]));
            lbd[1] = (3 * (l[2] - l[1])) / (2 * (h[1] + h[2]));

            //прямой ход
            for (int k = 3; k < n + 1; k++)
            {
                beta[k] = -h[k] / (2 * (h[k - 1] + h[k]) + h[k - 1] * beta[k - 2]);
                lbd[k] = (3 * (l[k] - l[k - 1]) - h[k - 1] * lbd[k - 2]) / (2 * (h[k - 1] + h[k]) + h[k - 1] * beta[k - 2]);
            }

            c[0] = 0; c[n] = 0;
            //обратный ход
            for (int k = n; k >= 2; k--)
                c[k - 1] = beta[k - 1] * c[k] + lbd[k - 1];

            return c;
        }

        public void Draw()
        {
            if (AllocConsole())
            {
                Console.Write("Введите количество узлов: ");
                n = Convert.ToInt32(Console.ReadLine());
                FreeConsole();
            }


            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Интерполирование сплайнами";
            myPane.XAxis.Title.Text = "Ось Х";
            myPane.YAxis.Title.Text = "Ось Y";
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MinorGrid.IsVisible = true;
            myPane.XAxis.MinorGrid.IsVisible = true;
            myPane.XAxis.Scale.Min = -1;
            myPane.XAxis.Scale.Max = 1;

            double[] x = new double[n + 1];//x[i]
            double step = 2.0D / n;
            //вычисление равноотстоящих узлов
            for (int i = 0; i < n + 1; i++)
                x[i] = -1 + i * step;

            //double[] x_p = { -1, -0.8, -0.5, -0.2, -0.1, 0, 0.1, 0.2, 0.5, 0.8, 1 };
            //вычисление чебышевских узлов
            double[] x_p = new double[n + 1];//x
            for (int i = 0; i < n + 1; i++)
                x_p[n - i] = Math.Cos((2 * i - 1) * Math.PI / (2 * n));

            double[] a = new double[n + 1];
            for (int i = 1; i < n + 1; i++)
                a[i] = f(x[i]);


            double[] h = new double[n + 1];
            double[] l = new double[n + 1];
            for (int k = 1; k < n + 1; k++)
            {
                h[k] = x[k] - x[k - 1];
                l[k] = (f(x[k]) - f(x[k - 1])) / h[k];
            }
            double[] c = new double[n + 1];
            c = Tridiagonal(h, l);

            double[] b = new double[n + 1];
            for (int k = 1; k < n + 1; k++)
                b[k] = l[k] + (2 * c[k] * h[k] + h[k] * c[k - 1]) / 3;

            double[] d = new double[n + 1];
            for (int k = 1; k < n; k++)
                d[k] = l[k] + (c[k] - c[k - 1]) / (3 * h[k]);


            PointPairList list1 = new PointPairList();
            PointPairList list2 = new PointPairList();

            foreach (double x_i in x)
                list1.Add(x_i, f(x_i));

            list2.Add(x[0], f(x[0]));
            for (int k = 1; k < n + 1; k++)
                list2.Add(x[k], S_k(x_p[k], x[k], a[k], b[k], c[k], d[k]));

            LineItem myCurve = myPane.AddCurve("f(x)", list1, Color.Blue, SymbolType.Circle);
            LineItem myCurve1 = myPane.AddCurve("S(x)", list2, Color.Yellow, SymbolType.Circle);


            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }
    }
}