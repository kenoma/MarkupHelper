using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MarkupHelper.Common.Domain.Model;
using MarkupHelper.Common.Domain.Repository;
using MarkupHelper.Model;
using MarkupHelper.Service;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Serilog;

namespace MarkupHelper.ViewModel
{
    public class MainWindowsViewModel : BindableBase
    {
        private UserModel _user;
        public ICommand ValidateTokenCommand { get; private set; }
        public ICommand GetUnAssignedGroupCommand { get; private set; }
        public ICommand SubmitTagsCommand { get; private set; }
        public ICommand GoToVkCommand { get; private set; }
        public ICommand GoToFbCommand { get; private set; }

        private IMarkupRepository _markupRepositoryClient;
        private string _userToken;
        private ILogger _logger;
        private bool _isReady;
        private readonly string[] AllowedToEdit = new[] { "Объект", "Проблема 1", "Проблема 2" };
        private Uri _groupUrl;
        private int _userScore;
        private double _userProgress;

        public ObservableCollection<UserTagModel> UserTags { get; set; } = new ObservableCollection<UserTagModel>();

        private Content _currentGroup;
        private bool _isVkReady;
        private bool _isFbReady;

        public MainWindowsViewModel()
        {
            ValidateTokenCommand = new DelegateCommand(ValidateToken, () => IsVkReady && IsFbReady && !IsReady);
            GetUnAssignedGroupCommand = new DelegateCommand(GetUnmarkedPost, () => IsVkReady && IsFbReady);
            SubmitTagsCommand = new DelegateCommand(SubmitTags, () => IsVkReady && IsFbReady && UpdateSubmitEnable());
            GoToVkCommand = new DelegateCommand(GoToVk, () => !IsVkReady);
            GoToFbCommand = new DelegateCommand(GoToFb, () => !IsFbReady);

            _logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .MinimumLevel.Verbose()
                .WriteTo.Seq(config.Default.SeqServer, compact: true)
                .CreateLogger();
            _markupRepositoryClient = new DummyMarkupRepository();//new MarkupRepositoryClient(config.Default.ServiceEndpoint, _logger);//
            IsReady = false;
        }

        private void GoToFb()
        {
            GroupUrl = new Uri("https://m.facebook.com/");
            IsFbReady = true;
            (ValidateTokenCommand as DelegateCommand).RaiseCanExecuteChanged();
            (GetUnAssignedGroupCommand as DelegateCommand).RaiseCanExecuteChanged();
            (SubmitTagsCommand as DelegateCommand).RaiseCanExecuteChanged();
            (GoToVkCommand as DelegateCommand).RaiseCanExecuteChanged();
            (GoToFbCommand as DelegateCommand).RaiseCanExecuteChanged();
        }

        private void GoToVk()
        {
            GroupUrl = new Uri("https://m.vk.com");
            IsVkReady = true;
            (ValidateTokenCommand as DelegateCommand).RaiseCanExecuteChanged();
            (GetUnAssignedGroupCommand as DelegateCommand).RaiseCanExecuteChanged();
            (SubmitTagsCommand as DelegateCommand).RaiseCanExecuteChanged();
            (GoToVkCommand as DelegateCommand).RaiseCanExecuteChanged();
            (GoToFbCommand as DelegateCommand).RaiseCanExecuteChanged();
        }

        private void SubmitTags()
        {
            try
            {
                if (_user == null)
                {
                    return;
                }

                if (CurrentGroup == null)
                {
                    return;
                }

                if (UserTags.Any(z => string.IsNullOrWhiteSpace(z.CurrentTag)))
                {
                    return;
                }

                foreach (var tag in UserTags)
                {
                    _markupRepositoryClient.SubmitContentTag(_user, CurrentGroup, tag.Category, tag.CurrentTag);
                }

                CurrentGroup = null;
                GroupUrl = new Uri("about:blank");
                UserTags.Clear();
                (SubmitTagsCommand as DelegateCommand).RaiseCanExecuteChanged();
                UserScore = _markupRepositoryClient.CalculateUserScore(_user);
                UserProgress = _markupRepositoryClient.PercentageDone(_user);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                MessageBox.Show("Не удалось отправить результаты");
            }
        }

        private void GetUnmarkedPost()
        {
            try
            {
                GroupUrl = new Uri("about:blank");

                var tags = _markupRepositoryClient.GetTagsList(_user);

                UserTags.Clear();

                foreach (var gtag in tags.GroupBy(z => z.Category))
                {
                    var item = new UserTagModel {
                        Category = gtag.Key,
                        CurrentTag = "",
                        Tags = gtag.Select(z => z.Tag).ToArray(),
                        AllowEdit = AllowedToEdit.Contains( gtag.Key)
                    };
                    item.PropertyChanged += (a, b) => (SubmitTagsCommand as DelegateCommand).RaiseCanExecuteChanged();
                    UserTags.Add(item);
                }

                var unmarked = _markupRepositoryClient.GetUnmarkedContent(_user);
                GroupUrl = unmarked.PostAddress;
                CurrentGroup = unmarked;
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
                _user = _markupRepositoryClient.GetUser(_userToken);
                if (_user == null)
                {
                    return;
                }

                var rnd = new Random(Environment.TickCount);
                UserScore = _markupRepositoryClient.CalculateUserScore(_user);
                UserProgress = _markupRepositoryClient.PercentageDone(_user);

                GroupUrl = new Uri("http://m.facebook.com/");
                IsReady = true;
                (ValidateTokenCommand as DelegateCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                MessageBox.Show("Не удалось проверить токен пользователя.");
            }
        }

        public bool IsReady { get => _isReady; set => SetProperty(ref _isReady, value); }
        public bool IsVkReady { get => _isVkReady; set => SetProperty(ref _isVkReady, value); }
        public bool IsFbReady { get => _isFbReady; set => SetProperty(ref _isFbReady, value); }
        public string UserToken { get => _userToken; set => SetProperty(ref _userToken, value); }

        private bool UpdateSubmitEnable()
        {
            return CurrentGroup != null && !UserTags.Any(z => string.IsNullOrWhiteSpace(z.CurrentTag));
        }

        public Uri GroupUrl { get => _groupUrl; set => SetProperty(ref _groupUrl, value); }
        public int UserScore { get => _userScore; set => SetProperty(ref _userScore, value); }
        public double UserProgress { get => _userProgress; set => SetProperty(ref _userProgress, value); }

        public Content CurrentGroup { get => _currentGroup; set => SetProperty(ref _currentGroup, value); }
    }
}
