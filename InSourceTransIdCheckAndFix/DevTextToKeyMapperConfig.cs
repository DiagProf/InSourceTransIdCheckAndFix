
namespace InSourceTransIdCheckAndFix;

public class DevTextToKeyMapperConfig
{
    public string MapperClassName { get; set; } = "Trs";
    public string ToDoMethodName { get; set; } = "ToDo";
    public string OnMethodName { get; set; } = "On";

    public string DbDevTextManagerImple { get; set; } = "";

    public IDevTextManager DevTextManager { get; internal set; } = null!;
}
