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
using MarkupHelper.Common.Domain.Model;

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
        private string _groupUrl;
        private int _userScore;
        public ObservableCollection<string> Tags { get; set; } = new ObservableCollection<string>();
        //public ObservableCollection<string> Emotions { get; set; } = new ObservableCollection<string>();
        private bool _isSubmitEnabled;
        private Content _currentGroup;
        private string _emo;

        public MainWindowsViewModel()
        {
            ValidateTokenCommand = new DelegateCommand(ValidateToken);
            GetUnAssignedGroupCommand = new DelegateCommand(GetUnAssignedGroup);
            SubmitTagsCommand = new DelegateCommand(SubmitTags);
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
            try
            {
                if (_user == null)
                    return;
                if (CurrentGroup == null)
                    return;

                if (string.IsNullOrWhiteSpace(Tag1) ||
                    string.IsNullOrWhiteSpace(Tag2))
                    return;

                _markupRepositoryClient.SubmitContentTag(_user, CurrentGroup, Tag1);
                _markupRepositoryClient.SubmitContentTag(_user, CurrentGroup, Tag2);
                _markupRepositoryClient.SubmitContentTag(_user, CurrentGroup, Emo);

                CurrentGroup = null;
                GroupUrl = "about:blank";
                Tag1 = null;
                Tag2 = null;
                Emo = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                MessageBox.Show("Не удалось отправить результаты");
            }
        }

        private void GetUnAssignedGroup()
        {
            try
            {
                GroupUrl = "about:blank";
                var unmarked = _markupRepositoryClient.GetUnmarkedContent(_user);
                GroupUrl = $"https://m.vk.com/club{unmarked.VkContentId}";
                CurrentGroup = unmarked;
                Tag1 = null;
                Tag2 = null;
                Emo = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                MessageBox.Show("Не удалось получить новую группу.");
            }
        }

        private void ValidateToken()
        {
            try
            {
                var user = _markupRepositoryClient.GetUser(_userToken);
                _user = user;
                if (_user == null)
                    return;
                var rnd = new Random(Environment.TickCount);
                UserScore = _markupRepositoryClient.CalculateUserScore(_user);
                var tags = _markupRepositoryClient.GetTagsList(_user);
                Tags.Clear();
                var arr = tags.Except(ContentTag.PredefinedEmotions)
                    .GroupBy(z => z[0])
                    .OrderBy(z => rnd.NextDouble())
                    .SelectMany(z => z.OrderBy(x => rnd.NextDouble()));

                foreach (var tag in arr)
                    Tags.Add(tag);

                //Emotions.Clear();
                //foreach (var tag in ContentTag.PredefinedEmotions)
                //    Emotions.Add(tag);
                
                GroupUrl = "https://m.vk.com/";
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

        public string Tag1
        {
            get => _tag1;
            set
            {
                SetProperty(ref _tag1, value);
                UpdateSubmitEnable();
            }
        }

        public string Tag2
        {
            get => _tag2;
            set
            {
                SetProperty(ref _tag2, value);
                UpdateSubmitEnable();
            }
        }

        public string Emo
        {
            get => _emo;
            set
            {
                SetProperty(ref _emo, value);
                UpdateSubmitEnable();
            }
        }

        private void UpdateSubmitEnable()
        {
            IsSubmitEnabled = CurrentGroup != null && !string.IsNullOrWhiteSpace(Tag1) && !string.IsNullOrWhiteSpace(Tag2) && !string.IsNullOrWhiteSpace(Emo)
                && Tag1 != Tag2 && Tag1 != Emo && Tag2 != Emo;
        }

        public string GroupUrl { get => _groupUrl; set => SetProperty(ref _groupUrl, value); }
        public bool IsSubmitEnabled { get => _isSubmitEnabled; set => SetProperty(ref _isSubmitEnabled, value); }
        public int UserScore { get => _userScore; set => SetProperty(ref _userScore, value); }
        public Content CurrentGroup { get => _currentGroup; set => SetProperty(ref _currentGroup, value); }
    }
}
