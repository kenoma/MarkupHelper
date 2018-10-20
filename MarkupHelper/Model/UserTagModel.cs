using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.ViewModel;

namespace MarkupHelper.Model
{
    public class UserTagModel : BindableBase
    {
        private string _category;
        private string _currentTag;
        private string[] _tags;
        private bool _allowEdit;

        public string Category
        {
            get { return _category; }
            set { SetProperty(ref _category, value); }
        }

        public string CurrentTag
        {
            get { return _currentTag; }
            set { SetProperty(ref _currentTag, value); }
        }

        public string[] Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }

        public bool AllowEdit
        {
            get { return _allowEdit; }
            set { SetProperty(ref _allowEdit, value); }
        }
    }
}
