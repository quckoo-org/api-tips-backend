namespace ApiTips.Api.Models.Rbac;

public class Permission
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<Method> Methods { get; set; } = new();
}