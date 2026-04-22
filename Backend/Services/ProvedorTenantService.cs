using Backend.Data;

namespace Backend.Services;

/// <summary>
/// Implementação do serviço de provedor de tenant
/// </summary>
public class ProvedorTenantService : IProvedorTenant
{
    private int _tenantIdAtual;

    public ProvedorTenantService()
    {
        _tenantIdAtual = 1; // Tenant padrão
    }

    public int ObterIdTenantAtual()
    {
        return _tenantIdAtual;
    }

    public void DefinirIdTenantAtual(int tenantId)
    {
        _tenantIdAtual = tenantId;
    }
}
