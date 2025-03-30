using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_11_Example.Model;
using Project_11_Example.ViewModel.Command;
using Project_11_Example.ViewModel.Helpers;

namespace Project_11_Example.ViewModel
{
    public class WeatherVM : INotifyPropertyChanged
    {
        public WeatherVM()
        {
            // 디자인 모드에서만 볼 수 있게 해주는 모드
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                SelectedCity = new City
                {
                    LocalizedName = "서울"
                };

                CurrentConditions = new CurrentConditions
                {
                    WeatherText = "Partly cloudy",
                    Temperature = new Temperature
                    {
                        Metric = new Units
                        {
                            Value = 21
                        }
                    }
                };
            }

            SearchCommand = new SearchCommand(this);

            Cities = new ObservableCollection<City>();
        }

        public ObservableCollection<City> Cities { get; set; }


        public SearchCommand SearchCommand { get; set; }

        // [1] 시나리오: 도시 검색으로 요청한다. (query)
        private string query;

        public string Query
        {
            get { return query; }
            // [2] Property가 변경되면,
            set
            {
                query = value;
                // OnPropertyChanged 함수에서
                OnPropertyChanged(nameof(Query));
            }
        }

        // [1] 시나리오: City 정보가 업데이트 된다.
        private City selectedcity;

        public City SelectedCity
        {
            get { return selectedcity; }
            // [2] Property가 변경되면,
            set
            {
                selectedcity = value;
                // OnPropertyChanged 함수에서
                OnPropertyChanged(nameof(selectedcity));

                GetCurrentCondition();
            }
        }

        private async void GetCurrentCondition()
        {
            Query = string.Empty;
            Cities.Clear();
            CurrentConditions = await AccuWeatherHelper.GetCurrentConditions(SelectedCity.Key);
        }


        // [1] 시나리오: CurrentConditions 현재 날씨 정보가 업데이트 된다.
        private CurrentConditions currentCondition;

        public CurrentConditions CurrentConditions
        {
            get { return currentCondition; }
            // [2] Property가 변경되면,
            set
            {
                currentCondition = value;
                // OnPropertyChanged 함수에서
                OnPropertyChanged(nameof(CurrentConditions));
            }
        }

        // API
        public async Task MakeQueryAsync()
        {
            var cities = await AccuWeatherHelper.GetCities(Query);

            Cities.Clear();
            foreach (City city in cities)
            {
                if (city != null)
                    Cities.Add(city);
            }
        }

        // [4] 이벤트 핸들러가 동작하면,
        //XAML에 바인딩 되어있는 모든 UI컨트롤에 해당 query 값이 업데이트 된다.
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            // [3] PropertyChanged 이벤트를 실행한다.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
