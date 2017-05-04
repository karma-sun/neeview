// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{

    /// <summary>
    /// フォルダーの並び順とかの保存される情報
    /// </summary>
    public class FolderCollectionParameter : INotifyPropertyChanged
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
        #region Property: FolderOrder
        private FolderOrder _folderOrder;
        public FolderOrder FolderOrder
        {
            get { return _folderOrder; }
            set { _folderOrder = value; Save(); s_randomSeed = new Random().Next(); RaisePropertyChanged(); }
        }
        #endregion

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// シャッフル用ランダムシード(基準)
        /// </summary>
        private static int s_randomSeed = new Random().Next();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public FolderCollectionParameter(string path)
        {
            Path = path;
            Load();
            RandomSeed = s_randomSeed;
        }

        public void Save()
        {
            ModelContext.BookHistory.SetFolderOrder(Path, _folderOrder);
        }

        private void Load()
        {
            _folderOrder = ModelContext.BookHistory.GetFolderOrder(Path);
        }

        public FolderCollectionParameter Clone()
        {
            return (FolderCollectionParameter)this.MemberwiseClone();
        }
    }

}
