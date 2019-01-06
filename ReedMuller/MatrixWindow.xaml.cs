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
using System.Windows.Shapes;

namespace ReedMuller
{
    /// <summary>
    /// Interaction logic for MatrixWindow.xaml
    /// </summary>
    public partial class MatrixWindow : Window
    {
        public MatrixWindow(bool[][] matrix, int m, int r)
        {
            InitializeComponent();
            mValue.Text = "m = " + m;
            rValue.Text = "r = " + r;

            StringBuilder strBuilder = new StringBuilder();

            foreach (bool[] line in matrix)
            {
                foreach (bool n in line)
                {
                    strBuilder.Append(n ? 1 : 0);
                    strBuilder.Append("  ");
                }
                strBuilder.Append("\n");
            }

            matrixValue.Text = strBuilder.ToString();
        }
    }
}
