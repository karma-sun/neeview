using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び順とかの保存される情報
    /// </summary>
    public class FolderParameter : BindableBase
    {
        private static int _randomSeed = new Random().Next();
        private FolderOrder _folderOrder;
        private bool _isFolderRecursive;


        public FolderParameter(string path)
        {
            Path = path;
            Load();
            RandomSeed = _randomSeed;
        }


        /// <summary>
        /// 場所
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// ソート順
        /// </summary>
        public FolderOrder FolderOrder
        {
            get { return _folderOrder; }
            set
            {
                _randomSeed = new Random().Next();
                if (_folderOrder != value || value == FolderOrder.Random)
                {
                    _folderOrder = value;
                    Save();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// この場所にあるフォルダーはサブフォルダーを読み込む
        /// </summary>
        public bool IsFolderRecursive
        {
            get { return _isFolderRecursive; }
            set
            {
                if (_isFolderRecursive != value)
                {
                    _isFolderRecursive = value;
                    Save();
                    RaisePropertyChanged();
                }

            }
        }

        private void Save()
        {
            BookHistoryCollection.Current.SetFolderMemento(Path, CreateMemento());
        }

        private void Load()
        {
            var memento = BookHistoryCollection.Current.GetFolderMemento(Path);
            Restore(memento);
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember(Name = "FolderOrderV2")]
            public FolderOrder FolderOrder { get; set; }

            [DataMember]
            public bool IsFolderRecursive { get; set; }

            public static Memento Default = new Memento();

            public bool IsDefault => (FolderOrder == Default.FolderOrder && IsFolderRecursive == Default.IsFolderRecursive);


            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.FolderOrder = this.FolderOrder;
            memento.IsFolderRecursive = this.IsFolderRecursive;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            // プロパティで設定するとSave()されてしまうのを回避
            _folderOrder = memento.FolderOrder;
            _isFolderRecursive = memento.IsFolderRecursive;
            RaisePropertyChanged(null);
        }

        #endregion
    }
}
