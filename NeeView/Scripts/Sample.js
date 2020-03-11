log("[Script sampe]")

isYes = nv.ShowDialog("Is this a pen?", "I think it's a pen, really?", 2)
if (isYes) {
    nv.ShowDialog("Good.")
}
