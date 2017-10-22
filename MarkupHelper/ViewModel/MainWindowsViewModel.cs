using MarkupHelper.Common.Service;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Enrichers;
using System.Windows.Input;
using System.Windows;
using Serilog.Core;
using System.Collections.ObjectModel;

namespace MarkupHelper.ViewModel
{
    public class MainWindowsViewModel : BindableBase
    {
        private Common.Domain.Model.UserModel _user;
        public ICommand ValidateTokenCommand { get; private set; }
        public ICommand GetUnAssignedGroupCommand { get; private set; }
        public ICommand SubmitTagsCommand { get; private set; }
        private MarkupRepositoryClient _markupRepositoryClient;
        private string _userToken;
        private ILogger _logger;
        private bool _isReady;
        private string _tag1;
        private string _tag2;
        private string _tag3;
        private string _tag4;
        private Uri _groupUrl;
        public ObservableCollection<string> Tags = new ObservableCollection<string>();


        public MainWindowsViewModel()
        {
            ValidateTokenCommand = new DelegateCommand(ValidateToken);
            GetUnAssignedGroupCommand = new DelegateCommand(GetUnAssignedGroup);
            SubmitTagsCommand = new DelegateCommand(SubmitTags, () => !string.IsNullOrWhiteSpace(Tag1) & !string.IsNullOrWhiteSpace(Tag2) & !string.IsNullOrWhiteSpace(Tag3) & !string.IsNullOrWhiteSpace(Tag4));
            _logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .MinimumLevel.Verbose()
                .WriteTo.Seq(config.Default.SeqServer, compact: true)
                .CreateLogger();
            _markupRepositoryClient = new MarkupRepositoryClient(config.Default.ServiceEndpoint, _logger);
            IsReady = false;
        }

        private void SubmitTags()
        {
            throw new NotImplementedException();
        }

        private void GetUnAssignedGroup()
        {
            try
            {
                GroupUrl = new Uri("about:blank");
                var unmarked = _markupRepositoryClient.GetUnmarkedGroup(_user);
                GroupUrl = new Uri($"https://m.vk.com/club{unmarked.VkId}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                MessageBox.Show("Не удалось отобразить группу.");
            }
        }

        private void ValidateToken()
        {
            try
            {
                var user = _markupRepositoryClient.GetUser(_userToken);
                _user = user;
                var tags = _markupRepositoryClient.GetTagsList(_user);
                var rnd = new Random(Environment.TickCount);
                foreach (var tag in tags.OrderBy(z => rnd.NextDouble()))
                    Tags.Add(tag);
                GroupUrl = new Uri("https://m.vk.com/");
                IsReady = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                MessageBox.Show("Не удалось проверить токен пользователя.");
            }
        }

        public bool IsReady { get => _isReady; set { SetProperty(ref _isReady, value); } }
        public string UserToken { get => _userToken; set { SetProperty(ref _userToken, value); } }
        public string Tag1 { get => _tag1; set => SetProperty(ref _tag1, value); }
        public string Tag2 { get => _tag2; set => SetProperty(ref _tag2, value); }
        public string Tag3 { get => _tag3; set => SetProperty(ref _tag3, value); }
        public string Tag4 { get => _tag4; set => SetProperty(ref _tag4, value); }
        
        public Uri GroupUrl { get => _groupUrl; set => SetProperty(ref _groupUrl, value); }
    }
}
