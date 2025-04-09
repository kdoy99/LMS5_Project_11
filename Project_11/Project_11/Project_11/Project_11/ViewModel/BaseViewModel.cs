using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project_11.ViewModel
{
    // 메시지 출력용 클래스
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void ShowMessage(string message)
        {
            string title = "알림";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        protected void ShowErrorMessage(string message)
        {
            string title = "오류";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
