// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public class FolderParameter : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

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
                if (_folderOrder != value)
                {
                    _folderOrder = value;
                    RaisePropertyChanged();
                    Save();
                }
            }
        }

        private FolderOrder _folderOrder;

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// シャッフル用ランダムシード(基準)
        /// </summary>
        private static int _randomSeed = new Random().Next();


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
                    RaisePropertyChanged();
                    Save();
                }

            }
        }

        private bool _isFolderRecursive;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public FolderParameter(string path)
        {
            Path = path;
            Load();
            RandomSeed = _randomSeed;
        }

        //
        private void Save()
        {
            ////ModelContext.BookHistory.SetFolderOrder(Path, _folderOrder);
            ModelContext.BookHistory.SetFolderMemento(Path, CreateMemento());
        }

        //
        private void Load()
        {
            ////_folderOrder = ModelContext.BookHistory.GetFolderOrder(Path);
            var memento = ModelContext.BookHistory.GetFolderMemento(Path);
            Restore(memento);
        }

        /*
        public FolderParameter Clone()
        {
            return (FolderParameter)this.MemberwiseClone();
        }
        */


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public FolderOrder FolderOrder { get; set; }
            [DataMember]
            public bool IsFolderRecursive { get; set; }

            public static Memento Default = new Memento();
            public bool IsDefault => (FolderOrder == Default.FolderOrder && IsFolderRecursive == Default.IsFolderRecursive);
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.FolderOrder = this.FolderOrder;
            memento.IsFolderRecursive = this.IsFolderRecursive;
            return memento;
        }

        //
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
