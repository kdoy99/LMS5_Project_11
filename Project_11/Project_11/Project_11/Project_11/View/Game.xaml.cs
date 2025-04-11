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
using Project_11.Model;
using Project_11.ViewModel;
using System.Collections.Specialized;

namespace Project_11.View
{
    /// <summary>
    /// Server.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Game : Window
    {
        public Game(ViewModel_Game viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        public void DisplayMessage(string textbox_input)
        {
            MessageBox.Show(textbox_input);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel_Game vm)
            {
                vm.OnLoaded();
            }
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as ViewModel_Game;
                if (viewModel?.SendMessageCommand.CanExecute(null) == true)
                {
                    viewModel.SendMessageCommand.Execute(null);
                }

                ChatInput.Focus(); // 포커스 조정
                e.Handled = true; // 기존 엔터 동작 방지
            }
        }

        private void ChatBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (ChatBox.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += (s, args) =>
                {
                    if (ChatBox.Items.Count > 0)
                    {
                        ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
                    }
                };
            }
        }

        private void GameRoomList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as ViewModel_Game;
            if (viewModel != null && GameRoomList.SelectedItem is Data selectedRoom)
            {
                viewModel.JoinRoom(selectedRoom);
            }
        }
    }
}
