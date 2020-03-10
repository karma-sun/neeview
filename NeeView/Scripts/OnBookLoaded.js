// OnBookLoaded：本を読み込んだときに呼ばれるスクリプト

// 例：新しく開くブックのフォルダーのパスに"English"が含まれていたら左開き、それ以外は右開きにする
if (nv.Book.IsNew) {
    if (nv.Book.Address.match(/English/) != null) {
        nv.Command("SetBookReadOrderLeft").Execute()
    }
    else {
        nv.Command("SetBookReadOrderRight").Execute()
    }
}
