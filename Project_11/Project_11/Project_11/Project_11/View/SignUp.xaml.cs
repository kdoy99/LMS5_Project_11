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
using Project_11.ViewModel;

namespace Project_11.View
{
    /// <summary>
    /// SignUp.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }

        private void pwBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel_SignUp viewmodel)
            {
                viewmodel.account.Password = pwBox_New.Password;
            }
        }
    }
}
