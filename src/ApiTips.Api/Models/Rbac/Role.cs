namespace ApiTips.Api.Models.Rbac;

public class Role
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<Permission> Permissions { get; set; } = new();
}