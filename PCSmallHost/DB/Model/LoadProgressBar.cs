using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// 进度条实体类
    /// </summary>
    [Serializable]
    public partial class LoadProgressBar : INotifyPropertyChanged
    {
        public LoadProgressBar()
        { }
        private int _filesCount;
        public int FilesCount
        {
            get { return _filesCount; }
            set
            {
                _filesCount = value;
                OnPropertyChanged(nameof(FilesCount));
                OnPropertyChanged(nameof(ImportedFilesPercentage));
            }
        }

        private int _importedFilesCount;
        public int ImportedFilesCount
        {
            get { return _importedFilesCount; }
            set
            {
                _importedFilesCount = value;
                OnPropertyChanged(nameof(ImportedFilesCount));
                OnPropertyChanged(nameof(ImportedFilesPercentage));
            }
        }

        public double ImportedFilesPercentage
        {
            get
            {
                if (FilesCount == 0)
                    return 0;
                double percentage = (double)ImportedFilesCount / FilesCount * 100;
                return Math.Round(percentage, 2); // 保留两位小数
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
