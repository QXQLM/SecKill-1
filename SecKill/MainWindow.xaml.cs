﻿using SecKill.Constants;
using SecKill.Model;
using SecKill.Service;
using SecKill.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using config = SecKill.Config.Config;

namespace SecKill
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Area> areas = Areas.GetAreas();
        private SettingCookieWindow settingWindow;
        private SwitchMemberWindow MemberWindow;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Province.ItemsSource = areas;
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string provinceName = Province.SelectedValue.ToString();
            List<Area> cities = areas.First(p => p.Value == provinceName)?.Children;
            City.ItemsSource = cities;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string provinceName = Province.SelectedValue.ToString();
            string cityName = City.SelectedValue.ToString();
            if (provinceName.Length == 0 || cityName.Length == 0)
            {
                MessageBox.Show("省份或城市不能为空");
            }
            else
            {
                config.RegionCode = cityName;
                Area city = (Area)City.SelectedItem;
                string message = $"已选择区域:{city.Name}";
                MessageBox.Show(message);
                LogModel.UpdateLogStr(message);
            }
        }

        private void SettingCookie_Click(object sender, RoutedEventArgs e)
        {
            if (settingWindow == null)
            {
                SettingCookieWindow settingCookieWindow = new SettingCookieWindow
                {
                    Title = "设置抢购参数"
                };
                settingWindow = settingCookieWindow;
            }
            settingWindow.Show();
            settingWindow.Focus();
        }

        private void SwitchMember_Click(object sender, RoutedEventArgs e)
        {

            if (MemberWindow == null)
            {
                SwitchMemberWindow switchMemberWindow = new SwitchMemberWindow
                {
                    Title = "选择成员"
                };
                MemberWindow = switchMemberWindow;
            }

            MemberWindow.Show();
        }

        private async void RefreshVaccineList_Click(object sender, RoutedEventArgs e)
        {
            List<VaccineList> vaccineLists = await HttpService.GetVaccineLists();
            DataGrid.DataContext = vaccineLists;
            if (vaccineLists.Count() == 0)
            {
                MessageBox.Show("该地区目前没有秒杀！");
            }
            else
            {
                MessageBox.Show("疫苗列表刷新成功！");
            }
        }

        private void StartKill_Click(object sender, RoutedEventArgs e)
        {
            if (config.Cookie.Count == 0)
            {
                throw new BusinessException("请配置cookie!!!");
            }
            if (DataGrid.SelectedItems.Count == 0)
            {
                throw new BusinessException("请选择要抢购的疫苗");
            }

            VaccineList selectedItem = DataGrid.SelectedItem as VaccineList;
            int id = selectedItem.Id;
            string startIime = selectedItem.StartTime;
            Task.Factory.StartNew(async () =>
            {
                await SecKillService.StartSecKill(id, startIime);
            });
            MessageBox.Show("设置抢购成功");
        }
    }
}
