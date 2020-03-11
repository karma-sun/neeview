// OnBookLoaded
// この名前のスクリプトは本を開いたときに自動的に呼ばれます。

log("[OnBookLoaded]")

// 例：新しく開いたブックのパスに"English"が含まれていたら左開き、それ以外は右開きにする
if (nv.Book.IsNew) {
    if (nv.Book.Address.match(/English/) != null) {
        nv.Command("SetBookReadOrderLeft").Execute()
    }
    else {
        nv.Command("SetBookReadOrderRight").Execute()
    }
}

// 例：動画ファイルのときに操作方法を変更する
if (nv.Book.IsMedia) {
    // 動画時はクリックで再生/停止、ダブルクリックで最大化
    nv.Command("ToggleMediaPlay").ShortCutKey = "LeftClick"
    nv.Command("ToggleFullScreen").ShortCutKey = "F11"
    nv.Command("Script.MovieDoubleClick").ShortCutKey = "LeftDoubleClick" // TODO: ダブルクリック処理の整備
    nv.Command("NextPage").ShortCutKey = "Left"
}
else {
    // 通常時は標準設定に戻す
    nv.Command("ToggleMediaPlay").ShortCutKey = null
    nv.Command("Script.MovieDoubleClick").ShortCutKey = null
    nv.Command("ToggleFullScreen").ShortCutKey = "F11,LeftDoubleClick"
    nv.Command("NextPage").ShortCutKey = "Left,LeftClick"
}
