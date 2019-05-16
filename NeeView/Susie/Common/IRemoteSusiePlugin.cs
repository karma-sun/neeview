using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Susie
{
    public interface IRemoteSusiePlugin
    {
        /// <summary>
        /// サーバー設定初期化
        /// </summary>
        void SetServerSetting(SusiePluginServerSetting setting);

        /// <summary>
        /// サーバー設定取得
        /// </summary>
        SusiePluginServerSetting GetServerSetting();


        /// <summary>
        /// プラグインフォルダー設定。プラグインは初期化される
        /// </summary>
        void SetPluginFolder(string pluginFolder);

        /// <summary>
        /// プラグインをキャッシュするかを設定
        /// TODO: プラグイン個別での設定も？事前解凍中は開放させたくない
        /// </summary>
        void SetPluginCahceEnabled(bool isCacheEnabled);

        /// <summary>
        /// プラグイン情報取得
        /// </summary>
        /// <param name="pluginNames">取得するプラグイン名。nullの場合、全プラグイン情報を取得</param>
        List<SusiePluginInfo> GetPlugins(List<string> pluginNames);

        /// <summary>
        /// プラグイン情報設定
        /// </summary>
        /// <param name="settings">プラグイン設定</param>
        void SetPlugins(List<SusiePluginSetting> settings);

        /// <summary>
        /// プラグインの並び順設定
        /// </summary>
        /// <param name="order">プラグイン名リスト</param>
        void SetPluginOrder(List<string> order);

        /// <summary>
        /// 画像プラグイン情報取得
        /// </summary>
        /// <param name="fileName">画像ファイル名</param>
        /// <param name="buff">ヘッダ(2KB)。nullの場合はファイルから読み込む</param>
        /// <param name="isCheckExtension">プラグインに設定されている拡張子でも判定を行う</param>
        /// <returns>対応したプラグイン情報。見つからなければnull</returns>
        SusiePluginInfo GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension);

        /// <summary>
        /// 書庫プラグイン情報取得
        /// </summary>
        /// <param name="fileName">書庫ファイル名</param>
        /// <param name="buff">ヘッダ(2KB)。nullの場合はファイルから読み込む</param>
        /// <param name="isCheckExtension">プラグインに設定されている拡張子でも判定を行う</param>
        /// <returns>対応したプラグイン情報。見つからなければnull</returns>
        SusiePluginInfo GetArchivePlugin(string fileName, byte[] buff, bool isCheckExtension);

        /// <summary>
        /// 設定ダイアログを開く
        /// </summary>
        /// <param name="pluginName">プラグイン名</param>
        /// <param name="hWnd">親のウィンドウハンドル(32bit)</param>
        void ShowConfigulationDlg(string pluginName, int hWnd);

        /// <summary>
        /// 画像取得
        /// </summary>
        /// <param name="fileName">画像ファイル名</param>
        /// <param name="buff">画像データ。nullの場合はファイルから読みk無</param>
        /// <param name="isCheckExtension">プラグインに設定されている拡張子でも判定を行う</param>
        /// <returns>Bitmap画像データ</returns>
        SusiePicture GetPicture(string fileName, byte[] buff, bool isCheckExtension);

        /// <summary>
        /// 書庫エントリー取得
        /// </summary>
        /// <param name="pluginName">書庫プラグイン名</param>
        /// <param name="fileName">書庫ファイル名</param>
        /// <returns>書庫エントリー一覧</returns>
        List<SusieArchiveEntry> GetArchiveEntry(string pluginName, string fileName);

        /// <summary>
        /// 書庫エントリーファイル取得
        /// </summary>
        /// <param name="pluginName">書庫プラグイン名</param>
        /// <param name="fileName">書庫ファイル名</param>
        /// <param name="position">エントリーID</param>
        /// <returns>データ</returns>
        byte[] LoadArchiveEntry(string pluginName, string fileName, uint position);

        /// <summary>
        /// 書庫エントリーファイル出力
        /// </summary>
        /// <param name="pluginName">書庫プラグイン名</param>
        /// <param name="fileName">書庫ファイル名</param>
        /// <param name="position">エントリーID</param>
        /// <param name="extractFolder">出力先フォルダー</param>
        void ExtracArchiveEntrytToFolder(string pluginName, string fileName, uint position, string extractFolder);
    }
}
