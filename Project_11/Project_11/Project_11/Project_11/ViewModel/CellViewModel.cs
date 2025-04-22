using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Project_11.ViewModel.Commands;

namespace Project_11.ViewModel
{
    public class CellViewModel : BaseViewModel
    {
        public int Row { get; }
        public int Column { get; }

        private string _stone;
        public string Stone
        {
            get => _stone;
            set
            {
                _stone = value;
                OnPropertyChanged(nameof(Stone));
            }
        }

        public ICommand StoneCommand { get; }

        private Action<CellViewModel> _placeStoneCallback;

        public CellViewModel(int row, int column, Action<CellViewModel> placeStoneCallback)
        {
            Row = row;
            Column = column;
            _placeStoneCallback = placeStoneCallback;
            StoneCommand = new RelayCommand(PlaceStone);
            Stone = "";
        }

        private void PlaceStone()
        {
            _placeStoneCallback?.Invoke(this);
        }
    }
}
