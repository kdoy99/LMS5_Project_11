using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_11.ViewModel
{
    public class BoardViewModel : BaseViewModel
    {
        public ObservableCollection<CellViewModel> Cells { get; set; } = new();

        public BoardViewModel()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Cells.Add(new CellViewModel(row, col, OnCellClicked));
                }
            }

            //초기 배치
            this[3, 3].Stone = "W";
            this[3, 4].Stone = "B";
            this[4, 3].Stone = "B";
            this[4, 4].Stone = "W";
        }

        public CellViewModel this[int row, int col] => Cells.First(c => c.Row == row && c.Column == col);

        private void OnCellClicked(CellViewModel cell)
        {
            if (string.IsNullOrWhiteSpace(cell.Stone))
            {
                cell.Stone = "B";
            }
        }
    }
}
