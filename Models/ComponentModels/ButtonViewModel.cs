namespace GameStoreWeb.Models.ComponentModels;
//Keeping temporary in the code for mvc based application, will revert or delete later
public class ButtonViewModel
{
    public string Value { get; set; }
    public string ActionName { get; set; }
    public string ControllerName { get; set; }
    public StyleProps StyleProps { get; set; }
    public OnMouseLeaveProps MouseEnterProps { get; set; }
    public OnMouseLeaveProps MouseLeaveProps { get; set; }
}
public class StyleProps
{
    public string BackgroundColor { get; set; }
    public string Color { get; set; }
    public string On { get; set; }
    // public string OnClick { get; set; }
}
public class OnMouseOverProps
{
    public string BackgroundColor { get; set; }
    public string Color { get; set; }
    // public string OnClick { get; set; }
}
public class OnMouseLeaveProps
{
    public string BackgroundColor { get; set; }
    public string Color { get; set; }
    // public string OnClick { get; set; }
}